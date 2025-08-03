using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Xeon.XDebugger.Model;

namespace Xeon.XDebugger.Control
{
    public class FloatSliderControl : ControlBase
    {
        [SerializeField]
        protected Slider slider;
        [SerializeField]
        protected TMP_InputField input;

        protected FloatSliderModel model;

        protected Action<float> onValueChanged;

        public void Setup(FloatSliderModel model, Action<float> onValueChanged)
        {
            Setup(model.Title);

            slider.wholeNumbers = false;

            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
            slider.onValueChanged.AddListener(OnSliderValueChanged);

            input.onEndEdit.RemoveListener(OnEndEdit);
            input.onEndEdit.AddListener(OnEndEdit);

            this.onValueChanged = onValueChanged;

            Refresh();
        }

        public override void Refresh()
        {
            slider.minValue = model.Min;
            slider.maxValue = model.Max;
            slider.SetValueWithoutNotify(model.Value);
            input.SetTextWithoutNotify(model.GetRoundedText);
        }

        protected virtual void OnSliderValueChanged(float value)
        {
            input.SetTextWithoutNotify(model.GetRoundedText);
            onValueChanged?.Invoke(value);
        }

        protected virtual void OnEndEdit(string text)
        {
            if (!float.TryParse(text, out var tmp))
            {
                Refresh();
                return;
            }

            slider.SetValueWithoutNotify(tmp);
            onValueChanged?.Invoke(tmp);
        }
    }
}
