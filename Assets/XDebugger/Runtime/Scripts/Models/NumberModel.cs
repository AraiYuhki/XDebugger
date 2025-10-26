using System;
using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Model
{
    public class NumberModel : ControlModelBase
    {
        protected override string prefabAddress => $"XDebugger/{nameof(NumberControl)}";

        private NumberControl control;
        private float value = 0f;
        private float step = 1f;

        private Action<float> onChangedValue;

        public float Value
        {
            get => value;
            set
            {
                this.value = value;
                control?.Refresh();
            }
        }

        public float Step => step;

        public NumberModel(string title, float value, float step, Action<float> onChangedValue, int priority = 0) : base(title, priority)
        {
            Initialize(value, step, onChangedValue);
        }

        public NumberModel(string title, float value, float step, Action<float> onChangedValue, IGroupModel parent, int priority = 0)
            : base(title, parent, priority)
        {
            Initialize(value, step, onChangedValue);
        }

        private void Initialize(float value, float step, Action<float> onChangedValue)
        {
            this.value = value;
            this.step = step;
            this.onChangedValue = onChangedValue;
        }

        public override ControlBase CreateControl(Transform parent, IUIFactory uiFactory)
        {
            control = uiFactory.CreateControl<NumberControl>(parent);
            control.Setup(this, OnValueChanged);
            return control;
        }

        private void OnValueChanged(float newValue)
        {
            value = newValue;
            onChangedValue?.Invoke(value);
        }
    }
}
