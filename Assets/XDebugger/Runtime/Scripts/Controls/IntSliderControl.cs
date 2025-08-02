using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Xeon
{
    public class IntSliderControl : ControlBase
    {
        [SerializeField]
        protected Slider slider;
        [SerializeField]
        protected TMP_InputField input;

        protected int value = 0;
        protected int min = 0;
        protected int max = 255;

        protected Func<int> getter;
        protected Action<int> setter;

        public void Setup(string title, int value, int min, int max, Func<int> getter, Action<int> setter)
        {
            Setup(title);

            this.getter = getter;
            this.setter = setter;

            this.value = value;
            this.min = min;
            this.max = max;

            slider.minValue = min;
            slider.maxValue = max;
            slider.value = value;
            slider.wholeNumbers = true;

            input.text = value.ToString();

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
            input.SetTextWithoutNotify(value.ToString());
        }

        protected virtual void OnSliderValueChanged(float value)
        {
            this.value = Mathf.FloorToInt(value);
            input.SetTextWithoutNotify(value.ToString());
            setter?.Invoke(this.value);
        }

        protected virtual void OnEndEdit(string text)
        {
            if (!int.TryParse(text, out var tmp))
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
