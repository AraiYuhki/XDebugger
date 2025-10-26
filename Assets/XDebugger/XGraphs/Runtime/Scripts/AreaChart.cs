using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Xeon.Common
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class AreaChart : MaskableGraphic
    {
        [SerializeField]
        private List<Series> seriesList = new();

        [SerializeField]
        private RectOffset padding = new RectOffset();

        [SerializeField]
        private float min = 0f;

        [SerializeField]
        private float max = 100f;

        [SerializeField]
        private bool stacked = false;

        private readonly List<float> topBuffer = new();
        private readonly List<float> bottomBuffer = new();
        private readonly List<float> accumulator = new();

        public void SetSeries(List<Series> newSeries)
        {
            seriesList = newSeries ?? new List<Series>();
            SetVerticesDirty();
        }

        public void SetRange(float newMin, float newMax)
        {
            min = newMin;
            max = newMax;
            SetVerticesDirty();
        }

        public void SetStacked(bool value)
        {
            stacked = value;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (seriesList == null || seriesList.Count == 0)
                return;

            var rect = rectTransform.rect;
            var width = rect.width - padding.horizontal;
            var height = rect.height - padding.vertical;
            if (width <= 0f || height <= 0f)
                return;

            var sampleCount = 0;
            foreach (var s in seriesList)
            {
                if (s == null)
                    continue;
                sampleCount = Mathf.Max(sampleCount, s.Values.Count);
            }

            if (sampleCount <= 1)
                return;

            EnsureCapacity(topBuffer, sampleCount);
            EnsureCapacity(bottomBuffer, sampleCount);
            EnsureCapacity(accumulator, sampleCount);

            var bottom = rect.yMin + padding.bottom;
            var top = rect.yMax - padding.top;
            var left = rect.xMin + padding.left;
            var stepX = width / (sampleCount - 1f);
            var baseColor = color;
            var range = Mathf.Abs(max - min) < Mathf.Epsilon ? 1f : max - min;

            for (var i = 0; i < sampleCount; i++)
            {
                topBuffer[i] = bottom;
                bottomBuffer[i] = bottom;
                accumulator[i] = min;
            }

            foreach (var series in seriesList)
            {
                if (series == null || series.Values.Count == 0)
                    continue;

                var seriesColor = MultiplyColor(baseColor, series.Color);
                var baseIndex = vh.currentVertCount;

                var valueCount = series.Values.Count;
                var lastValue = valueCount > 0 ? series.Values[valueCount - 1] : 0f;

                for (var index = 0; index < sampleCount; index++)
                {
                    var value = index < valueCount ? series.Values[index] : lastValue;
                    var previousRaw = stacked ? accumulator[index] : min;
                    var clampedValue = stacked ? Mathf.Max(0f, value) : Mathf.Clamp(value, min, max);
                    var currentRaw = stacked ? previousRaw + clampedValue : clampedValue;

                    var basePercent = Mathf.Clamp01((previousRaw - min) / range);
                    var currentPercent = Mathf.Clamp01((currentRaw - min) / range);

                    bottomBuffer[index] = Mathf.Lerp(bottom, top, basePercent);
                    topBuffer[index] = Mathf.Lerp(bottom, top, currentPercent);

                    if (stacked)
                        accumulator[index] = currentRaw;
                }

                AddArea(vh, left, stepX, seriesColor, topBuffer, bottomBuffer, sampleCount, baseIndex);
            }
        }

        private static void AddArea(VertexHelper vh, float left, float stepX, Color seriesColor, List<float> top, List<float> bottom, int count, int baseIndex)
        {
            var vert = UIVertex.simpleVert;
            vert.color = seriesColor;

            for (var index = 0; index < count; index++)
            {
                var x = left + stepX * index;

                vert.position = new Vector2(x, top[index]);
                vh.AddVert(vert);

                vert.position = new Vector2(x, bottom[index]);
                vh.AddVert(vert);
            }

            for (var index = 1; index < count; index++)
            {
                var prevTop = baseIndex + (index - 1) * 2;
                var prevBottom = prevTop + 1;
                var currentTop = baseIndex + index * 2;
                var currentBottom = currentTop + 1;

                vh.AddTriangle(prevBottom, prevTop, currentTop);
                vh.AddTriangle(prevBottom, currentTop, currentBottom);
            }
        }

        private static void EnsureCapacity(List<float> buffer, int count)
        {
            if (buffer.Capacity < count)
                buffer.Capacity = count;

            if (buffer.Count < count)
            {
                var add = count - buffer.Count;
                for (var i = 0; i < add; i++)
                    buffer.Add(0f);
            }
            else if (buffer.Count > count)
            {
                buffer.RemoveRange(count, buffer.Count - count);
            }
        }

        private static Color MultiplyColor(Color lhs, Color rhs)
        {
            return new Color(lhs.r * rhs.r, lhs.g * rhs.g, lhs.b * rhs.b, lhs.a * rhs.a);
        }
    }
}
