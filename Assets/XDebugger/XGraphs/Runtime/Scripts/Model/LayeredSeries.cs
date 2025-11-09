using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Xeon.XGraph.Model
{
    [Serializable]
    public class LayeredSeries : IEnumerable<MultiLegendSeries>
    {
        [SerializeField] private MultiLegendSeries[] buffer;

        private event Action onChangedValue;
        private event Action onChangedColor;

        public event Action OnChangedValue
        {
            add
            {
                onChangedValue -= value;
                onChangedValue += value;
                value?.Invoke();
            }

            remove => onChangedValue -= value;
        }

        public event Action OnChangedColor
        {
            add
            {
                onChangedColor -= value;
                onChangedColor += value;
                value?.Invoke();
            }

            remove => onChangedColor -= value;
        }

        public int Count => buffer == null ? 0 : buffer.Length;

        public LayeredSeries() { }
        public LayeredSeries(IEnumerable<MultiLegendSeries> buffer)
        {
            this.buffer = buffer.ToArray();
            foreach (var multiLegendSeries in buffer)
            {
                foreach (var series in multiLegendSeries)
                {
                    series.OnChangedValue += ValueChanged;
                    series.OnChangedColor += ColorChanged;
                }
            }
        }

        public void SetBuffer(IEnumerable<MultiLegendSeries> newData)
        {
            foreach (var multiValueSeries in buffer)
            {
                foreach (var series in multiValueSeries)
                {
                    series.OnChangedValue -= ValueChanged;
                    series.OnChangedColor -= ColorChanged;
                }
            }

            buffer = newData.ToArray();

            foreach (var multiValueSeries in buffer)
            {
                foreach (var series in multiValueSeries)
                {
                    series.OnChangedValue += ValueChanged;
                    series.OnChangedColor += ColorChanged;
                }
            }
        }

        private void ValueChanged() => onChangedValue?.Invoke();
        private void ColorChanged() => onChangedColor?.Invoke();

        public void RecalculateColors(Color color)
        {
            foreach (var series in buffer)
                series.RecalculateColors(color);
        }

        public IEnumerator<MultiLegendSeries> GetEnumerator()
        {
            foreach (var data in buffer)
                yield return data;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
