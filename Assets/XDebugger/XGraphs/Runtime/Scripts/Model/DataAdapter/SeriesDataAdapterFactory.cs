using System;
using System.Collections.Generic;
using Xeon.Common;

namespace Xeon.XGraph.Model
{
    /// <summary>
    /// シリーズデータアダプター生成クラス
    /// </summary>
    public static class SeriesDataAdapterFactory
    {
        public static ISeriesDataAdapter Create(float[] values, Action onChangedCollection = null)
        {
            return new ArraySeriesDataAdapter(values, onChangedCollection);
        }
        
        public static ISeriesDataAdapter Create(List<float> values, Action onChangedCollection = null)
        {
            return new ListSeriesDataAdapter(values, onChangedCollection);
        }

        public static ISeriesDataAdapter Create(CircularBuffer<float> values, Action onChangedCollection = null)
        {
            return new CircularBufferSeriesDataAdapter(values, onChangedCollection);
        }
    }
}
