using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEditor;
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
        var prevBebel = false;
        vertices.Clear();
        indicies.Clear();
        var vertexIndex = 0;
        for (var index = 0; index < values.Length; index++)
        {
            var current = new Vector2(stepX * index, values[index]);
            var isBebel = false;
            var next = Vector2.zero;
            var top = Vector2.zero;
            var bottom = Vector2.zero;
            if (index == 0)
            {
                next = new Vector2(stepX * (index + 1), values[index + 1]);
                var direction = (next - current).normalized;
                (top, bottom) = GetEndCap(current, direction, halfWidth);
                AddVertex(vh, top);
                AddVertex(vh, bottom);
                vertices.Add(top);
                vertices.Add(bottom);
                prev = current;
                continue;
            }
            if (index == values.Length - 1)
            {
                var direction = (current - prev).normalized;
                (top, bottom) = GetEndCap(current, direction, halfWidth);
                AddVertex(vh, top);
                AddVertex(vh, bottom);
                vertices.Add(top);
                vertices.Add(bottom);
                vh.AddTriangle(vertexIndex, vertexIndex + 1, vertexIndex + 2);
                vh.AddTriangle(vertexIndex + 2, vertexIndex + 1, vertexIndex + 3);

                indicies.Add((vertexIndex, vertexIndex + 1, vertexIndex + 2));
                indicies.Add((vertexIndex + 2, vertexIndex + 1, vertexIndex + 3));
                continue;
            }

            next = new Vector2(stepX * (index + 1), values[index + 1]);
            Vector2? tmp;
            (top, bottom, tmp) = GetJoinVertices(prev, current, next, halfWidth);
            AddVertex(vh, top);
            AddVertex(vh, bottom);
            vertices.Add(top);
            vertices.Add(bottom);

            if (tmp.HasValue)
            {
                AddVertex(vh, tmp.Value);
                vertices.Add(tmp.Value);
                isBebel = true;
            }

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
                var direction = (current - prev).normalized;
                if (direction.y < 0)
                {
                    if (prevBebel)
                    {
                        vh.AddTriangle(vertexIndex, vertexIndex + 1, vertexIndex + 2);
                        vh.AddTriangle(vertexIndex + 3, vertexIndex, vertexIndex + 2);

                        indicies.Add((vertexIndex, vertexIndex + 1, vertexIndex + 2));
                        indicies.Add((vertexIndex + 3, vertexIndex, vertexIndex + 2));
                    }
                    else
                    {
                        vh.AddTriangle(vertexIndex, vertexIndex + 1, vertexIndex + 2);
                        vh.AddTriangle(vertexIndex + 2, vertexIndex + 1, vertexIndex + 3);

                        indicies.Add((vertexIndex, vertexIndex + 1, vertexIndex + 2));
                        indicies.Add((vertexIndex + 2, vertexIndex + 1, vertexIndex + 3));
                    }
                }
                else
                {
                    vh.AddTriangle(vertexIndex, vertexIndex + 1, vertexIndex + 2);
                    vh.AddTriangle(vertexIndex + 2, vertexIndex + 1, vertexIndex + 3);

                    indicies.Add((vertexIndex, vertexIndex + 1, vertexIndex + 2));
                    indicies.Add((vertexIndex + 2, vertexIndex + 1, vertexIndex + 3));
                }
                vertexIndex += 2;
            }
            prevBebel = isBebel;
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
        for (var index = 0; index < vertices.Count; index++)
        {
            Handles.Label(vertices[index], index.ToString());
        }
        foreach (var index in indicies)
        {
            var vertex = new Span<Vector3>( new Vector3[] { vertices[index.Item1], vertices[index.Item2], vertices[index.Item3] });
            Gizmos.DrawLineStrip(vertex, true);
        }
    }
}
