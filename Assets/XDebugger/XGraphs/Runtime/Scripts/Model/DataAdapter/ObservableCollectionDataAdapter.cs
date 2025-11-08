using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using UnityEngine;

namespace Xeon.XGraph.Model
{
    [Serializable]
    public class ObservableCollectionDataAdapter : ISeriesDataAdapter
    {
        [SerializeField]
        private ObservableCollection<float> values;
        private event Action onChangedCollection;

        public float this[int index] 
        {
            get => values[index];
            set
            {
                values[index] = value;
                onChangedCollection?.Invoke();
            }
        }
        public event Action OnChangedCollection
        {
            add
            {
                onChangedCollection -= value;
                onChangedCollection += value;
                value?.Invoke();
            }
            remove => onChangedCollection -= value;
        }

        public int Count => values.Count;

        public ObservableCollectionDataAdapter(ObservableCollection<float> newValue, Action onChangedCollection)
        {
            values = newValue;
            this.onChangedCollection += onChangedCollection;
            values.CollectionChanged += CollectionChanged;
        }


        public void Add(float value) => values.Add(value);

        public void Clear() => values.Clear();

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            onChangedCollection?.Invoke();
        }

        public IEnumerator<float> GetEnumerator() => values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
