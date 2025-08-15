using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Xeon.XDebugger.Model;

namespace Xeon.XDebugger.Control
{
    public class IntSliderControl : ControlBase
    {
        [SerializeField]
        protected Slider slider;
        [SerializeField]
        protected TMP_InputField input;

        protected IntSliderModel model;

        protected Action<int> onValueChanged;

        public void Setup(IntSliderModel model, Action<int> onValueChanged)
        {
            this.model = model;
            Setup(model.Title);

            slider.wholeNumbers = true;

            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
            slider.onValueChanged.AddListener(OnSliderValueChanged);

            input.onEndEdit.RemoveListener(OnEndEdit);
            input.onEndEdit.AddListener(OnEndEdit);

            Refresh();
        }

        public override void Refresh()
        {
            slider.SetValueWithoutNotify(model.Value);
            input.SetTextWithoutNotify(model.Value.ToString());
        }

        protected virtual void OnSliderValueChanged(float value)
        {
            var tmp = Mathf.FloorToInt(value);
            input.SetTextWithoutNotify(tmp.ToString());
            onValueChanged?.Invoke(tmp);
        }

        protected virtual void OnEndEdit(string text)
        {
            if (!int.TryParse(text, out var tmp))
            {
                Refresh();
                return;
            }

            slider.SetValueWithoutNotify(tmp);
            onValueChanged?.Invoke(tmp);
        }
    }
}
