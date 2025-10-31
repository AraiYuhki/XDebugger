using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Xeon.Common;

namespace Xeon.XGraph.Model
{
    public class CircularBufferSeriesDataAdapter : ISeriesDataAdapter
    {
        private CircularBuffer<float> values;
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
        
        public CircularBufferSeriesDataAdapter(CircularBuffer<float> values, Action onChangedCollection)
        {
            this.values = values;
            this.onChangedCollection += onChangedCollection;
        }

        public int Count => values.Count;

        public float this[int index]
        {
            get => values[index];
            set
            {
                values[index] = value;
                onChangedCollection?.Invoke();
            }
        }

        public void Add(float value)
        {
            values.PushBack(value);
            onChangedCollection?.Invoke();
        }

        public void Clear()
        {
            values.Clear();
            onChangedCollection?.Invoke();
        }
        
        public IEnumerator<float> GetEnumerator() => values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
