using System;
using System.Collections.Generic;

namespace Xeon.XGraph.Model
{
    /// <summary>
    /// グラフ系列データアダプタインターフェース
    /// </summary>
    public interface ISeriesDataAdapter : IReadOnlyList<float>
    {
        float this[int index] { get; set; }
        void Add(float value);
        void Clear();
        event Action OnChangedCollection;
    }
}