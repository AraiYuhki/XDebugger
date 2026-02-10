using System;
using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Model
{
    /// <summary>
    /// 文字列の入力・編集を提供するコントロールのデータモデル。
    /// </summary>
    public class StringModel : ControlModelBase
    {
        protected override string prefabAddress => $"XDebugger/{nameof(StringControl)}";

        private string text = string.Empty;
        private Action<string> onChangedValue;

        /// <summary>
        /// 現在の文字列値。設定時にコールバックは発火しない。
        /// </summary>
        public string Text
        {
            get => text;
            set => SetText(value, false);
        }

        /// <summary>
        /// <see cref="StringModel"/> のコンストラクタ。
        /// </summary>
        /// <param name="title">表示タイトル。</param>
        /// <param name="text">初期テキスト。</param>
        /// <param name="onChangedValue">値変更時のコールバック。</param>
        /// <param name="priority">表示優先度。</param>
        public StringModel(string title, string text, Action<string> onChangedValue, int priority = 0) : base(title, priority)
        {
            Initialize(text, onChangedValue);
        }

        /// <summary>
        /// 親グループを指定する <see cref="StringModel"/> のコンストラクタ。
        /// </summary>
        /// <param name="title">表示タイトル。</param>
        /// <param name="text">初期テキスト。</param>
        /// <param name="onChangedValue">値変更時のコールバック。</param>
        /// <param name="parent">所属する親グループ。</param>
        /// <param name="priority">表示優先度。</param>
        public StringModel(string title, string text, Action<string> onChangedValue, IGroupModel parent, int priority = 0)
            : base(title, parent, priority)
        {
            Initialize(text, onChangedValue);
        }

        private void Initialize(string text, Action<string> onChangedValue)
        {
            this.text = text;
            this.onChangedValue = onChangedValue;
        }

        /// <inheritdoc/>
        public override ControlBase CreateControl(Transform parent, IUIFactory uiFactory)
        {
            var control = uiFactory.CreateControl<StringControl>(parent);
            control.Setup(this);
            return control;
        }

        /// <summary>
        /// View側からテキストが変更されたことを通知し、コールバックを発火する。
        /// </summary>
        /// <param name="newValue">新しい文字列値。</param>
        public void NotifyTextChangedFromView(string newValue) => SetText(newValue, true);

        /// <summary>
        /// テキストを設定し、必要に応じてコールバックを発火する。
        /// </summary>
        /// <param name="newValue">新しい文字列値。</param>
        /// <param name="notifyCallback">trueの場合、値変更コールバックを発火する。</param>
        public void SetText(string newValue, bool notifyCallback)
        {
            if (text == newValue)
                return;

            text = newValue;
            NotifyChanged();

            if (notifyCallback)
                onChangedValue?.Invoke(text);
        }
    }
}
