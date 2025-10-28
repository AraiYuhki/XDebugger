using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Xeon.XGraph.Model
{
    [Serializable]
    public class ArraySeriesDataAdapter : ISeriesDataAdapter
    {
        [SerializeField]
        private float[] values;
        private event Action onChangedCollection;
        
        public event Action OnChangedCollection
        {
            add
            {
                onChangedCollection -= value;
                onChangedCollection += value;
                onChangedCollection?.Invoke();
            }
            remove => onChangedCollection -= value;
        }

        public float this[int index]
        {
            get => values[index];
            set
            {
                values[index] = value;
                onChangedCollection?.Invoke();
            }
        }

        public ArraySeriesDataAdapter(float[] data, Action onChangedCollection)
        {
            values = data;
            this.onChangedCollection = onChangedCollection;
        }

        public void Add(float value) => values = values.Append(value).ToArray();

        public void Clear() => values = Enumerable.Empty<float>().ToArray();

        public IEnumerator<float> GetEnumerator()
        {
            foreach (var value in values)
                yield return value;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => values.Length;
    }
}
