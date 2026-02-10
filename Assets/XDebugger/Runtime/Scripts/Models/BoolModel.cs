using System;
using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Model
{
    public class BoolModel : ControlModelBase
    {
        protected override string prefabAddress => $"XDebugger/{nameof(BoolControl)}";

        private bool isOn = false;
        private Action<bool> onChangedValue;

        public bool Value
        {
            get => isOn;
            set => SetValue(value, false);
        }

        public BoolModel(string title, bool isOn, Action<bool> onChangedValue, int priority = 0) : base(title, priority)
        {
            Initialize(isOn, onChangedValue);
        }

        public BoolModel(string title, bool isOn, Action<bool> onChangedValue, IGroupModel parent, int priority = 0)
            : base(title, parent, priority)
        {
            Initialize(isOn, onChangedValue);
        }

        private void Initialize(bool isOn, Action<bool> onChangedValue)
        {
            this.isOn = isOn;
            this.onChangedValue = onChangedValue;
        }

        public override ControlBase CreateControl(Transform parent, IUIFactory uiFactory)
        {
            var control = uiFactory.CreateControl<BoolControl>(parent);
            control.Setup(this);
            return control;
        }

        public void NotifyValueChangedFromView(bool newValue) => SetValue(newValue, true);

        public void SetValue(bool newValue, bool notifyCallback)
        {
            if (isOn == newValue)
                return;

            isOn = newValue;
            NotifyChanged();

            if (notifyCallback)
                onChangedValue?.Invoke(isOn);
        }
    }
}
