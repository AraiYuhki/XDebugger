using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Xeon.Common;

namespace Xeon.XGraph.Model
{
    /// <summary>
    /// グラフで使用するデータ系列 (List / float[] / CircularBuffer をソースにできる)
    /// </summary>
    [Serializable]
    public class Series : IEnumerable<float>
    {
        [SerializeField] private string name;
        [SerializeField] private Color color = Color.white;
        
        public event Action OnChangedCollection
        {
            add => data.OnChangedCollection += value;
            remove => data.OnChangedCollection -= value;
        }

        public float this[int index] => data[index];
        public float MaxValue => data.Max();

        
        private ISeriesDataAdapter data; 

        public string Name => name;
        public Color Color => color;
        public int Count => data.Count;

        public Series(string name, Color color, float[] values) : this(name, color)
        {
            data = SeriesDataAdapterFactory.Create(values, null);
        }

        public Series(string name, Color color, List<float> values) : this(name, color)
        {
            data = SeriesDataAdapterFactory.Create(values, null);
        }

        public Series(string name, Color color, CircularBuffer<float> values) : this(name, color)
        {
            data = SeriesDataAdapterFactory.Create(values, null);
        }

        private Series(string name, Color color)
        {
            this.name = name;
            this.color = color;
        }

        public void Add(float value) => data.Add(value);

        public void Clear() => data.Clear();

        public IEnumerator<float> GetEnumerator() => data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
