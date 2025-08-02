using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Xeon
{
    public class FloatSliderControl : ControlBase
    {
        [SerializeField]
        protected Slider slider;
        [SerializeField]
        protected TMP_InputField input;

        protected float value = 0f;
        protected float min = 0f;
        protected float max = 1f;
        protected int decimalPlace = 2;

        protected Func<float> getter;
        protected Action<float> setter;

        public void Setup(string title, float value, float min, float max, Func<float> getter, Action<float> setter, int decimalPlace = 2)
        {
            Setup(title);

            this.getter = getter;
            this.setter = setter;

            this.value = value;
            this.min = min;
            this.max = max;
            this.decimalPlace = decimalPlace;

            slider.minValue = min;
            slider.maxValue = max;
            slider.value = value;
            slider.wholeNumbers = false;

            input.text = Math.Round(value, decimalPlace).ToString();

            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
            slider.onValueChanged.AddListener(OnSliderValueChanged);

            input.onEndEdit.RemoveListener(OnEndEdit);
            input.onEndEdit.AddListener(OnEndEdit);
        }

        public override void Refresh()
        {
            if (getter == null) return;

            value = getter();
            slider.SetValueWithoutNotify(value);
            input.SetTextWithoutNotify(Math.Round(value, decimalPlace).ToString());
        }

        protected virtual void OnSliderValueChanged(float value)
        {
            this.value = value;
            input.SetTextWithoutNotify(Math.Round(value, decimalPlace).ToString());
            setter?.Invoke(this.value);
        }

        protected virtual void OnEndEdit(string text)
        {
            if (!float.TryParse(text, out var tmp))
            {
                Refresh();
                return;
            }

            value = tmp;
            slider.SetValueWithoutNotify(value);
            setter?.Invoke(value);
        }
    }
}
