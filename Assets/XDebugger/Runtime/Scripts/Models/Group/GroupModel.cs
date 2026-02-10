using System.Collections.Generic;
using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Model
{
    /// <summary>
    /// 子コントロールをグループ化して管理する抽象データモデル。
    /// </summary>
    public abstract class GroupModel : ControlModelBase, IGroupModel
    {
        protected List<ControlModelBase> children = new();
        protected List<ControlBase> childrenControls = new();

        /// <summary>
        /// <see cref="GroupModel"/> のコンストラクタ。
        /// </summary>
        /// <param name="title">グループの表示タイトル。</param>
        /// <param name="priority">表示優先度。</param>
        public GroupModel(string title, int priority = 0) : base(title, priority)
        {
        }

        /// <summary>
        /// 親グループを指定する <see cref="GroupModel"/> のコンストラクタ。
        /// </summary>
        /// <param name="title">グループの表示タイトル。</param>
        /// <param name="parent">所属する親グループ。</param>
        /// <param name="priority">表示優先度。</param>
        public GroupModel(string title, IGroupModel parent, int priority = 0)
            : base(title, parent, priority)
        {
        }

        /// <summary>
        /// 子コントロールモデルをグループに追加する。
        /// </summary>
        /// <param name="model">追加する子コントロールモデル。</param>
        public void AddChild(ControlModelBase model) => children.Add(model);

        /// <summary>
        /// グループに所属する子コントロールモデルの読み取り専用リスト。
        /// </summary>
        public IReadOnlyList<ControlModelBase> Children => children;

        /// <inheritdoc/>
        public override ControlBase CreateControl(Transform parent, IUIFactory uiFactory)
        {
            var control = Instantiate<ContentGroup>(parent);
            control.Setup(this, parent.GetComponent<ContentGroup>());

            ResetChildren(control, uiFactory);

            return control;
        }

        protected void ResetChildren(ContentGroup control, IUIFactory uiFactory)
        {
            childrenControls.Clear();

            foreach (var child in children)
            {
                childrenControls.Add(child.CreateControl(control.GetContent(), uiFactory));
            }
        }
    }
}