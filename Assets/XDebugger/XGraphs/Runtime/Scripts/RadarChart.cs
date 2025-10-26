using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Xeon.Common
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class RadarChart : MaskableGraphic
    {
        [SerializeField]
        private List<string> categories = new();

        [SerializeField]
        private List<Series> series = new();

        [SerializeField]
        private float maxValue = 1f;

        [SerializeField]
        private float startAngle = 90f;

        public void SetCategories(List<string> newCategories)
        {
            categories = newCategories ?? new List<string>();
            SetVerticesDirty();
        }

        public void SetSeries(List<Series> newSeries)
        {
            series = newSeries ?? new List<Series>();
            SetVerticesDirty();
        }

        public void SetMaxValue(float value)
        {
            maxValue = Mathf.Max(0.0001f, value);
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (series == null || series.Count == 0)
                return;

            var categoryCount = categories.Count;
            if (categoryCount == 0)
                return;

            var rect = rectTransform.rect;
            var center = rect.center;
            var radius = Mathf.Min(rect.width, rect.height) * 0.5f;
            var angleStep = 360f / categoryCount;

            foreach (var data in series)
            {
                if (data == null)
                    continue;

                AddSeriesPolygon(vh, data, center, radius, angleStep);
            }
        }

        private void AddSeriesPolygon(VertexHelper vh, Series data, Vector2 center, float radius, float angleStep)
        {
            var baseIndex = vh.currentVertCount;
            var finalColor = MultiplyColor(color, data.Color);
            var vertex = UIVertex.simpleVert;
            vertex.color = finalColor;

            vertex.position = center;
            vh.AddVert(vertex);

            var categoryCount = Mathf.Max(1, categories.Count);

            for (var i = 0; i < categoryCount; i++)
            {
                var value = Mathf.Clamp01(data.GetValue(i) / Mathf.Max(0.0001f, maxValue));
                var angle = (startAngle - angleStep * i) * Mathf.Deg2Rad;
                var position = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius * value;
                vertex.position = position;
                vh.AddVert(vertex);
            }

            for (var i = 0; i < categoryCount; i++)
            {
                var next = (i + 1) % categoryCount;
                vh.AddTriangle(baseIndex, baseIndex + i + 1, baseIndex + next + 1);
            }
        }

        private static Color MultiplyColor(Color lhs, Color rhs)
        {
            return new Color(lhs.r * rhs.r, lhs.g * rhs.g, lhs.b * rhs.b, lhs.a * rhs.a);
        }
    }
}
