using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class LineGraph : MaskableGraphic
{
    [SerializeField]
    private float thicness = 1f;
    [SerializeField]
    private float length = 400f;
    [SerializeField, Tooltip("thicnessの何倍までマイターを許容するか")]
    private float miterLimit = 4f;
    [SerializeField]
    private float[] values = new float[3] { 0f, 200f, 0f };

    private float halfWidth => thicness * 0.5f;

    private List<Vector3> vertices = new List<Vector3>();
    private List<(int, int, int)> indicies = new();


    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        
        var stepX = length / (values.Length - 1f);
        var prev = Vector2.zero;
        var prevBebel = false;
        vertices.Clear();
        indicies.Clear();

        var vertexIndex = 0;
        for (var index = 0; index < values.Length; index++)
        {
            var current = new Vector2(stepX * index, values[index]);
            var isBebel = false;
            if (index == 0)
            {
                ProcessStart(vh, stepX, current);
                prev = current;
                continue;
            }
            if (index == values.Length - 1)
            {
                ProcessEnd(vh, current, prev);
                AddTriangle(vh, vertexIndex, vertexIndex + 1, vertexIndex + 2);
                AddTriangle(vh, vertexIndex + 2, vertexIndex + 1, vertexIndex + 3);
                continue;
            }

            var next = new Vector2(stepX * (index + 1), values[index + 1]);
            var (top, bottom, tmp) = GetJoinVertices(prev, current, next);
            AddVertices(vh, top, bottom);

            if (tmp.HasValue)
            {
                AddVertex(vh, tmp.Value);
                isBebel = true;
            }

            AddTriangles(vh, vertexIndex, (current - prev).normalized, isBebel, prevBebel);
            vertexIndex += isBebel ? 3 : 2;

            prev = current;
            prevBebel = isBebel;
        }
    }

    private void ProcessStart(VertexHelper vh, float stepX, Vector2 current)
    {
        var next = new Vector2(stepX, values[1]);
        var direction = (next - current).normalized;
        var (top, bottom) = GetEndCap(current, direction);
        AddVertices(vh, top, bottom);
    }

    private void ProcessEnd(VertexHelper vh, Vector2 current, Vector2 prev)
    {
        var direction = (current - prev).normalized;
        var (top, bottom) = GetEndCap(current, direction);
        AddVertices(vh, top, bottom);
    }

    private void AddTriangles(VertexHelper vh, int index, Vector2 direction, bool isBebel, bool prevIsBebel)
    {
        if (isBebel)
        {
            AddTriangle(vh, index, index + 1, index + 2);
            AddTriangle(vh, index + 2, index + 1, index + 3);
            AddTriangle(vh, index + 2, index + 4, index + 3);
            return;
        }

        if (direction.y >= 0 || !prevIsBebel)
        {
            AddTriangle(vh, index, index + 1, index + 2);
            AddTriangle(vh, index + 2, index + 1, index + 3);
            return;
        }

        AddTriangle(vh, index, index + 1, index + 2);
        AddTriangle(vh, index + 3, index, index + 2);
    }

    private (Vector2 top, Vector2 bottom, Vector2? nextTop) GetJoinVertices(Vector2 prev, Vector2 current, Vector2 next)
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

    private (Vector2 top, Vector2 bottom) GetEndCap(Vector2 point, Vector2 direction)
    {
        var normal = GetNormal(direction);
        return (point + normal * halfWidth, point - normal * halfWidth);
    }

    private Vector2 GetNormal(Vector2 vector) => new Vector2(-vector.y, vector.x);

    private void AddVertices(VertexHelper vh, params Vector2[] positions)
    {
        foreach (var position in positions)
            AddVertex(vh, position);
    }

    private void AddVertex(VertexHelper vh, Vector2 position)
    {
        var vert = UIVertex.simpleVert;
        vert.position = new Vector3(position.x, position.y);
        vert.color = color;
        vh.AddVert(vert);
        vertices.Add(position);
    }

    private void AddTriangle(VertexHelper vh, int a, int b, int c)
    {
        vh.AddTriangle(a, b, c);
        indicies.Add((a, b, c));
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
