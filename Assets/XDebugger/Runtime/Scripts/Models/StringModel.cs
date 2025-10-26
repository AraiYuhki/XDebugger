using System;
using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Model
{
    public class StringModel : ControlModelBase
    {
        protected override string prefabAddress => $"XDebugger/{nameof(StringControl)}";
        
        private string text = string.Empty;
        private StringControl control;

        private Action<string> onChangedValue;

        public string Text
        {
            get => text;
            set
            {
                text = value;
                control?.Refresh();
            }
        }

        public StringModel(string title, string text, Action<string> onChangedValue, int priority = 0) :base(title, priority)
        {
            Initialize(text, onChangedValue);
        }

        public StringModel(string title, string text, Action<string> onChangedValue, IGroupModel parent, int priority = 0)
            : base(title, parent, priority)
        {
            Initialize(text, onChangedValue);
        }

        private void Initialize(string text, Action<string> onChangedValue)
        {
            this.text = text;
            this.onChangedValue = onChangedValue;
        }

        public override ControlBase CreateControl(Transform parent, IUIFactory uiFactory)
        {
            control = uiFactory.CreateControl<StringControl>(parent);
            control.Setup(this, OnChangedValue);
            control.SetInteractable(IsInteractable);
            return control;
        }

        private void OnChangedValue(string newValue)
        {
            text = newValue;
            onChangedValue?.Invoke(text);
        }
    }
}