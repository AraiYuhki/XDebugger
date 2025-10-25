using System.Collections.Generic;

namespace Xeon.Common
{
    public interface IStackedBarItemData : IEnumerable<float>
    {
        public float this[int index] { get; }
        public int Count { get; }
    }
}
