#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Xeon.XGraph.Model
{
    /// <summary>
    /// テスト用データ系列クラス
    /// </summary>
    [Serializable]
    public class TestSeries : SeriesBase
    {
        [SerializeField] private ArraySeriesDataAdapter data;
        
        public override event Action OnChangedCollection
        {
            add => data.OnChangedCollection += value;
            remove => data.OnChangedCollection -= value;
        }

        public override float this[int index] => data[index];
        public override float MaxValue => data.Max();
        
        public override int Count => data.Count;

        public override IEnumerator<float> GetEnumerator() => data.GetEnumerator();
    }
}
#endif
