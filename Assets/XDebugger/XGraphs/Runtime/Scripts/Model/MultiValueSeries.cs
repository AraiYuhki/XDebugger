using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace Xeon.XGraph.Model
{
    [Serializable]
    public class MultiValueSeries : IEnumerable<Series>
    {
        [SerializeField]
        protected Series[] series;

        protected Color[] preMultipliedColors = Array.Empty<Color>();

        public bool HasSeries => series != null;
        public int Count => series == null ? 0 : series.Length;

        public Series this[int index]
        {
            get => series[index];
        }

        public MultiValueSeries() { }

        public MultiValueSeries(List<Series> source, Color color)
        {
            series = source.ToArray();
            preMultipliedColors = new Color[series.Length];
            SetColor(color);
        }

        public MultiValueSeries(int legendCount, Color color)
        {
            series = new Series[legendCount];
            preMultipliedColors = new Color[legendCount];
            SetColor(color);
        }

        public void AddValue(params float[] values)
        {
            if (series.Length != values.Length)
            {
                throw new ArgumentException($"指定した配列の長さが異なっています Expect: {series.Length} Actual: {values.Length}");
            }
            for (var index = 0; index < series.Length; index++)
                series[index].Add(values[index]);
        }

        public void Clear()
        {
            foreach (var series in series)
                series.Clear();
        }

        public void SetColor(Color color)
        {
            if (preMultipliedColors == null || preMultipliedColors.Length != Count)
                preMultipliedColors = new Color[Count];
            for (var index = 0; index < Count; index++)
                preMultipliedColors[index] = series[index].Color * color;
        }

        public Color GetSegmentColor(int index, Color defaultColor)
        {
            if(preMultipliedColors.Length == 0)
                return defaultColor;
            if (index < preMultipliedColors.Length)
                return preMultipliedColors[index];

            // 不足している場合は最後の要素を使用する
            return preMultipliedColors[preMultipliedColors.Length - 1];
        }

        public IEnumerator<Series> GetEnumerator()
        {
            foreach (var s in series)
                yield return s;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
