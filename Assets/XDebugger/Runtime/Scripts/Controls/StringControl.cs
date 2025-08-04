using System;
using TMPro;
using UnityEngine;
using Xeon.XDebugger.Model;

namespace Xeon.XDebugger.Control
{
    public class StringControl : ControlBase
    {
        [SerializeField]
        protected TMP_InputField inputField;

        protected StringModel model;

        protected Action<string> onChangedValue;

        public void Setup(StringModel model, Action<string> onChangedValue)
        {
            Setup(model.Title);
            
            this.model = model;
            this.onChangedValue = onChangedValue;

            Refresh();
        }

        public override void Refresh()
        {
            inputField.SetTextWithoutNotify(model.Text);
            inputField.onEndEdit.RemoveListener(OnEndEdit);
            inputField.onEndEdit.AddListener(OnEndEdit);
        }

        private void OnEndEdit(string text) => model.Text = text;
    }
}
