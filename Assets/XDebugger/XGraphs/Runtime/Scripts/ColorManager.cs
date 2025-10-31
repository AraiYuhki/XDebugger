using System;
using UnityEngine;
using Xeon.XGraph.Model;

namespace Xeon.XGraph.Controller
{
    public class ColorManager
    {
        private MultiValueSeries buffer;
        private Color[] preMultipliedColors = Array.Empty<Color>();

        public bool IsDirty { get; set; }

        public ColorManager(MultiValueSeries buffer, Color graphicsColor)
        {
            this.buffer = buffer;
            IsDirty = true;
            RecalculateColors(graphicsColor);
        }

        public void RecalculateColors(Color graphicsColor)
        {
            if (!IsDirty)
                return;
            if (preMultipliedColors == null || preMultipliedColors.Length != buffer.Count)
                preMultipliedColors = new Color[buffer.Count];
            for (var index = 0; index < buffer.Count; index++)
                preMultipliedColors[index] = buffer.Series[index].Color * graphicsColor;
            IsDirty = false;
        }

        public Color GetSegmentColor(int index, Color defaultColor)
        {
            if (preMultipliedColors == null || preMultipliedColors.Length == 0)
                return defaultColor;
            if (index < preMultipliedColors.Length)
                return preMultipliedColors[index];
            
            // 不足している場合は最後の要素を使用する
            return preMultipliedColors[preMultipliedColors.Length - 1];
        }
    }
}