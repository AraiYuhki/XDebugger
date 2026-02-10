using System;
using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Model
{
    public class FloatSliderModel : ControlModelBase
    {
        protected override string prefabAddress => $"XDebugger/{nameof(FloatSliderControl)}";

        private float _value, min, max;
        private Action<float> onChangedValue;

        public float Value
        {
            get => _value;
            set => SetValue(value, false);
        }

        public float Min => min;
        public float Max => max;
        public int Digits { get; private set; }

        public string GetRoundedText => Math.Round(_value, Digits).ToString();

        public FloatSliderModel(string title, float value, float min, float max, Action<float> onChangedValue, int digits = 2, int priority = 0)
            : base(title, priority)
        {
            Initialize(value, min, max, onChangedValue, digits);
        }

        public FloatSliderModel(string title, float value, float min, float max, Action<float> onChangedValue, IGroupModel parent, int digits = 2, int priority = 0)
            : base(title, parent, priority)
        {
            Initialize(value, min, max, onChangedValue, digits);
        }

        private void Initialize(float value, float min, float max, Action<float> onChangedValue, int digits)
        {
            this._value = value;
            this.min = min;
            this.max = max;
            Digits = digits;
            this.onChangedValue = onChangedValue;
        }

        public void SetMin(float min, bool isRefreshControl = true)
        {
            this.min = min;
            if (isRefreshControl)
                NotifyChanged();
        }

        public void SetMax(float max, bool isRefreshControl = true)
        {
            this.max = max;
            if (isRefreshControl)
                NotifyChanged();
        }

        public override ControlBase CreateControl(Transform parent, IUIFactory uiFactory)
        {
            var control = uiFactory.CreateControl<FloatSliderControl>(parent);
            control.Setup(this);
            return control;
        }

        public void NotifyValueChangedFromView(float newValue) => SetValue(newValue, true);

        public void SetValue(float newValue, bool notifyCallback)
        {
            if (Mathf.Approximately(_value, newValue))
                return;

            _value = newValue;
            NotifyChanged();

            if (notifyCallback)
                onChangedValue?.Invoke(_value);
        }
    }
}
