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

        protected float value = 0f;
        protected float step = 1f;
        protected Func<float> getter;
        protected Action<float> setter;

        public void Setup(string title, float value, float step, Func<float> getter, Action<float> setter)
        {
            Setup(title);
            this.getter = getter;
            this.setter = setter;

            this.value = value;
            this.step = step;

            input.SetTextWithoutNotify(value.ToString());
            input.onEndEdit.RemoveListener(OnEndEdit);
            input.onEndEdit.AddListener(OnEndEdit);

            rightButton.onClick.RemoveListener(OnClickRightButton);
            rightButton.onClick.AddListener(OnClickRightButton);

            leftButton.onClick.RemoveListener(OnClickLeftButton);
            leftButton.onClick.AddListener(OnClickLeftButton);
        }

        public override void Refresh()
        {
            if (getter == null) return;
            value = getter();
            input.SetTextWithoutNotify(value.ToString());
        }

        protected virtual void OnEndEdit(string text)
        {
            if (float.TryParse(text, out var value))
            {
                this.value = value;
                setter?.Invoke(value);
            }
            else
                Refresh();
        }

        protected virtual void OnClickRightButton()
        {
            value += step;
            OnChanged();
        }

        protected virtual void OnClickLeftButton()
        {
            value -= step;
            OnChanged();
        }

        protected virtual void OnChanged()
        {
            input.SetTextWithoutNotify(value.ToString());
            setter?.Invoke(value);
        }
    }
}
