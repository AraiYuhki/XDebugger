using System;
using UnityEngine;

namespace Xeon.XGraph.Model
{
    [Serializable]
    public class SingleValueSeries : ISeries
    {
        [SerializeField] private string name;
        [SerializeField] private Color color;
        [SerializeField] private float value;

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

        public float Value
        {
            get => value;
            set
            {
                this.value = value;
                onChangedValue?.Invoke();
            }
        }

        private event Action onChangedValue;
        public event Action OnChangedValue
        {
            add
            {
                onChangedValue -= value;
                onChangedValue += value;
                value?.Invoke();
            }
            remove => onChangedValue -= value;
        }

        private event Action onChangedColor;
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

        public SingleValueSeries()
        {
        }

        public SingleValueSeries(string name, Color color, float value)
        {
            this.name = name;
            this.color = color;
            this.value = value;
        }
    }
}
