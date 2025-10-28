using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Xeon.XGraph.Model
{
    /// <summary>
    /// データ系列インターフェース
    /// </summary>
    [Serializable]
    public abstract class SeriesBase : IEnumerable<float>
    {
        [SerializeField] protected string name;
        [SerializeField] protected Color color = Color.white;
        public abstract float this[int index] { get; }
        public abstract event Action OnChangedCollection;
        public abstract float MaxValue { get; }
        public abstract int Count { get; }
        
        public string Name => name;
        public Color Color => color;

        public abstract IEnumerator<float> GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
