using UnityEngine;
using UnityEngine.UI;

namespace Xeon.Common
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class HistogramChart : MaskableGraphic
    {
        [SerializeField]
        private Series data = new();

        [SerializeField]
        private RectOffset padding = new RectOffset();

        [SerializeField]
        private float barSpacing = 2f;

        [SerializeField]
        private float customMaxValue = -1f;

        public void SetSeries(Series newData)
        {
            data = newData ?? new Series();
            SetVerticesDirty();
        }

        public void SetBarSpacing(float spacing)
        {
            barSpacing = Mathf.Max(0f, spacing);
            SetVerticesDirty();
        }

        public void SetCustomMax(float value)
        {
            customMaxValue = value;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (data == null || data.Values.Count == 0)
                return;

            var rect = rectTransform.rect;
            var width = rect.width - padding.horizontal;
            var height = rect.height - padding.vertical;
            if (width <= 0f || height <= 0f)
                return;

            var count = data.Values.Count;
            if (count == 0)
                return;

            var baseColor = MultiplyColor(color, data.Color);
            var availableHeight = height;
            var bottom = rect.yMin + padding.bottom;
            var left = rect.xMin + padding.left;
            var step = width / count;
            var spacing = Mathf.Clamp(barSpacing, 0f, step * 0.5f);
            var halfSpacing = spacing * 0.5f;

            var maxValue = customMaxValue > 0f ? customMaxValue : GetMaxValue(data);
            if (maxValue <= 0f)
                return;

            for (var i = 0; i < count; i++)
            {
                var value = Mathf.Max(0f, data.Values[i]);
                var normalized = Mathf.Clamp01(value / maxValue);
                var barHeight = normalized * availableHeight;

                var xMin = left + step * i + halfSpacing;
                var xMax = left + step * (i + 1) - halfSpacing;

                var yMin = bottom;
                var yMax = bottom + barHeight;

                AddQuad(vh, xMin, xMax, yMin, yMax, baseColor);
            }
        }

        private static void AddQuad(VertexHelper vh, float xMin, float xMax, float yMin, float yMax, Color fillColor)
        {
            var baseIndex = vh.currentVertCount;
            var vert = UIVertex.simpleVert;
            vert.color = fillColor;

            vert.position = new Vector2(xMin, yMax);
            vh.AddVert(vert);
            vert.position = new Vector2(xMax, yMax);
            vh.AddVert(vert);
            vert.position = new Vector2(xMax, yMin);
            vh.AddVert(vert);
            vert.position = new Vector2(xMin, yMin);
            vh.AddVert(vert);

            vh.AddTriangle(baseIndex, baseIndex + 1, baseIndex + 2);
            vh.AddTriangle(baseIndex, baseIndex + 2, baseIndex + 3);
        }

        private static float GetMaxValue(Series series)
        {
            var maxValue = 0f;
            foreach (var value in series.Values)
                maxValue = Mathf.Max(maxValue, value);
            return maxValue;
        }

        private static Color MultiplyColor(Color lhs, Color rhs)
        {
            return new Color(lhs.r * rhs.r, lhs.g * rhs.g, lhs.b * rhs.b, lhs.a * rhs.a);
        }
    }
}
