using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Xeon.XDebugger.Model;

namespace Xeon.XDebugger.Control
{
    public class NumberControl : ControlBase
    {
        [SerializeField]
        protected TMP_InputField input;
        [SerializeField]
        protected Button rightButton;
        [SerializeField]
        protected Button leftButton;

        protected NumberModel model;
        protected Action<float> onChangedValue;

        public void Setup(NumberModel model, Action<float> onChangedValue)
        {
            Setup(model.Title);
            this.model = model;
            this.onChangedValue = onChangedValue;

            input.onEndEdit.RemoveListener(OnEndEdit);
            input.onEndEdit.AddListener(OnEndEdit);

            rightButton.onClick.RemoveListener(OnClickRightButton);
            leftButton.onClick.AddListener(OnClickLeftButton);

            Refresh();
        }

        public override void Refresh()
        {
            input.SetTextWithoutNotify(model.Value.ToString());
        }

        protected virtual void OnEndEdit(string text)
        {
            if (float.TryParse(text, out var value))
                onChangedValue?.Invoke(value);
            else
                Refresh();
        }

        protected virtual void OnClickRightButton() => model.Value += model.Step;
        protected virtual void OnClickLeftButton() => model.Value -= model.Step;
    }
}
