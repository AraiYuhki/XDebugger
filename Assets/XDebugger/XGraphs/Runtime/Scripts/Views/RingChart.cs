using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Xeon.XGraph.Model;

namespace Xeon.XGraph.View
{
    [RequireComponent(typeof(CanvasRenderer)), ExecuteInEditMode]
    public class RingChart : ChartBase
    {
        private const float StartAngleOffset = 90f;

        [SerializeField, Range(0f, 359f)] private float startAngle = 0f;
        [SerializeField] private float innerRadius = 5f;
        [SerializeField] private float spacing = 0f;
        [SerializeField] private LayeredSeries data = new LayeredSeries();

        private List<UIVertex> vertices = new();
        private List<int> indices = new();

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            vertices.Clear();
            indices.Clear();

            if (data == null || data.Count <= 0)
                return;
            if (!geometryDirty)
                return;

            var halfRadius = 0.5f * Mathf.Min(rectTransform.rect.width, rectTransform.rect.height);
            var ringWidth = (halfRadius - this.innerRadius - spacing) / data.Count;
            var innerRadius = this.innerRadius;
            var startAngleDegree = StartAngleOffset - startAngle;
            foreach (var layer in data)
            {
                CalculateLayer(layer, innerRadius, startAngleDegree, ringWidth);
                innerRadius += ringWidth + spacing;
            }
            vh.AddUIVertexStream(vertices, indices);
        }

        private void CalculateLayer(MultiLegendSeries layer, float innerRadius, float startAngleDegree, float ringWidth)
        {
            var startIndex = 0;
            var total = layer.Sum(series => series.Value);
            foreach (var series in layer)
            {
                var value = series.Value;
                var arcAngleDegree = 360f * value / total;
                var segments = SegmentsForArc(innerRadius + ringWidth, arcAngleDegree);
                var stepAngleDegree = arcAngleDegree / segments;

                var vertex = UIVertex.simpleVert;
                vertex.color = series.Color;

                for (var segment = 0; segment <= segments; segment++)
                {
                    var angleDegree = startAngleDegree - stepAngleDegree * segment;
                    var angleRadian = angleDegree * Mathf.Deg2Rad;
                    var basePosition = new Vector3(Mathf.Cos(angleRadian), Mathf.Sin(angleRadian));
                    vertex.position = basePosition * innerRadius;
                    vertices.Add(vertex);

                    vertex.position = basePosition * (innerRadius + ringWidth);
                    vertices.Add(vertex);

                    if (segment > 0)
                        AddQuadIndecies();
                }
                startIndex = vertices.Count;
                startAngleDegree -= arcAngleDegree;
            }
        }

        private void AddQuadIndecies()
        {
            indices.Add(vertices.Count - 4); // 0
            indices.Add(vertices.Count - 3); // 1
            indices.Add(vertices.Count - 2); // 2

            indices.Add(vertices.Count - 3); // 1
            indices.Add(vertices.Count - 1); // 3
            indices.Add(vertices.Count - 2); // 2
        }

        private int SegmentsForArc(float halfRadius, float angleDegree)
        {
            var scaleFactor = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);
            var radius = halfRadius * scaleFactor;
            if (radius <= 0f || angleDegree <= 0f)
                return 1;

            const float maxEdgeLength = 4f;
            var arcLength = 2f * Mathf.PI * radius *(angleDegree / 360f);
            var approx = Mathf.Max(1f, arcLength / maxEdgeLength);
            return Mathf.Clamp(Mathf.CeilToInt(approx), 1, 512);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (Application.isPlaying)
                return;
        }
#endif
    }
}