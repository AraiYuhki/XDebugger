using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Xeon.Common
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class PieChart : MaskableGraphic
    {
        [SerializeField]
        private List<Series> series = new();

        [SerializeField, Tooltip("描画開始角度 (度)")]
        private float startAngle = 0f;

        [SerializeField, Tooltip("セグメントあたりの角度(度)")]
        private float segmentAngle = 6f;

        public void SetSeries(List<Series> newSeries)
        {
            series = newSeries ?? new List<Series>();
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
            var radius = Mathf.Min(rect.width, rect.height) * 0.5f;

            var currentAngle = startAngle;
            foreach (var data in series)
            {
                if (data == null)
                    continue;

                var value = Mathf.Max(0f, data.SumValues());
                if (value <= 0f)
                    continue;

                var sliceAngle = (value / total) * 360f;
                AddSlice(vh, center, radius, currentAngle, currentAngle + sliceAngle, MultiplyColor(color, data.Color));
                currentAngle += sliceAngle;
            }
        }

        private void AddSlice(VertexHelper vh, Vector2 center, float radius, float start, float end, Color sliceColor)
        {
            var delta = Mathf.Abs(end - start);
            var steps = Mathf.Max(1, Mathf.CeilToInt(delta / Mathf.Max(1f, segmentAngle)));
            var stepAngle = (end - start) / steps;

            var centerIndex = vh.currentVertCount;
            var centerVertex = UIVertex.simpleVert;
            centerVertex.color = sliceColor;
            centerVertex.position = center;
            vh.AddVert(centerVertex);

            for (var i = 0; i <= steps; i++)
            {
                var angle = (start + stepAngle * i) * Mathf.Deg2Rad;
                var position = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius + center;
                var vertex = UIVertex.simpleVert;
                vertex.color = sliceColor;
                vertex.position = position;
                vh.AddVert(vertex);
            }

            for (var i = 0; i < steps; i++)
            {
                vh.AddTriangle(centerIndex, centerIndex + i + 1, centerIndex + i + 2);
            }
        }

        private static Color MultiplyColor(Color lhs, Color rhs)
        {
            return new Color(lhs.r * rhs.r, lhs.g * rhs.g, lhs.b * rhs.b, lhs.a * rhs.a);
        }
    }
}
