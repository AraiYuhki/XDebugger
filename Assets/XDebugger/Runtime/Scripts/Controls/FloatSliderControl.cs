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

        public void Setup(FloatSliderModel model)
        {
            Setup(model.Title);
            if (this.model != null)
                this.model.Changed -= OnModelChanged;

            this.model = model;
            this.model.Changed += OnModelChanged;

            slider.wholeNumbers = false;

            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
            slider.onValueChanged.AddListener(OnSliderValueChanged);

            input.onEndEdit.RemoveListener(OnEndEdit);
            input.onEndEdit.AddListener(OnEndEdit);

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
            model.NotifyValueChangedFromView(value);
        }

        protected virtual void OnEndEdit(string text)
        {
            if (!float.TryParse(text, out var tmp))
            {
                Refresh();
                return;
            }

            slider.SetValueWithoutNotify(tmp);
            model.NotifyValueChangedFromView(tmp);
        }

        private void OnModelChanged() => Refresh();

        private void OnDestroy()
        {
            if (slider != null)
                slider.onValueChanged.RemoveListener(OnSliderValueChanged);

            if (input != null)
                input.onEndEdit.RemoveListener(OnEndEdit);

            if (model != null)
                model.Changed -= OnModelChanged;
        }
    }
}
