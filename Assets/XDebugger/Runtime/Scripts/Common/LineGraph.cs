using System;
using UnityEngine;
using UnityEngine.UI;

public class LineGraph : MaskableGraphic
{
    [SerializeField] private float[] values = new float[0];

    [SerializeField] private float thickness = 0.5f; // 半幅（この値の2倍が実線幅）
    [SerializeField] private float length = 400f; // X 方向の合計長

    // ミター（角のとがり）について:
    // - 折れ線の角で、外側の辺を延長して尖らせてつなぐ方法のことです。
    // - 角が鋭いと先端が伸びすぎて“トゲ”のようになり、見た目が崩れます。
    // - miterLimit でこの尖りの長さの上限を決め、超えた場合は角を斜めに切る（ベベル）方法に切り替えて抑えます。
    // - 計算上のミター長は thickness / dot(miterDirection, nextNormal) で求めます（thickness は線の半幅）。
    [SerializeField] private float miterLimit = 4f; // thickness の何倍までミターを許容

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (values == null || values.Length < 2)
        {
            return;
        }

        int pointCount = values.Length;
        float stepX = pointCount > 1 ? length / (pointCount - 1) : 0f;

        // 中心線の点配列
        var points = new Vector2[pointCount];
        ComputeCenterlinePoints(pointCount, stepX, values, points);

        // 上辺/下辺の頂点配列
        var topVertices = new Vector2[pointCount];
        var bottomVertices = new Vector2[pointCount];
        ComputeStripVertices(points, thickness, miterLimit, topVertices, bottomVertices);

        // 頂点と三角形を追加
        AppendStrip(vh, topVertices, bottomVertices);
    }

    // values から中心線の 2D 点を等間隔に生成
    private void ComputeCenterlinePoints(int pointCount, float stepX, float[] srcValues, Vector2[] outPoints)
    {
        for (int i = 0; i < pointCount; i++)
        {
            outPoints[i] = new Vector2(stepX * i, srcValues[i]);
        }
    }

    // 上下のストリップ頂点を計算（ミターを用いて角を自然に接続）
    private void ComputeStripVertices(Vector2[] points, float halfWidth, float miterLimitMul, Vector2[] outTop, Vector2[] outBottom)
    {
        int n = points.Length;
        for (int i = 0; i < n; i++)
        {
            if (i == 0)
            {
                // 先頭点は次点方向の法線で幅を出す
                var dir = SafeNormalize(points[1] - points[0]);
                ComputeEndCap(points, i, dir, halfWidth, outTop, outBottom);
                continue;
            }

            if (i == n - 1)
            {
                // 末尾点は前点方向の法線で幅を出す
                var dir = SafeNormalize(points[i] - points[i - 1]);
                ComputeEndCap(points, i, dir, halfWidth, outTop, outBottom);
                continue;
            }

            // 中間点はミター結合（必要に応じてベベルへフォールバック）
            ComputeJoinVertices(points, i, halfWidth, miterLimitMul, outTop, outBottom);
        }
    }

    // 端点の上下頂点を計算
    private void ComputeEndCap(Vector2[] points, int index, Vector2 direction, float halfWidth, Vector2[] outTop, Vector2[] outBottom)
    {
        var normal = Perpendicular(direction);
        outTop[index] = points[index] + normal * halfWidth;
        outBottom[index] = points[index] - normal * halfWidth;
    }

    // 中間点の上下頂点を計算（ミター＋リミット、必要ならベベル）
    private void ComputeJoinVertices(Vector2[] points, int index, float halfWidth, float miterLimitMul, Vector2[] outTop, Vector2[] outBottom)
    {
        var prevDir = SafeNormalize(points[index] - points[index - 1]);
        var nextDir = SafeNormalize(points[index + 1] - points[index]);
        var prevNormal = Perpendicular(prevDir);
        var nextNormal = Perpendicular(nextDir);

        // ミターの計算と制御
        var miterDir = SafeNormalize(prevNormal + nextNormal);
        float dotToNextNormal = Vector2.Dot(miterDir, nextNormal);

        if (Mathf.Abs(dotToNextNormal) < 1e-3f)
        {
            // ほぼ直線/鋭角で不安定 → 次の法線で固定
            outTop[index] = points[index] + nextNormal * halfWidth;
            outBottom[index] = points[index] - nextNormal * halfWidth;
            return;
        }

        float miterLen = halfWidth / dotToNextNormal;
        float maxAllowed = halfWidth * Mathf.Max(1f, miterLimitMul);

        if (Mathf.Abs(miterLen) > maxAllowed)
        {
            // スパイク防止: ベベルへフォールバック
            var normal = dotToNextNormal > 0f ? nextNormal : -nextNormal;
            outTop[index] = points[index] + normal * halfWidth;
            outBottom[index] = points[index] - normal * halfWidth;
            return;
        }

        outTop[index] = points[index] + miterDir * miterLen;
        outBottom[index] = points[index] - miterDir * miterLen;
    }

    // 上下ストリップを UI 頂点へ追加し、隣接クワッドを三角形で張る
    private void AppendStrip(VertexHelper vh, Vector2[] top, Vector2[] bottom)
    {
        int n = top.Length;

        // 頂点追加（上→下の順）
        for (int i = 0; i < n; i++)
        {
            AddVertex(vh, new Vector3(top[i].x, top[i].y, 0f));
            AddVertex(vh, new Vector3(bottom[i].x, bottom[i].y, 0f));
        }

        // クワッドを二枚の三角形で構成
        for (int i = 0; i < n - 1; i++)
        {
            int vi = i * 2;
            vh.AddTriangle(vi, vi + 1, vi + 2);
            vh.AddTriangle(vi + 2, vi + 1, vi + 3);
        }
    }

    // 値更新と再描画
    public void SetValues(float[] newValues)
    {
        values = newValues ?? Array.Empty<float>();
        SetVerticesDirty();
    }

    // ベクトルの法線（左90度回転）
    private static Vector2 Perpendicular(Vector2 v) => new Vector2(-v.y, v.x);

    // 安全な正規化（ゼロ長のときは右方向を返す）
    private static Vector2 SafeNormalize(Vector2 v)
    {
        float m = v.magnitude;
        return m > 1e-6f ? v / m : Vector2.right;
    }

    private void AddVertex(VertexHelper vh, Vector3 pos)
    {
        var vert = UIVertex.simpleVert;
        vert.position = pos;
        vert.color = color;
        vh.AddVert(vert);
    }
}
