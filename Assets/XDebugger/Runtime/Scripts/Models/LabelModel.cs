using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Model
{
    /// <summary>
    /// 読み取り専用のラベルテキストを表示するコントロールのデータモデル。
    /// </summary>
    public class LabelModel : ControlModelBase
    {
        protected override string prefabAddress => $"XDebugger/{nameof(LabelControl)}";

        /// <summary>
        /// <see cref="LabelModel"/> のコンストラクタ。
        /// </summary>
        /// <param name="title">表示するラベルテキスト。</param>
        /// <param name="priority">表示優先度。</param>
        public LabelModel(string title, int priority = 0) : base(title, priority)
        {
        }

        /// <inheritdoc/>
        public override ControlBase CreateControl(Transform parent, IUIFactory uiFactory)
        {
            var control = uiFactory.CreateControl<LabelControl>(parent);
            control.Setup(this);
            return control;
        }

        /// <summary>
        /// ラベルのテキストを更新する。
        /// </summary>
        /// <param name="title">新しいラベルテキスト。</param>
        public void SetText(string title)
        {
            Title = title;
            NotifyChanged();
        }
    }
}
