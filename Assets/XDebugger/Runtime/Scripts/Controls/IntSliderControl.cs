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

        public void Setup(IntSliderModel model)
        {
            Setup(model.Title);
            if (this.model != null)
                this.model.Changed -= OnModelChanged;

            this.model = model;
            this.model.Changed += OnModelChanged;

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
            model.NotifyValueChangedFromView(tmp);
        }

        protected virtual void OnEndEdit(string text)
        {
            if (!int.TryParse(text, out var tmp))
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
