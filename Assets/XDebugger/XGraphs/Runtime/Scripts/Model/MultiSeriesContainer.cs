using System;
using System.Collections.Generic;
using UnityEngine;

namespace Xeon.XGraph.Model
{
    [Serializable]
    public class MultiSeriesContainer
    {
        [SerializeField]
        private Series[] series;

        public IReadOnlyCollection<Series> Series => series;
        public int Count => series.Length;

        public MultiSeriesContainer() { }

        public MultiSeriesContainer(List<Series> source)
        {
            series = source.ToArray();
        }

        public MultiSeriesContainer(int legendCount)
        {
            series = new Series[legendCount];
        }

        public void AddValue(params float[] values)
        {
            if (series.Length != values.Length)
            {
                throw new ArgumentException($"指定した配列の長さがことなっています Expect: {series.Length} Actual: {values.Length}");
            }
            for (var index = 0; index < series.Length; index++)
                series[index].Add(values[index]);
        }

        public void Clear()
        {
            foreach (var series in series)
                series.Clear();
        }
    }
}
