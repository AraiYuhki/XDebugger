using System;
using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Model
{
    public class NumberModel : ControlModelBase
    {
        protected override string prefabAddress => $"XDebugger/{nameof(NumberControl)}";

        private float value = 0f;
        private float step = 1f;

        private Action<float> onChangedValue;

        public float Value
        {
            get => value;
            set => SetValue(value, false);
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
            var control = uiFactory.CreateControl<NumberControl>(parent);
            control.Setup(this);
            return control;
        }

        public void NotifyValueChangedFromView(float newValue) => SetValue(newValue, true);

        public void SetValue(float newValue, bool notifyCallback)
        {
            if (Mathf.Approximately(value, newValue))
                return;

            value = newValue;
            NotifyChanged();

            if (notifyCallback)
                onChangedValue?.Invoke(value);
        }
    }
}
