namespace Xeon.XDebugger.Model
{
    /// <summary>
    /// 無効化グループのスコープを管理するクラス
    /// </summary>
    public class DisableGroupScope : GroupLayoutScope
    {
        /// <summary>
        /// 無効化グループスコープを初期化する
        /// </summary>
        /// <param name="title">グループのタイトル</param>
        /// <param name="parent">親ページモデル</param>
        /// <param name="priority">表示優先度</param>
        public DisableGroupScope(string title, PageModel parent, int priority = 0)
            : base(title, parent, priority)
        {
        }

        protected override ControlModelBase CreateModel(string title, int priority = 0)
            => new DisableGroupModel(title, priority);
    }
}
