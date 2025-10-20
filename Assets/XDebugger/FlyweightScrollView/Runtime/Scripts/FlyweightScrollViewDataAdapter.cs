using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Xeon.Common.FlyweightScrollView
{
    public class FlyweightScrollViewDataAdapter<T> : IObservableCollection<T>
    {
        private ObservableCollection<T> source;
        public event NotifyCollectionChangedEventHandler CollectionChanged;


        public T this[int index]
        {
            get => source[index];
            set => source[index] = value;
        }

        public int Count => source.Count;

        public FlyweightScrollViewDataAdapter(ObservableCollection<T> source)
        {
            this.source = source;
            source.CollectionChanged += OnChangedCollection;
        }

        private void OnChangedCollection(object sender, NotifyCollectionChangedEventArgs e)
            => CollectionChanged?.Invoke(this, e);

        public void Clear() => source.Clear();

        public IEnumerator<T> GetEnumerator()
        {
            for (var index = 0; index < source.Count; index++)
            {
                yield return source[index];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => source.GetEnumerator();
    }
}