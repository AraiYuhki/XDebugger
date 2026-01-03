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
        private Action<string> onChangedValue;

        public string Text
        {
            get => text;
            set => SetText(value, false);
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
            var control = uiFactory.CreateControl<StringControl>(parent);
            control.Setup(this);
            return control;
        }

        public void NotifyTextChangedFromView(string newValue) => SetText(newValue, true);

        private void SetText(string newValue, bool notifyCallback)
        {
            if (text == newValue)
                return;

            text = newValue;
            NotifyChanged();

            if (notifyCallback)
                onChangedValue?.Invoke(text);
        }
    }
}
