using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Xeon.Common;

namespace Xeon.XGraph.Model
{
    /// <summary>
    /// グラフで使用するデータ系列
    /// </summary>
    [Serializable]
    public class Series : ISeries, IEnumerable<float>
    {
        [SerializeField] private string name;
        [SerializeField] private Color color = Color.white;
#if UNITY_EDITOR
        [SerializeField] private List<float> testData = new();
#endif

        private event Action onChangedCollection;
        private event Action onChangedColor;

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
        
        public event Action OnChangedColor
        {
            add
            {
                onChangedColor -= value;
                onChangedColor += value;
                value?.Invoke();
            }
            remove => onChangedColor -= value;
        }

        public float this[int index]
        {
            get
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
                    return data[index];
                return testData[index];
#else
                return data[index];
#endif
            }
            
        }
        public float MaxValue
        {
            get
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
                    return data.Max();
                return testData.Max();
#else
                return data.Max();
#endif
            }
        }
        
        private ISeriesDataAdapter data;
        
        public int Count {
            get
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
                    return data.Count;
                return testData.Count;
#else
                return data.Count;
#endif
            }
        }
        public string Name => name;

        public Color Color
        {
            get => color;
            set
            {
                color = value;
                onChangedColor?.Invoke();
            }
        }

        public Series(string name, Color color, float[] values) : this(name, color)
        {
            data = SeriesDataAdapterFactory.Create(values, OnChangedCollectionInternal);
        }

        public Series(string name, Color color, List<float> values) : this(name, color)
        {
            data = SeriesDataAdapterFactory.Create(values, OnChangedCollectionInternal);
        }

        public Series(string name, Color color, CircularBuffer<float> values) : this(name, color)
        {
            data = SeriesDataAdapterFactory.Create(values, OnChangedCollectionInternal);
        }

        public Series() { }

        private Series(string name, Color color)
        {
            this.name = name;
            this.color = color;
        }

        private void OnChangedCollectionInternal()
        {
            onChangedCollection?.Invoke();
        }

        public void Add(float value) => data.Add(value);

        public void Clear() => data.Clear();

        public IEnumerator<float> GetEnumerator() => data.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
