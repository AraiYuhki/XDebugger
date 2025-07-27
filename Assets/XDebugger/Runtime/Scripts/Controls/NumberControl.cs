using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Xeon
{
    public class NumberControl : ControlBase
    {
        [SerializeField]
        protected TMP_InputField input;
        [SerializeField]
        protected Button rightButton;
        [SerializeField]
        protected Button leftButton;

        protected Func<float> getter;
        protected Action<float> setter;

        public void Setup(string title, float value, Func<float> getter, Action<float> setter)
        {
            Setup(title);
            this.getter = getter;
            this.setter = setter;

            input.SetTextWithoutNotify(value.ToString());
            input.onEndEdit.RemoveListener(OnEndEdit);
            input.onEndEdit.AddListener(OnEndEdit);
        }

        public override void Refresh()
        {
            if (getter == null) return;
            input.SetTextWithoutNotify(getter().ToString());
        }

        private void OnEndEdit(string text)
        {
            if (float.TryParse(text, out var value))
                setter?.Invoke(value);
            else
                Refresh();
        }
    }
}
