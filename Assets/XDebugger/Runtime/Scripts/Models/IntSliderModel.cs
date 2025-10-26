using System;
using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Model
{
    public class IntSliderModel : ControlModelBase
    {
        protected override string prefabAddress => $"XDebugger/{nameof(IntSliderControl)}";

        private int value, min, max;
        private IntSliderControl control;
        private Action<int> onChangedValue;

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
            Initialize(value, min, max, onChangedValue);
        }

        public IntSliderModel(string title, int value, int min, int max, Action<int> onChangedValue, IGroupModel parent, int priority = 0)
            : base(title, parent, priority)
        {
            Initialize(value, min, max, onChangedValue);
        }

        private void Initialize(int value, int min, int max, Action<int> onChangedValue)
        {
            this.value = value;
            this.min = min;
            this.max = max;
            this.onChangedValue = onChangedValue;
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

        public override ControlBase CreateControl(Transform parent, IUIFactory uiFactory)
        {
            control = uiFactory.CreateControl<IntSliderControl>(parent);
            control.Setup(this, OnValueChanged);
            control.SetInteractable(IsInteractable);
            return control;
        }

        private void OnValueChanged(int newValue)
        {
            value = newValue;
            onChangedValue?.Invoke(value);
        }
    }
}