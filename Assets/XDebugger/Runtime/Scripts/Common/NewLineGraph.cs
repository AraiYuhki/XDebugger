using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class NewLineGraph : MaskableGraphic
{
    [SerializeField]
    private float thicness = 1f;
    [SerializeField]
    private float length = 400f;
    [SerializeField, Tooltip("thicnessの何倍までマイターを許容するか")]
    private float miterLimit = 4f;
    [SerializeField]
    private float[] values = new float[3] { 0f, 200f, 0f };

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        var stepX = length / (values.Length - 1f);
        var halfWidth = thicness * 0.5f;
        var prev = Vector2.zero;
        for (var index = 0; index < values.Length; index++)
        {
            var current = new Vector2(stepX * index, values[index]);
            if (index == 0)
            {
                var next = new Vector2(stepX * (index + 1), values[index + 1]);
                var direction = (next - current).normalized;
                var (top, bottom) = GetEndCap(current, direction, halfWidth);
                AddVertex(vh, top);
                AddVertex(vh, bottom);
            }
            else if (index == values.Length - 1)
            {
                var direction = (current - prev).normalized;
                var (top, bottom) = GetEndCap(current, direction, halfWidth);
                AddVertex(vh, top);
                AddVertex(vh, bottom);
            }
            else
            {
                var next = new Vector2(stepX * (index + 1), values[index + 1]);
                var (top, bottom) = GetJoinVertices(prev, current, next, halfWidth);
                AddVertex(vh, top);
                AddVertex(vh, bottom);
            }
            prev = current;
        }

        for (var index = 0; index < values.Length - 1; index++)
        {
            var vertexIndex = index * 2;
            vh.AddTriangle(vertexIndex, vertexIndex + 1, vertexIndex + 2);
            vh.AddTriangle(vertexIndex + 2, vertexIndex + 1, vertexIndex + 3);
        }
    }

    private (Vector2 top, Vector2 bottom) GetJoinVertices(Vector2 prev, Vector2 current, Vector2 next, float halfWidth)
    {
        var prevDirection = (current - prev).normalized;
        var nextDirection = (next - current).normalized;
        var prevNormal = GetNormal(prevDirection);
        var nextNormal = GetNormal(nextDirection);

        var miterDirection = (prevNormal + nextNormal).normalized;
        var dotToNextNormal = Vector2.Dot(miterDirection, nextNormal);

        if (Mathf.Abs(dotToNextNormal) < float.Epsilon)
        {
            return (current + nextNormal * halfWidth, current - nextNormal * halfWidth);
        }

        var miterLength = halfWidth / dotToNextNormal;

        return (current + miterDirection * miterLength, current - miterDirection * miterLength);
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
