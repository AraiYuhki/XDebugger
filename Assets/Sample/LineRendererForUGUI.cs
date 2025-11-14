using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(CanvasRenderer))]
public class LineRendererForUGUI : MaskableGraphic
{
    [SerializeField] private Sprite sprite;
    [SerializeField] private Vector2[] points = new Vector2[0]; // ローカル座標系になるので注意
    [SerializeField] private float thickness = 1f; // 線の太さ
    [SerializeField] private float miterLimit = 4f; // マイター（角の突出）の最大長

    /// <summary>セグメントの長さが短すぎる場合の閾値（線の太さに依存）</summary>
    private float SegmentEpsilon => Mathf.Max(0.001f, thickness * 0.05f);

    /// <summary>2つのセグメントが平行かどうか判定するための閾値</summary>
    private float ParallelEpsilon(float lengthA, float lengthB) => 1e-5f * (lengthA * lengthB + 1f);

    /// <summary>交点計算時の後退許容値（線の太さに依存）</summary>
    private float BackwardTolerance => Mathf.Max(0.00015f, thickness * 0.02f);

    /// <summary>
    /// 使用するテクスチャ（spriteが設定されていればそのテクスチャ、なければnull）
    /// </summary>
    public override Texture mainTexture => sprite == null ? null : sprite.texture;

    private readonly List<UIVertex> vertices = new(); // 頂点リスト
    private readonly List<int> indices = new(); // インデックスリスト

    private UIVertex vertex;

    /// <summary>
    /// メッシュ生成処理（頂点・インデックスを計算してUIに描画）
    /// </summary>
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        vertices.Clear();
        indices.Clear();
        // 座標が線を引くのに足りない場合は処理を行わない
        if (points == null || points.Length < 2)
            return;

        var vertexIndex = 0;
        var halfThickness = thickness / 2f;
        var isPrevBevel = false;
        var isPrevTurnLeft = false;
        vertex = UIVertex.simpleVert;
        vertex.color = color;
        for (var index = 0; index < points.Length; index++)
        {
            var pointData = new LineJoinContext(index, points);
            // 先頭
            if (pointData.IsFirst)
            {
                ProcessStartPoint(pointData, halfThickness);
                continue;
            }

            // 終端
            if (pointData.IsLast)
            {
                ProcessEndPoint(pointData, vertexIndex, halfThickness, isPrevBevel, isPrevTurnLeft);
                continue;
            }

            // 端以外の処理
            isPrevBevel = ProcessMiddlePoint(pointData, vertexIndex, halfThickness, isPrevBevel, isPrevTurnLeft);
            isPrevTurnLeft = pointData.IsLeftTurn;

            // 中間点は共通で使用するので、2つずつインデックスを進める
            vertexIndex += isPrevBevel ? 3 : 2;
        }

