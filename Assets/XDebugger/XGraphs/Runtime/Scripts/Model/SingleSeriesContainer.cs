using System;
using UnityEngine;

namespace Xeon.XGraph.Model
{
    [Serializable]
    public class SingleSeriesContainer
    {
        [SerializeField]
        private Series series;

        public Series Series => series;

        public SingleSeriesContainer() { }
        public SingleSeriesContainer(Series source)
        {
            series = source;
        }

        public void AddData(float data)
        {
            series.Add(data);
        }

        public void Clear()
        {
            series.Clear();
        }
    }
}