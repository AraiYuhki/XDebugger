using System;
using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Model
{
    /// <summary>
    /// 数値の入力・増減操作を提供するコントロールのデータモデル。
    /// </summary>
    public class NumberModel : ControlModelBase
    {
        protected override string prefabAddress => $"XDebugger/{nameof(NumberControl)}";

        private float _value = 0f;
        private float step = 1f;

        private Action<float> onChangedValue;

        /// <summary>
        /// 現在の数値。設定時にコールバックは発火しない。
        /// </summary>
        public float Value
        {
            get => _value;
            set => SetValue(value, false);
        }

        /// <summary>
        /// 増減操作時のステップ幅。
        /// </summary>
        public float Step => step;

        /// <summary>
        /// <see cref="NumberModel"/> のコンストラクタ。
        /// </summary>
        /// <param name="title">表示タイトル。</param>
        /// <param name="value">初期値。</param>
        /// <param name="step">増減のステップ幅。</param>
        /// <param name="onChangedValue">値変更時のコールバック。</param>
        /// <param name="priority">表示優先度。</param>
        public NumberModel(string title, float value, float step, Action<float> onChangedValue, int priority = 0) : base(title, priority)
        {
            Initialize(value, step, onChangedValue);
        }

        /// <summary>
        /// 親グループを指定する <see cref="NumberModel"/> のコンストラクタ。
        /// </summary>
        /// <param name="title">表示タイトル。</param>
        /// <param name="value">初期値。</param>
        /// <param name="step">増減のステップ幅。</param>
        /// <param name="onChangedValue">値変更時のコールバック。</param>
        /// <param name="parent">所属する親グループ。</param>
        /// <param name="priority">表示優先度。</param>
        public NumberModel(string title, float value, float step, Action<float> onChangedValue, IGroupModel parent, int priority = 0)
            : base(title, parent, priority)
        {
            Initialize(value, step, onChangedValue);
        }

        private void Initialize(float value, float step, Action<float> onChangedValue)
        {
            this._value = value;
            this.step = step;
            this.onChangedValue = onChangedValue;
        }

        /// <inheritdoc/>
        public override ControlBase CreateControl(Transform parent, IUIFactory uiFactory)
        {
            var control = uiFactory.CreateControl<NumberControl>(parent);
            control.Setup(this);
            return control;
        }

        /// <summary>
        /// View側から値が変更されたことを通知し、コールバックを発火する。
        /// </summary>
        /// <param name="newValue">新しい数値。</param>
        public void NotifyValueChangedFromView(float newValue) => SetValue(newValue, true);

        /// <summary>
        /// 値を設定し、必要に応じてコールバックを発火する。
        /// </summary>
        /// <param name="newValue">新しい数値。</param>
        /// <param name="notifyCallback">trueの場合、値変更コールバックを発火する。</param>
        public void SetValue(float newValue, bool notifyCallback)
        {
            if (Mathf.Approximately(_value, newValue))
                return;

            _value = newValue;
            NotifyChanged();

            if (notifyCallback)
                onChangedValue?.Invoke(_value);
        }
    }
}
