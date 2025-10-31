using System;
using UnityEngine;
using Xeon.XGraph.Model;

namespace Xeon.XGraph.Controller
{
    /// <summary>
    /// MultiValueSeries の凡例カラーをキャッシュするヘルパークラス。
    /// </summary>
    public sealed class ColorManager : IDisposable
    {
        private MultiValueSeries buffer;
        private Color[] preMultipliedColors = Array.Empty<Color>();
        private Color lastGraphicsColor;
        private bool hasLastGraphicsColor;
        private bool isDirty = true;

        public void SetBuffer(MultiValueSeries newBuffer)
        {
            if (ReferenceEquals(buffer, newBuffer))
            {
                isDirty = true;
                hasLastGraphicsColor = false;
                return;
            }

            Unsubscribe(buffer);
            buffer = newBuffer;
            Subscribe(buffer);

            if (buffer == null)
                preMultipliedColors = Array.Empty<Color>();

            hasLastGraphicsColor = false;
            isDirty = true;
        }

        public void RecalculateColors(Color graphicsColor)
        {
            if (buffer == null || buffer.Count == 0)
            {
                preMultipliedColors = Array.Empty<Color>();
                hasLastGraphicsColor = true;
                lastGraphicsColor = graphicsColor;
                isDirty = false;
                return;
            }

            if (!hasLastGraphicsColor || lastGraphicsColor != graphicsColor)
            {
                lastGraphicsColor = graphicsColor;
                hasLastGraphicsColor = true;
                isDirty = true;
            }

            if (preMultipliedColors.Length != buffer.Count)
            {
                preMultipliedColors = new Color[buffer.Count];
                isDirty = true;
            }

            if (!isDirty)
                return;

            for (var index = 0; index < buffer.Count; index++)
                preMultipliedColors[index] = buffer.Series[index].Color * graphicsColor;

            isDirty = false;
        }

        public Color GetSegmentColor(int index, Color defaultColor)
        {
            if (preMultipliedColors.Length == 0)
                return defaultColor;
            if (index < preMultipliedColors.Length)
                return preMultipliedColors[index];

            // 不足している場合は最後の要素を使用する
            return preMultipliedColors[preMultipliedColors.Length - 1];
        }

        public void Dispose()
        {
            SetBuffer(null);
        }

        private void Subscribe(MultiValueSeries target)
        {
            if (target?.Series == null)
                return;

            foreach (var series in target.Series)
            {
                if (series == null)
                    continue;
                series.OnChangedColor += OnSeriesColorChanged;
            }
        }

        private void Unsubscribe(MultiValueSeries target)
        {
            if (target?.Series == null)
                return;

            foreach (var series in target.Series)
            {
                if (series == null)
                    continue;
                series.OnChangedColor -= OnSeriesColorChanged;
            }
        }

        private void OnSeriesColorChanged()
        {
            isDirty = true;
        }
    }
}
