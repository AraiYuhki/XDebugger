using System;
using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Model
{
    /// <summary>
    /// ブール値のトグル操作を提供するコントロールのデータモデル。
    /// </summary>
    public class BoolModel : ControlModelBase
    {
        protected override string prefabAddress => $"XDebugger/{nameof(BoolControl)}";

        private bool isOn = false;
        private Action<bool> onChangedValue;

        /// <summary>
        /// 現在のブール値。設定時にコールバックは発火しない。
        /// </summary>
        public bool Value
        {
            get => isOn;
            set => SetValue(value, false);
        }

        /// <summary>
        /// <see cref="BoolModel"/> のコンストラクタ。
        /// </summary>
        /// <param name="title">表示タイトル。</param>
        /// <param name="isOn">初期値。</param>
        /// <param name="onChangedValue">値変更時のコールバック。</param>
        /// <param name="priority">表示優先度。</param>
        public BoolModel(string title, bool isOn, Action<bool> onChangedValue, int priority = 0) : base(title, priority)
        {
            Initialize(isOn, onChangedValue);
        }

        /// <summary>
        /// 親グループを指定する <see cref="BoolModel"/> のコンストラクタ。
        /// </summary>
        /// <param name="title">表示タイトル。</param>
        /// <param name="isOn">初期値。</param>
        /// <param name="onChangedValue">値変更時のコールバック。</param>
        /// <param name="parent">所属する親グループ。</param>
        /// <param name="priority">表示優先度。</param>
        public BoolModel(string title, bool isOn, Action<bool> onChangedValue, IGroupModel parent, int priority = 0)
            : base(title, parent, priority)
        {
            Initialize(isOn, onChangedValue);
        }

        private void Initialize(bool isOn, Action<bool> onChangedValue)
        {
            this.isOn = isOn;
            this.onChangedValue = onChangedValue;
        }

        /// <inheritdoc/>
        public override ControlBase CreateControl(Transform parent, IUIFactory uiFactory)
        {
            var control = uiFactory.CreateControl<BoolControl>(parent);
            control.Setup(this);
            return control;
        }

        /// <summary>
        /// View側から値が変更されたことを通知し、コールバックを発火する。
        /// </summary>
        /// <param name="newValue">新しいブール値。</param>
        public void NotifyValueChangedFromView(bool newValue) => SetValue(newValue, true);

        /// <summary>
        /// 値を設定し、必要に応じてコールバックを発火する。
        /// </summary>
        /// <param name="newValue">新しいブール値。</param>
        /// <param name="notifyCallback">trueの場合、値変更コールバックを発火する。</param>
        public void SetValue(bool newValue, bool notifyCallback)
        {
            if (isOn == newValue)
                return;

            isOn = newValue;
            NotifyChanged();

            if (notifyCallback)
                onChangedValue?.Invoke(isOn);
        }
    }
}
