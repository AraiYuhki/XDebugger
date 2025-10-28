using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Xeon.XGraph.Model
{
    [Serializable]
    public class ListSeriesDataAdapter : ISeriesDataAdapter
    {
        [SerializeField]
        private List<float> values;
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

        public ListSeriesDataAdapter(List<float> values, Action onChangedCollection)
        {
            this.values = values;
            this.onChangedCollection = onChangedCollection;
        }

        public void Add(float value)
        {
            values.Add(value);
            onChangedCollection?.Invoke();
        }

        public void Clear()
        {
            values.Clear();
            onChangedCollection?.Invoke();
        }

        public IEnumerator<float> GetEnumerator() => values.GetEnumerator();
        
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => values.Count;
    }
}
