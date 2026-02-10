using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Xeon.XDebugger.Model;

namespace Xeon.XDebugger.Control
{
    /// <summary>
    /// スライダーと入力フィールドで整数値を操作するコントロール
    /// </summary>
    public class IntSliderControl : ControlBase
    {
        [SerializeField]
        protected Slider slider;
        [SerializeField]
        protected TMP_InputField input;

        protected IntSliderModel model;

        /// <summary>
        /// 整数スライダーモデルを設定し、UIと双方向バインドする
        /// </summary>
        /// <param name="model">整数スライダーのデータモデル</param>
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

        /// <summary>
        /// モデルの値でスライダーと入力フィールドを更新する
        /// </summary>
        public override void Refresh()
        {
            slider.minValue = model.Min;
            slider.maxValue = model.Max;
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
