using UnityEngine;
using UnityEngine.UI;
using Xeon.XDebugger.Model;

namespace Xeon.XDebugger.Control
{
    /// <summary>
    /// トグルUIで真偽値を操作するコントロール
    /// </summary>
    public class BoolControl : ControlBase
    {
        [SerializeField]
        private Toggle toggle;

        protected BoolModel model;

        /// <summary>
        /// 真偽値モデルを設定し、トグルUIと双方向バインドする
        /// </summary>
        /// <param name="model">真偽値のデータモデル</param>
        public void Setup(BoolModel model)
        {
            Setup(model.Title);
            if (this.model != null)
                this.model.Changed -= OnModelChanged;

            this.model = model;
            this.model.Changed += OnModelChanged;

            toggle.onValueChanged.RemoveListener(OnValueChanged);
            toggle.onValueChanged.AddListener(OnValueChanged);

            Refresh();
        }

        /// <summary>
        /// モデルの値でトグルUIを更新する
        /// </summary>
        public override void Refresh()
        {
            toggle.SetIsOnWithoutNotify(model.Value);
        }

        private void OnValueChanged(bool flag)
            => model.NotifyValueChangedFromView(flag);

        private void OnModelChanged() => Refresh();

        private void OnDestroy()
        {
            if (toggle != null)
                toggle.onValueChanged.RemoveListener(OnValueChanged);

            if (model != null)
                model.Changed -= OnModelChanged;
        }
    }
}
