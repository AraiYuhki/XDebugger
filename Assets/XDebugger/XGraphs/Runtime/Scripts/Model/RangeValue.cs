using System;
using UnityEngine;

namespace Xeon.XGraph.Model
{
    [Serializable]
    public class RangeValue
    {
        [SerializeField]
        private float min = 0f;
        [SerializeField]
        private float max = 100f;

        private float range = 0f;

        private bool isApproximately = false;

        public float Min
        {
            get => min;
            set 
            {
                min = value;
                range = max - min;
                isApproximately = Mathf.Approximately(min, max);
            }
        }

        public float Max
        {
            get => max;
            set
            {
                max = value;
                range = max - min;
                isApproximately = Mathf.Approximately(min, max);
            }
        }

        public float Range => range;

        public RangeValue()
        {
            isApproximately = Mathf.Approximately(min, max);
        }

        public RangeValue(float min, float max)
        {
            this.min = min;
            this.max = max;
            range = max - min;
            isApproximately = Mathf.Approximately(min, max);
        }

        public float Normalized(float value) => (value - min) / range;

        public bool InRange(float value)
        {
            if (isApproximately)
                return false;
            return min <= value && value <= max;
        }
        public bool IsApproximately => isApproximately;
    }
}