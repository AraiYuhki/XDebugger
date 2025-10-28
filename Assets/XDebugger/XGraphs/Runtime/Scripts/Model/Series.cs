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
    public class Series : SeriesBase
    {
        public override event Action OnChangedCollection
        {
            add => data.OnChangedCollection += value;
            remove => data.OnChangedCollection -= value;
        }

        public override float this[int index] => data[index];
        public override float MaxValue => data.Max();
        
        private ISeriesDataAdapter data;
        
        public override int Count => data.Count;

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

        public override IEnumerator<float> GetEnumerator() => data.GetEnumerator();
    }
}
