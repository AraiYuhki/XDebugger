using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class NewLineGraph : MaskableGraphic
{
    [SerializeField]
    private new RectTransform rectTransform;
    
    [SerializeField]
    private float thicness = 1f;
    
    [SerializeField, Tooltip("thicnessの何倍までマイターを許容するか")]
    private float miterLimit = 4f;
    
    [SerializeField]
    private float[] values = new float[] { 0f, 1f, 0f };

    [SerializeField]
    private float minValue = -1f;
    
    [SerializeField]
    private float maxValue = 1f;

    private float[] ConvertValues()
    {
        // クラス変数 values の各値を、下端=minValue、上端=maxValue として
        // rectTransform.rect.height の 0..height に正規化してY座標へ変換する
        var height = rectTransform != null ? rectTransform.rect.height : 0f;
        var vals = values ?? System.Array.Empty<float>();
        var count = vals.Length;
        var ys = new float[count];
        if (count == 0)
            return ys;

        var range = maxValue - minValue;
        if (Mathf.Approximately(range, 0f))
        {
            // 範囲がゼロの場合は中央に配置
            var mid = height * 0.5f;
            for (int i = 0; i < count; i++) ys[i] = mid;
            return ys;
        }

        for (int i = 0; i < count; i++)
        {
            var v = vals[i];
            // min-maxに対して0..1に正規化し、0..heightへスケーリング（上下関係はmaxが上、minが下）
            var t = Mathf.Clamp01((v - minValue) / range);
            ys[i] = t * height;
        }
        return ys;
    }
    
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (rectTransform == null)
            return;

        var ys = ConvertValues();
        var count = ys.Length;
        if (count < 2)
            return;

        var stepX = rectTransform.rect.width / (count - 1f);
        var halfWidth = thicness * 0.5f;
        var prev = Vector2.zero;
        var vertexIndex = 0;
        for (var index = 0; index < count; index++)
        {
            var current = new Vector2(stepX * index, ys[index]);
            var isBebel = false;
            if (index == 0)
            {
                var next = new Vector2(stepX * (index + 1), ys[index + 1]);
                var direction = (next - current).normalized;
                var (top, bottom) = GetEndCap(current, direction, halfWidth);
                AddVertex(vh, top);
                AddVertex(vh, bottom);
            }
            else if (index == count - 1)
            {
                var direction = (current - prev).normalized;
                var (top, bottom) = GetEndCap(current, direction, halfWidth);
                if (direction.y < 0)
                {
                    AddVertex(vh, bottom);
                    AddVertex(vh, top);
                }
                else
                {
                    AddVertex(vh, top);
                    AddVertex(vh, bottom);
                }
            }
            else
            {
                var next = new Vector2(stepX * (index + 1), ys[index + 1]);
                var (top, bottom, tmp) = GetJoinVertices(prev, current, next, halfWidth);
                AddVertex(vh, top);
                AddVertex(vh, bottom);

                if (tmp.HasValue)
                {
                    AddVertex(vh, tmp.Value);
                    isBebel = true;
                }

            }
            if (index > 0)
            {
                if (isBebel)
                {
                    vh.AddTriangle(vertexIndex, vertexIndex + 1, vertexIndex + 2);
                    vh.AddTriangle(vertexIndex + 2, vertexIndex + 1, vertexIndex + 3);
                    vh.AddTriangle(vertexIndex + 2, vertexIndex + 4, vertexIndex + 3);
                    vertexIndex += 3;
                }
                else
                {
                    vh.AddTriangle(vertexIndex, vertexIndex + 1, vertexIndex + 2);
                    vh.AddTriangle(vertexIndex + 2, vertexIndex + 1, vertexIndex + 3);
                    vertexIndex += 2;
                }
            }
            prev = current;
        }
    }

    private (Vector2 top, Vector2 bottom, Vector2? nextTop)
        GetJoinVertices(Vector2 prev, Vector2 current, Vector2 next, float halfWidth)
    {
        var prevDirection = (current - prev).normalized;
        var nextDirection = (next - current).normalized;
        var prevNormal = GetNormal(prevDirection);
        var nextNormal = GetNormal(nextDirection);

        var miterDirection = (prevNormal + nextNormal).normalized;
        var dotToNextNormal = Vector2.Dot(miterDirection, nextNormal);

        if (Mathf.Abs(dotToNextNormal) < float.Epsilon)
        {
            return (current + nextNormal * halfWidth, current - nextNormal * halfWidth, null);
        }

        var miterLength = halfWidth / dotToNextNormal;

        if (Mathf.Abs(miterLength) > (halfWidth * Mathf.Max(1f, miterLimit)))
        {
            var currentNormal = prevDirection.y > 0 ? prevNormal : -prevNormal;
            var top = current + currentNormal * halfWidth;
            var bottom = current - currentNormal * halfWidth;
            var diff = top - bottom;
            diff.x *= -1;
            var tmp = bottom + diff;
            var positions = (top, bottom, tmp);
            return positions;
        }

        return (current + miterDirection * miterLength, current - miterDirection * miterLength, null);
    }

    private (Vector2 top, Vector2 bottom) GetEndCap(Vector2 point, Vector2 direction, float halfWidth)
    {
        var normal = GetNormal(direction);
        return (point + normal * halfWidth, point - normal * halfWidth);
    }

    private Vector2 GetNormal(Vector2 vector) => new Vector2(-vector.y, vector.x);

    private void AddVertex(VertexHelper vh, Vector2 position)
    {
        var vert = UIVertex.simpleVert;
        vert.position = new Vector3(position.x, position.y);
        vert.color = color;
        vh.AddVert(vert);
    }
}
