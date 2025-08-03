using System;
using TMPro;
using UnityEngine;

namespace Xeon.XDebugger.Control
{
    public class StringControl : ControlBase
    {
        [SerializeField]
        protected TMP_InputField inputField;

        protected Func<string> getter;
        protected Action<string> setter;

        public void Setup(string title, string text, Func<string> getter, Action<string> setter)
        {
            Setup(title);
            this.getter = getter;
            this.setter = setter;

            inputField.SetTextWithoutNotify(text);
            inputField.onEndEdit.RemoveListener(OnEndEdit);
            inputField.onEndEdit.AddListener(OnEndEdit);
        }

        public override void Refresh()
        {
            if (getter == null) return;
            inputField.SetTextWithoutNotify(getter());
        }

        private void OnEndEdit(string text)
            => setter?.Invoke(text);
    }
}
