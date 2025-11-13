using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class LineRendererForUGUIBevel : MaskableGraphic
{
    [SerializeField] private Sprite sprite;
    [SerializeField] private Vector2[] points = new Vector2[0]; // ローカル座標系になるので注意
    [SerializeField] private float thicness = 1f;

    public override Texture mainTexture => sprite == null ? null : sprite.texture;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (points == null || points.Length < 2)
            return;

        var vertexIndex = 0;
        var halfThickness = thicness * 0.5f;
        var prevIsLeftTurn = false;
        var vertex = UIVertex.simpleVert;
        vertex.color = color;
        for (var index = 0; index < points.Length; index++)
        {
            if (index == 0)
            {
                ProcessStartPoint(vh, index, halfThickness);
                continue;
            }

            if (index == points.Length - 1)
            {
                ProcessEndPoint(vh, index, vertexIndex, halfThickness, prevIsLeftTurn);
                continue;
            }

            var currentPoint = points[index];
            var prevPoint = points[index - 1];
            var nextPoint = points[index + 1];

            // 方向ベクトル
            var directionPrev = (currentPoint - prevPoint).normalized;
            var directionNext = (nextPoint - currentPoint).normalized;
            // 各方向の法線
            var normalPrev = new Vector2(directionPrev.y, -directionPrev.x);
            var normalNext = new Vector2(directionNext.y, -directionNext.x);
            // 左折判定
            var isLeftTurn = Cross(directionPrev, directionNext) > 0; // 左折か？

            // 外側法線（前方向/次方向）
            var outerPrevNormal = isLeftTurn ? normalPrev : -normalPrev;
            var outerNextNormal = isLeftTurn ? normalNext : -normalNext;
            // 外側頂点
            var outerPrev = currentPoint + outerPrevNormal * halfThickness;
            var outerNext = currentPoint + outerNextNormal * halfThickness;

            // 内側法線
            var innerPrevNormal = -outerPrevNormal;
            var innerNextNormal = -outerNextNormal;
            // 内側線のオフセット基点
            var innerPrevOffsetPoint = currentPoint + innerPrevNormal * halfThickness;
            var innerNextOffsetPoint = currentPoint + innerNextNormal * halfThickness;

            // 交点（内側同士）
            var innerIntersection = Intersection(innerPrevOffsetPoint, directionPrev, innerNextOffsetPoint,
                directionNext, out var intersectionFound);
            if (!intersectionFound)
            {
                innerIntersection = currentPoint;
            }

            vertex.position = outerPrev; // 前セグメント側外頂点
            vh.AddVert(vertex);

            vertex.position = innerIntersection; // 内側交点
            vh.AddVert(vertex);

            vertex.position = outerNext; // 次セグメント側外頂点
            vh.AddVert(vertex);

            if (!isLeftTurn)
            {
                if (!prevIsLeftTurn)
                {
                    vh.AddTriangle(vertexIndex, vertexIndex + 1, vertexIndex + 2);
                    vh.AddTriangle(vertexIndex, vertexIndex + 2, vertexIndex + 3);
                    vh.AddTriangle(vertexIndex + 2, vertexIndex + 3, vertexIndex + 4);
                }
                else
                {
                    vh.AddTriangle(vertexIndex, vertexIndex + 1, vertexIndex + 2);
                    vh.AddTriangle(vertexIndex + 1, vertexIndex + 2, vertexIndex + 3);
                    vh.AddTriangle(vertexIndex + 2, vertexIndex + 3, vertexIndex + 4);
                }
            }
            else
            {
                if (!prevIsLeftTurn)
                {
                    vh.AddTriangle(vertexIndex, vertexIndex + 1, vertexIndex + 2);
                    vh.AddTriangle(vertexIndex + 1, vertexIndex + 2, vertexIndex + 3);
                    vh.AddTriangle(vertexIndex + 2, vertexIndex + 3, vertexIndex + 4);
                }
                else
                {
                    vh.AddTriangle(vertexIndex, vertexIndex + 1, vertexIndex + 2);
                    vh.AddTriangle(vertexIndex, vertexIndex + 2, vertexIndex + 3);
                    vh.AddTriangle(vertexIndex + 2, vertexIndex + 3, vertexIndex + 4);
                }
            }

            vertexIndex += 3;
            prevIsLeftTurn = isLeftTurn;
        }
    }

    private void ProcessStartPoint(VertexHelper vh, int index, float halfThickness)
    {
        var currentPoint = points[index];
        var nextPoint = points[index + 1];
        var direction = (nextPoint - currentPoint).normalized;
        var normal = new Vector2(direction.y, -direction.x);
        var vertex = UIVertex.simpleVert;
        vertex.color = color;

        vertex.position = currentPoint + normal * halfThickness;
        vh.AddVert(vertex);

        vertex.position = currentPoint - normal * halfThickness;
        vh.AddVert(vertex);
    }

    private void ProcessEndPoint(VertexHelper vh, int index, int vertexIndex, float halfThickness, bool prevIsCcw)
    {
        var currentPoint = points[index];
        var prevPoint = points[index - 1];
        var direction = (currentPoint - prevPoint).normalized;
        var normal = new Vector2(direction.y, -direction.x);

        var vertex = UIVertex.simpleVert;
        vertex.color = color;
        vertex.position = currentPoint + normal * halfThickness;
        vh.AddVert(vertex);

        vertex.position = currentPoint - normal * halfThickness;
        vh.AddVert(vertex);

        if (points.Length == 2)
        {
            vh.AddTriangle(vertexIndex, vertexIndex + 2, vertexIndex + 1);
            vh.AddTriangle(vertexIndex + 1, vertexIndex + 2, vertexIndex + 3);
        }
        else
        {
            if (!prevIsCcw)
            {
                vh.AddTriangle(vertexIndex, vertexIndex + 1, vertexIndex + 2);
                vh.AddTriangle(vertexIndex + 1, vertexIndex + 2, vertexIndex + 3);
            }
            else
            {
                vh.AddTriangle(vertexIndex, vertexIndex + 1, vertexIndex + 2);
                vh.AddTriangle(vertexIndex, vertexIndex + 2, vertexIndex + 3);
            }
        }
    }

    private Vector2 Intersection(Vector2 prevPoint, Vector2 directionPrev, Vector2 nextPoint, Vector2 directionNext,
        out bool found)
    {
        // 二つの半直線 (originA + t * directionA) と (originB + u * directionB) の交点を求める
        // directionA / directionB は正規化されている前提ではないがクロス判定のみなので不要
        var crossDirection = Cross(directionPrev, directionNext); // 方向ベクトル同士の外積（平行判定）
        if (Mathf.Abs(crossDirection) < float.Epsilon)
        {
            found = false; // 平行もしくは同一直線で交点が一意に定まらない
            return default;
        }

        var deltaOrigins = nextPoint - prevPoint; // 始点間ベクトル
        var tAlongA = Cross(deltaOrigins, directionNext) / crossDirection; // originA から交点までのスカラー値
        found = true;
        return prevPoint + directionPrev * tAlongA;
    }

    /// <summary>
    /// 外積
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;
#if UNITY_EDITOR
    [CustomEditor(typeof(LineRendererForUGUIBevel))]
    private class LineRendererForUGUIBevelEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("サンプル作成"))
            {
                var length = Random.Range(3, 10);
                var points = new Vector2[length];
                for (var index = 0; index < length; index++)
                {
                    points[index] = new Vector2(Random.Range(-100f, 100f), Random.Range(-100f, 100f));
                }

                if (target is LineRendererForUGUIBevel lineRenderer)
                {
                    lineRenderer.points = points;
                    lineRenderer.SetVerticesDirty();
                }
            }
        }
    }
#endif
}