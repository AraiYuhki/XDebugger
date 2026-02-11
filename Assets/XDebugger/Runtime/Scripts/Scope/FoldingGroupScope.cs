using UnityEngine;

namespace Xeon.XDebugger.Model
{
    /// <summary>
    /// 折りたたみグループのスコープを管理するクラス
    /// </summary>
    public class FoldingGroupScope : GroupLayoutScope
    {
        /// <summary>
        /// 折りたたみグループスコープを初期化する
        /// </summary>
        /// <param name="title">グループのタイトル</param>
        /// <param name="parent">親ページモデル</param>
        /// <param name="priority">表示優先度</param>
        public FoldingGroupScope(string title, PageModel parent, int priority = 0)
            : base(title, parent, priority)
        {
        }

        protected override ControlModelBase CreateModel(string title, int priority = 0)
            => new FoldingGroupModel(title, false, priority);
    }
}
