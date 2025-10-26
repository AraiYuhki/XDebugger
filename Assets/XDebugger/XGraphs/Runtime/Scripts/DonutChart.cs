using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Xeon.Common
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class DonutChart : MaskableGraphic
    {
        [SerializeField]
        private List<Series> series = new();

        [SerializeField, Range(0f, 0.95f)]
        private float innerRadiusRatio = 0.5f;

        [SerializeField, Tooltip("描画開始角度 (度)")]
        private float startAngle = 0f;

        [SerializeField, Tooltip("セグメントあたりの角度(度)")]
        private float segmentAngle = 6f;

        public void SetSeries(List<Series> newSeries)
        {
            series = newSeries ?? new List<Series>();
            SetVerticesDirty();
        }

        public void SetInnerRadiusRatio(float ratio)
        {
            innerRadiusRatio = Mathf.Clamp01(ratio);
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (series == null || series.Count == 0)
                return;

            var total = 0f;
            foreach (var data in series)
                total += Mathf.Max(0f, data?.SumValues() ?? 0f);

            if (total <= 0f)
                return;

            var rect = rectTransform.rect;
            var center = rect.center;
            var outerRadius = Mathf.Min(rect.width, rect.height) * 0.5f;
            var innerRadius = outerRadius * innerRadiusRatio;

            var currentAngle = startAngle;
            foreach (var data in series)
            {
                if (data == null)
                    continue;

                var value = Mathf.Max(0f, data.SumValues());
                if (value <= 0f)
                    continue;

                var sliceAngle = (value / total) * 360f;
                AddSlice(vh, center, innerRadius, outerRadius, currentAngle, currentAngle + sliceAngle, MultiplyColor(color, data.Color));
                currentAngle += sliceAngle;
            }
        }

        private void AddSlice(VertexHelper vh, Vector2 center, float innerRadius, float outerRadius, float start, float end, Color sliceColor)
        {
            var delta = Mathf.Abs(end - start);
            var steps = Mathf.Max(1, Mathf.CeilToInt(delta / Mathf.Max(1f, segmentAngle)));
            var stepAngle = (end - start) / steps;

            for (var i = 0; i < steps; i++)
            {
                var angle0 = (start + stepAngle * i) * Mathf.Deg2Rad;
                var angle1 = (start + stepAngle * (i + 1)) * Mathf.Deg2Rad;

                var outer0 = center + new Vector2(Mathf.Cos(angle0), Mathf.Sin(angle0)) * outerRadius;
                var outer1 = center + new Vector2(Mathf.Cos(angle1), Mathf.Sin(angle1)) * outerRadius;
                var inner0 = center + new Vector2(Mathf.Cos(angle0), Mathf.Sin(angle0)) * innerRadius;
                var inner1 = center + new Vector2(Mathf.Cos(angle1), Mathf.Sin(angle1)) * innerRadius;

                AddQuad(vh, inner0, inner1, outer1, outer0, sliceColor);
            }
        }

        private void AddQuad(VertexHelper vh, Vector2 bl, Vector2 tl, Vector2 tr, Vector2 br, Color sliceColor)
        {
            var startIndex = vh.currentVertCount;

            var vertex = UIVertex.simpleVert;
            vertex.color = sliceColor;

            vertex.position = bl;
            vh.AddVert(vertex);

            vertex.position = tl;
            vh.AddVert(vertex);

            vertex.position = tr;
            vh.AddVert(vertex);

            vertex.position = br;
            vh.AddVert(vertex);

            vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vh.AddTriangle(startIndex, startIndex + 2, startIndex + 3);
        }

        private static Color MultiplyColor(Color lhs, Color rhs)
        {
            return new Color(lhs.r * rhs.r, lhs.g * rhs.g, lhs.b * rhs.b, lhs.a * rhs.a);
        }
    }
}
