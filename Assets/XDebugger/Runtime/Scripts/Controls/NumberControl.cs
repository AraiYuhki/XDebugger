using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Xeon.XDebugger.Model;

namespace Xeon.XDebugger.Control
{
    /// <summary>
    /// 入力フィールドと増減ボタンで数値を操作するコントロール
    /// </summary>
    public class NumberControl : ControlBase
    {
        [SerializeField]
        protected TMP_InputField input;
        [SerializeField]
        protected Button rightButton;
        [SerializeField]
        protected Button leftButton;

        protected NumberModel model;

        /// <summary>
        /// 数値モデルを設定し、入力フィールドとボタンをバインドする
        /// </summary>
        /// <param name="model">数値のデータモデル</param>
        public void Setup(NumberModel model)
        {
            Setup(model.Title);
            if (this.model != null)
                this.model.Changed -= OnModelChanged;

            this.model = model;
            this.model.Changed += OnModelChanged;

            input.onEndEdit.RemoveListener(OnEndEdit);
            input.onEndEdit.AddListener(OnEndEdit);

            rightButton.onClick.RemoveListener(OnClickRightButton);
            rightButton.onClick.AddListener(OnClickRightButton);

            leftButton.onClick.RemoveListener(OnClickLeftButton);
            leftButton.onClick.AddListener(OnClickLeftButton);

            Refresh();
        }

        /// <summary>
        /// モデルの値で入力フィールドを更新する
        /// </summary>
        public override void Refresh()
        {
            input.SetTextWithoutNotify(model.Value.ToString());
        }

        protected virtual void OnEndEdit(string text)
        {
            if (float.TryParse(text, out var value))
                model.NotifyValueChangedFromView(value);
            else
                Refresh();
        }

        protected virtual void OnClickRightButton() => model.Value += model.Step;
        protected virtual void OnClickLeftButton() => model.Value -= model.Step;

        private void OnModelChanged() => Refresh();

        private void OnDestroy()
        {
            if (input != null)
                input.onEndEdit.RemoveListener(OnEndEdit);

            if (rightButton != null)
                rightButton.onClick.RemoveListener(OnClickRightButton);

            if (leftButton != null)
                leftButton.onClick.RemoveListener(OnClickLeftButton);

            if (model != null)
                model.Changed -= OnModelChanged;
        }
    }
}
