using NUnit.Framework;
using System;
using System.Collections.Generic;
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

    private List<Vector3> vertices = new List<Vector3>();
    private List<(int, int, int)> indicies = new();


    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        var stepX = length / (values.Length - 1f);
        var halfWidth = thicness * 0.5f;
        var prev = Vector2.zero;
        vertices.Clear();
        indicies.Clear();
        var vertexIndex = 0;
        for (var index = 0; index < values.Length; index++)
        {
            var current = new Vector2(stepX * index, values[index]);
            var isBebel = false;
            if (index == 0)
            {
                var next = new Vector2(stepX * (index + 1), values[index + 1]);
                var direction = (next - current).normalized;
                var (top, bottom) = GetEndCap(current, direction, halfWidth);
                AddVertex(vh, top);
                AddVertex(vh, bottom);
                vertices.Add(top);
                vertices.Add(bottom);
            }
            else if (index == values.Length - 1)
            {
                var direction = (current - prev).normalized;
                var (top, bottom) = GetEndCap(current, direction, halfWidth);
                if (direction.y < 0)
                {
                    AddVertex(vh, bottom);
                    AddVertex(vh, top);
                    vertices.Add(bottom);
                    vertices.Add(top);
                }
                else
                {
                    AddVertex(vh, top);
                    AddVertex(vh, bottom);
                    vertices.Add(top);
                    vertices.Add(bottom);
                }
            }
            else
            {
                var next = new Vector2(stepX * (index + 1), values[index + 1]);
                var (top, bottom, tmp) = GetJoinVertices(prev, current, next, halfWidth);
                AddVertex(vh, top);
                vertices.Add(top);
                AddVertex(vh, bottom);
                vertices.Add(bottom);

                if (tmp.HasValue)
                {
                    AddVertex(vh, tmp.Value);
                    vertices.Add(tmp.Value);
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

                    indicies.Add((vertexIndex, vertexIndex + 1, vertexIndex + 2));
                    indicies.Add((vertexIndex + 2, vertexIndex + 1, vertexIndex + 3));
                    indicies.Add((vertexIndex + 2, vertexIndex + 4, vertexIndex + 3));
                    vertexIndex += 3;
                }
                else
                {
                    vh.AddTriangle(vertexIndex, vertexIndex + 1, vertexIndex + 2);
                    vh.AddTriangle(vertexIndex + 2, vertexIndex + 1, vertexIndex + 3);

                    indicies.Add((vertexIndex, vertexIndex + 1, vertexIndex + 2));
                    indicies.Add((vertexIndex + 2, vertexIndex + 1, vertexIndex + 3));
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

    private void OnDrawGizmos()
    {
        foreach (var index in indicies)
        {
            var vertex = new Span<Vector3>( new Vector3[] { vertices[index.Item1], vertices[index.Item2], vertices[index.Item3] });
            Gizmos.DrawLineStrip(vertex, true);
        }
    }
}
