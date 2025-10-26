using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Xeon.Common
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class HeatmapChart : MaskableGraphic
    {
        [SerializeField]
        private List<Series> rows = new();

        [SerializeField]
        private RectOffset padding = new RectOffset();

        [SerializeField]
        private float min = 0f;

        [SerializeField]
        private float max = 1f;

        [SerializeField]
        private bool autoNormalize = true;

        [SerializeField]
        private Gradient gradient = new Gradient();

        public void SetRows(List<Series> newRows)
        {
            rows = newRows ?? new List<Series>();
            SetVerticesDirty();
        }

        public void SetRange(float newMin, float newMax)
        {
            min = newMin;
            max = newMax;
            autoNormalize = false;
            SetVerticesDirty();
        }

        public void UseAutoNormalize(bool value)
        {
            autoNormalize = value;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (rows == null || rows.Count == 0)
                return;

            var rect = rectTransform.rect;
            var rowCount = rows.Count;
            var columnCount = GetMaxColumnCount(rows);
            if (rowCount == 0 || columnCount == 0)
                return;

            var width = rect.width - padding.horizontal;
            var height = rect.height - padding.vertical;
            if (width <= 0f || height <= 0f)
                return;

            var cellWidth = width / columnCount;
            var cellHeight = height / rowCount;
            var left = rect.xMin + padding.left;
            var top = rect.yMax - padding.top;

            var baseColor = color;
            var (normalizedMin, normalizedMax) = autoNormalize ? CalculateRange(rows) : (min, max);
            var range = Mathf.Abs(normalizedMax - normalizedMin) < Mathf.Epsilon ? 1f : normalizedMax - normalizedMin;

            for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                var series = rows[rowIndex];
                if (series == null)
                    continue;

                var yMin = top - cellHeight * (rowIndex + 1);
                var yMax = top - cellHeight * rowIndex;

                var seriesColor = MultiplyColor(baseColor, series.Color);
                var values = series.Values;
                var valueCount = values.Count;

                for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
                {
                    var value = columnIndex < valueCount ? values[columnIndex] : values.Count > 0 ? values[valueCount - 1] : 0f;
                    var percent = Mathf.Clamp01((value - normalizedMin) / range);
                    var sampleColor = gradient != null ? gradient.Evaluate(percent) : Color.white;
                    var finalColor = MultiplyColor(seriesColor, sampleColor);

                    var xMin = left + cellWidth * columnIndex;
                    var xMax = left + cellWidth * (columnIndex + 1);

                    AddQuad(vh, xMin, xMax, yMin, yMax, finalColor);
                }
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

        private static int GetMaxColumnCount(List<Series> seriesList)
        {
            var maxColumns = 0;
            foreach (var series in seriesList)
            {
                if (series == null)
                    continue;
                maxColumns = Mathf.Max(maxColumns, series.Values.Count);
            }
            return maxColumns;
        }

        private static (float min, float max) CalculateRange(List<Series> seriesList)
        {
            var minValue = float.MaxValue;
            var maxValue = float.MinValue;

            foreach (var series in seriesList)
            {
                if (series == null)
                    continue;

                foreach (var value in series.Values)
                {
                    minValue = Mathf.Min(minValue, value);
                    maxValue = Mathf.Max(maxValue, value);
                }
            }

            if (minValue == float.MaxValue || maxValue == float.MinValue)
                return (0f, 1f);

            return (minValue, maxValue);
        }

        private static Color MultiplyColor(Color lhs, Color rhs)
        {
            return new Color(lhs.r * rhs.r, lhs.g * rhs.g, lhs.b * rhs.b, lhs.a * rhs.a);
        }
    }
}
