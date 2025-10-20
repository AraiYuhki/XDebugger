using System.Collections.Generic;
using System.Collections.Specialized;

namespace Xeon.Common.FlyweightScrollView
{
    public interface IObservableCollection<T> : IEnumerable<T>, INotifyCollectionChanged
    {
        T this[int index] { get; }
        void Clear();
        int Count { get; }
    }
}
