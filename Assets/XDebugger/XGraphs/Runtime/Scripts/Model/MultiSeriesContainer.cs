using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Xeon.XGraph.Model
{
    [Serializable]
    public class MultiSeriesContainer
    {
        [SerializeField]
        private Series[] series;

        public IReadOnlyCollection<Series> Series => series;
        public int Count => series.Length;

        public MultiSeriesContainer() { }

        public MultiSeriesContainer(List<Series> source)
        {
            series = source.ToArray();
        }

        public MultiSeriesContainer(int legendCount)
        {
            series = new Series[legendCount];
        }

        public void AddValue(params float[] values)
        {
            if (series.Length != values.Length)
            {
                throw new ArgumentException($"指定した配列の長さが異なっています Expect: {series.Length} Actual: {values.Length}");
            }
            for (var index = 0; index < series.Length; index++)
                series[index].Add(values[index]);
        }

        public void Clear()
        {
            foreach (var series in series)
                series.Clear();
        }
#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(MultiSeriesContainer))]
        private class MultiSeriesContainerEditor : PropertyDrawer
        {
            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return base.GetPropertyHeight(property, label);
            }

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                base.OnGUI(position, property, label);
            }
        }
#endif
    }
}
