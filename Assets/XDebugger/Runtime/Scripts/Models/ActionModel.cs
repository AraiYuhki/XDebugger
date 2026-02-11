using System;
using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Model
{
    /// <summary>
    /// ボタン押下時にアクションを実行するコントロールのデータモデル。
    /// </summary>
    public class ActionModel : ControlModelBase
    {
        protected override string prefabAddress => $"XDebugger/{nameof(ActionControl)}";

        protected ActionControl control;
        protected Action action;

        /// <summary>
        /// <see cref="ActionModel"/> のコンストラクタ。
        /// </summary>
        /// <param name="title">表示タイトル。</param>
        /// <param name="action">ボタン押下時に実行されるアクション。</param>
        /// <param name="priority">表示優先度。</param>
        public ActionModel(string title, Action action, int priority = 0) : base(title, priority)
        {
            this.action = action;
        }

        /// <summary>
        /// 登録されたアクションを実行する。
        /// </summary>
        public void ExecuteMethod() => action?.Invoke();

        /// <inheritdoc/>
        public override ControlBase CreateControl(Transform parent, IUIFactory uiFactory)
        {
            control = uiFactory.CreateControl<ActionControl>(parent);
            control.Setup(this);
            return control;
        }
    }
}