using System;

namespace Xeon.XDebugger.Model
{
    /// <summary>
    /// usingステートメントでグループレイアウトのスコープを管理する抽象基底クラス
    /// </summary>
    public abstract class GroupLayoutScope : IDisposable
    {
        protected PageModel parent;
        protected IGroupModel prevModel;

        protected ControlModelBase model;

        /// <summary>
        /// このスコープに紐づくコントロールモデル
        /// </summary>
        public ControlModelBase Model => model;

        /// <summary>
        /// グループレイアウトスコープを初期化し、親ページに新しいグループを設定する
        /// </summary>
        /// <param name="title">グループのタイトル</param>
        /// <param name="parent">親ページモデル</param>
        /// <param name="priority">表示優先度</param>
        public GroupLayoutScope(string title, PageModel parent, int priority = 0)
        {
            this.parent = parent;
            model = CreateModel(title, priority);
            prevModel = parent.CurrentGroup;
        }

        /// <summary>
        /// スコープ終了時にグループを以前の状態に復元する
        /// </summary>
        public void Dispose() => parent.SetGroup(prevModel);

        protected abstract ControlModelBase CreateModel(string title, int priority = 0);
    }
}
