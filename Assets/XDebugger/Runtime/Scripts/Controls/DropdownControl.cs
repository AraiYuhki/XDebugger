using TMPro;
using UnityEngine;
using Xeon.XDebugger.Model;

namespace Xeon.XDebugger.Control
{

    /// <summary>
    /// ドロップダウンUIで選択肢を操作するコントロール
    /// </summary>
    public class DropdownControl : ControlBase
    {
        [SerializeField]
        protected TMP_Dropdown dropdown;

        protected IDropdownModel model;

        /// <summary>
        /// ドロップダウンモデルを設定し、選択肢を初期化する
        /// </summary>
        /// <param name="model">ドロップダウンのデータモデル</param>
        public void Setup(IDropdownModel model)
        {
            Setup(model.Title);
            if (this.model != null)
                this.model.Changed -= OnModelChanged;

            this.model = model;
            this.model.Changed += OnModelChanged;

            dropdown.onValueChanged.RemoveListener(OnChangedValue);
            dropdown.onValueChanged.AddListener(OnChangedValue);
            Refresh();
        }

        /// <summary>
        /// ドロップダウンの選択肢と選択状態を更新する
        /// </summary>
        public override void Refresh()
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(model.Labels);
            dropdown.SetValueWithoutNotify(model.SelectedIndex);
        }

        protected virtual void OnChangedValue(int index) => model.NotifySelectedIndexChangedFromView(index);

        private void OnModelChanged() => Refresh();

        private void OnDestroy()
        {
            if (dropdown != null)
                dropdown.onValueChanged.RemoveListener(OnChangedValue);

            if (model != null)
                model.Changed -= OnModelChanged;
        }
    }
}
