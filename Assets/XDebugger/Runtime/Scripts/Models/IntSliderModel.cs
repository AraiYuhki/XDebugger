using System;
using UnityEngine;
using Xeon.XDebugger.Control;

namespace Xeon.XDebugger.Model
{
    public class IntSliderModel : ControlModelBase
    {
        protected override string prefabAddress => $"XDebugger/{nameof(IntSliderControl)}";

        private int value, min, max;
        private IntSliderControl control;
        private Action<int> onValueChanged;

        public int Value
        {
            get => value;
            set
            {
                this.value = value;
                control?.Refresh();
            }
        }

        public int Min => min;
        public int Max => max;
        
        public IntSliderModel(string title, int value, int min, int max, Action<int> onChangedValue, int priority = 0)
            : base(title, priority)
        {
            this.value = value;
            this.min = min;
            this.max = max;
            this.onValueChanged = onChangedValue;
        }

        public void SetMin(int min, bool isRefreshControl = true)
        {
            this.min = min;
            if (isRefreshControl)
                control?.Refresh();
        }

        public void SetMax(int max, bool isRefreshControl = true)
        {
            this.max = max;
            if (isRefreshControl)
                control?.Refresh();
        }

        public override ControlBase CreateControl(Transform parent)
        {
            control = Instantiate<IntSliderControl>(parent);
            control.Setup(this, OnValueChanged);
            return control;
        }

        private void OnValueChanged(int newValue)
        {
            value = newValue;
            onValueChanged?.Invoke(value);
        }
    }
}