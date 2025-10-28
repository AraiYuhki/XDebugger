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
    public class TestSeries : IEnumerable<float>
    {
        [SerializeField] private string name;
        [SerializeField] private Color color = Color.white;
        [SerializeField] private ArraySeriesDataAdapter data;
        
        public event Action OnChangedCollection
        {
            add => data.OnChangedCollection += value;
            remove => data.OnChangedCollection -= value;
        }

        public float this[int index] => data[index];
        public float MaxValue => data.Max();

        public string Name => name;
        public Color Color => color;
        public int Count => data.Count;

        public IEnumerator<float> GetEnumerator() => data.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
#endif
