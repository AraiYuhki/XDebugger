using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Xeon.XGraph.Model
{
    [Serializable]
    public class MultiLegendSeries : IEnumerable<SingleValueSeries>
    {
        [SerializeField]
        protected SingleValueSeries[] series;

        protected Color[] preMultipliedColors = Array.Empty<Color>();

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

        public bool HasSeries => series != null;
        public int Count => series == null ? 0 : series.Length;

        public MultiLegendSeries() { }

        public MultiLegendSeries(IEnumerable<SingleValueSeries> series)
        {
            this.series = series.ToArray();
            foreach (var s in this.series)
                RegisterEvent(s);
        }

        public void SetData(IEnumerable<SingleValueSeries> series)
        {
            foreach (var s in this.series)
                UnregisterEvent(s);
            this.series = series.ToArray();
            foreach (var s in this.series)
                RegisterEvent(s);
        }

        private void RegisterEvent(SingleValueSeries series)
        {
            series.OnChangedValue += ChangedValue;
            series.OnChangedColor += ChangedColor;
        }

        private void UnregisterEvent(SingleValueSeries series)
        {
            series.OnChangedValue -= ChangedValue;
            series.OnChangedColor -= ChangedColor;
        }

        private void ChangedValue() => onChangedValue?.Invoke();

        private void ChangedColor() => onChangedColor?.Invoke();

        public IEnumerator<SingleValueSeries> GetEnumerator()
        {
            foreach (var s in this.series)
                yield return s;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