        vh.AddUIVertexStream(vertices, indices);
    }

    /// <summary>
    /// 線の始点の頂点を計算・追加
    /// </summary>
    private void ProcessStartPoint(LineJoinContext pointData, float halfThickness)
    {
        vertex.position = pointData.Current + pointData.NextNormal * halfThickness;
        vertices.Add(vertex);

        vertex.position = pointData.Current - pointData.NextNormal * halfThickness;
        vertices.Add(vertex);
    }

    /// <summary>
    /// 線の終点の頂点を計算・追加
    /// </summary>
    private void ProcessEndPoint(LineJoinContext pointData, int vertexIndex, float halfThickness, bool isPrevBevel,
        bool isPrevLeftTurn)
    {
        vertex.position = pointData.Current + pointData.PrevNormal * halfThickness;
        vertices.Add(vertex);

        vertex.position = pointData.Current - pointData.PrevNormal * halfThickness;
        vertices.Add(vertex);
        AddTrianglesForEnd(vertexIndex, isPrevBevel, isPrevLeftTurn);
    }

    /// <summary>
    /// 線の中間点（角部分）の頂点を計算・追加
    /// </summary>
    private bool ProcessMiddlePoint(LineJoinContext pointData, int vertexIndex, float halfThickness,
        bool isPrevBevel, bool isPrevLeftTurn)
    {
        var miterDirection = pointData.MiterVector;
        // Miter 原始ベクトルがゼロに近い場合はベベルへフォールバック
        if (miterDirection.sqrMagnitude < 1e-8f)
        {
            ProcessBevel(pointData, vertexIndex, halfThickness, isPrevLeftTurn);
            return true;
        }

        // マイター突出長の算出
        var dotToNextNormal = Vector2.Dot(miterDirection, pointData.NextNormal);
        // 分母が極小のときは不安定なのでベベルへ
        const float dotEps = 1e-4f;
        if (Mathf.Abs(dotToNextNormal) < dotEps)
        {
            ProcessBevel(pointData, vertexIndex, halfThickness, isPrevLeftTurn);
            return true;
        }

        var miterLength = halfThickness / dotToNextNormal;

        // マイター長が許容範囲外ならベベル
        if (Mathf.Abs(miterLength) > halfThickness * Mathf.Max(1f, miterLimit))
        {
            ProcessBevel(pointData, vertexIndex, halfThickness, isPrevLeftTurn);
            return true;
        }

        // 使用するのはマイター方向とマイター突出長を使用して頂点位置を算出する
        vertex.position = pointData.Current + miterDirection * miterLength;
        vertices.Add(vertex);

        vertex.position = pointData.Current - miterDirection * miterLength;
        vertices.Add(vertex);
        if (!isPrevBevel)
        {
            AddTrianglesForMiter(vertexIndex, false, isPrevLeftTurn);
        }
        else
        {
            AddTrianglesForMiter(vertexIndex, true, isPrevLeftTurn);
        }

        return false;
    }

    /// <summary>
    /// ベベル（角の丸め処理）を行い、頂点を追加
    /// </summary>
    private void ProcessBevel(LineJoinContext pointData, int vertexIndex, float halfThickness, bool prevIsLeftTurn)
    {
        var isLeftTurn = pointData.IsLeftTurn;

        // 外側法線（方向ベクトルではなく法線を使用）
        var outerPrevNormal = isLeftTurn ? pointData.PrevNormal : -pointData.PrevNormal;
        var outerNextNormal = isLeftTurn ? pointData.NextNormal : -pointData.NextNormal;

        // 外側頂点
        var outerPrev = pointData.Current + outerPrevNormal * halfThickness;
        var outerNext = pointData.Current + outerNextNormal * halfThickness;

        // 内側法線
        var innerPrevNormal = -outerPrevNormal;
        var innerNextNormal = -outerNextNormal;

        var innerPrevOffsetPoint = pointData.Current + innerPrevNormal * halfThickness;
        var innerNextOffsetPoint = pointData.Current + innerNextNormal * halfThickness;

        // 交点（内側同士）
        var innerIntersection = Intersection(pointData, innerPrevOffsetPoint, innerNextOffsetPoint,
            out var intersectionFound);
        if (!intersectionFound)
        {
            // フォールバック: currentPoint ではなく 2 オフセットの中点で段差緩和
            innerIntersection = (innerPrevOffsetPoint + innerNextOffsetPoint) * 0.5f;
        }
        
        vertex.position = outerPrev;
        vertices.Add(vertex);
        vertex.position = innerIntersection;
        vertices.Add(vertex);
        vertex.position = outerNext;
        vertices.Add(vertex);

        // 三角形構築（左右で若干の貼り方差異）
        if (!isLeftTurn)
        {
            AddTrianglesForBevel(vertexIndex, prevIsLeftTurn, false);
        }
        else
        {
            AddTrianglesForBevel(vertexIndex, prevIsLeftTurn, true);
        }

        AddFinalBevelTriangle(vertexIndex);
    }

    /// <summary>
    /// 2つの線分の交点を計算
    /// </summary>
    private Vector2 Intersection(LineJoinContext pointData, Vector2 prevPoint, Vector2 nextPoint, out bool found)
    {
        if (pointData.PrevLength < SegmentEpsilon || pointData.NextLength < SegmentEpsilon)
        {
            Debug.LogWarning("セグメントの長さが短すぎます。フォールバック頂点を使用します。", this);
            found = false;
            return default;
        }

        var crossDirection = Cross(pointData.PrevDirection, pointData.NextDirection);
        if (Mathf.Abs(crossDirection) < ParallelEpsilon(pointData.PrevLength, pointData.NextLength))
        {
            Debug.LogWarning("セグメントが平行もしくは同一直線です。フォールバック頂点を使用します。", this);
            found = false;
            return default;
        }

        var deltaOrigins = nextPoint - prevPoint;
        var tAlongA = Cross(deltaOrigins, pointData.NextDirection) / crossDirection;

        // 範囲判定を PrevLength に訂正（進む方向は PrevDirection）
        if (tAlongA < -BackwardTolerance || tAlongA > pointData.PrevLength + BackwardTolerance)
        {
            found = false;
            Debug.LogWarning("幾何的縮退を検知しました。フォールバック頂点を使用します。", this);
            return default;
        }

        found = true;
        return prevPoint + pointData.PrevDirection * tAlongA;
    }

    /// <summary>
    /// 外積計算（2Dベクトル用）
    /// </summary>
    private float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;

    /// <summary>
    /// 三角形をインデックスリストに追加
    /// </summary>
    private void AddTriangle(int a, int b, int c)
    {
        indices.Add(a);
        indices.Add(b);
        indices.Add(c);
    }

    private void AddTrianglesForMiter(int baseIndex, bool prevWasBevel, bool prevLeftTurn)
    {
        if (!prevWasBevel)
        {
            // 通常 -> 通常
            AddTriangle(baseIndex, baseIndex + 2, baseIndex + 1);
            AddTriangle(baseIndex + 1, baseIndex + 2, baseIndex + 3);
            return;
        }

        // ベベル -> 通常 (前ベベルの左折/右折で貼り分け)
        if (prevLeftTurn)
        {
            AddTriangle(baseIndex, baseIndex + 2, baseIndex + 1);
            AddTriangle(baseIndex, baseIndex + 2, baseIndex + 3);
        }
        else
        {
            AddTriangle(baseIndex, baseIndex + 1, baseIndex + 2);
            AddTriangle(baseIndex + 1, baseIndex + 2, baseIndex + 3);
        }
    }

    private void AddTrianglesForBevel(int baseIndex, bool prevLeftTurn, bool currentLeftTurn)
    {
        // currentLeftTurn で左右を入れ替えるが分岐パターンは左右対称
        if (!currentLeftTurn)
        {
            if (!prevLeftTurn)
            {
                AddTriangle(baseIndex, baseIndex + 1, baseIndex + 2);
                AddTriangle(baseIndex, baseIndex + 2, baseIndex + 3);
            }
            else
            {
                AddTriangle(baseIndex, baseIndex + 1, baseIndex + 2);
                AddTriangle(baseIndex + 1, baseIndex + 2, baseIndex + 3);
            }
        }
        else
        {
            if (!prevLeftTurn)
            {
                AddTriangle(baseIndex, baseIndex + 1, baseIndex + 2);
                AddTriangle(baseIndex + 1, baseIndex + 2, baseIndex + 3);
            }
            else
            {
                AddTriangle(baseIndex, baseIndex + 1, baseIndex + 2);
                AddTriangle(baseIndex, baseIndex + 2, baseIndex + 3);
            }
        }
    }

    private void AddFinalBevelTriangle(int baseIndex)
    {
        AddTriangle(baseIndex + 2, baseIndex + 3, baseIndex + 4);
    }

    private void AddTrianglesForEnd(int baseIndex, bool prevWasBevel, bool prevLeftTurn)
    {
        if (!prevWasBevel)
        {
            AddTriangle(baseIndex, baseIndex + 2, baseIndex + 1);
            AddTriangle(baseIndex + 1, baseIndex + 2, baseIndex + 3);
            return;
        }

        if (!prevLeftTurn)
        {
            AddTriangle(baseIndex, baseIndex + 1, baseIndex + 2);
            AddTriangle(baseIndex + 1, baseIndex + 2, baseIndex + 3);
        }
        else
        {
            AddTriangle(baseIndex, baseIndex + 1, baseIndex + 2);
            AddTriangle(baseIndex, baseIndex + 2, baseIndex + 3);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(LineRendererForUGUI))]
    private class LineRendererForUGUIEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("サンプル作成"))
            {
                var length = Random.Range(3, 10);
                var pts = new Vector2[length];
                for (var idx = 0; idx < length; idx++)
                {
                    pts[idx] = new Vector2(Random.Range(-100f, 100f), Random.Range(-100f, 100f));
                }

                if (target is LineRendererForUGUI lr)
                {
                    lr.points = pts;
                    lr.SetVerticesDirty();
                }
            }
        }
    }
#endif
}
