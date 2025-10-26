using System;
using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Model
{
    public class BoolModel : ControlModelBase
    {
        protected override string prefabAddress => $"XDebugger/{nameof(BoolControl)}";

        private BoolControl control;
        private bool isOn = false;
        private Action<bool> onChangedValue;

        public bool IsOn
        {
            get => isOn;
            set
            {
                isOn = value;
                control?.Refresh();
            }
        }

        public BoolModel(string title, bool isOn, Action<bool> onChangedValue, int priority = 0) : base(title, priority)
        {
            Initialize(isOn, onChangedValue);
        }

        public BoolModel(string title, bool isOn, Action<bool> onChangedValue, IGroupModel parent, int priority = 0)
            : base(title, priority)
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
            control = uiFactory.CreateControl<BoolControl>(parent);
            control.Setup(this, OnChangedValue);
            return control;
        }

        private void OnChangedValue(bool newValue)
        {
            isOn = newValue;
            onChangedValue?.Invoke(isOn);
        }
    }
}