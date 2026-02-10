namespace Xeon.XDebugger.Model
{
    /// <summary>
    /// 垂直レイアウトグループのスコープを管理するクラス
    /// </summary>
    public class VerticalLayoutScope : GroupLayoutScope
    {
        /// <summary>
        /// 垂直レイアウトスコープを初期化する
        /// </summary>
        /// <param name="title">グループのタイトル</param>
        /// <param name="parent">親ページモデル</param>
        /// <param name="priority">表示優先度</param>
        public VerticalLayoutScope(string title, PageModel parent, int priority = 0)
            : base(title, parent, priority)
        {
        }

        protected override ControlModelBase CreateModel(string title, int priority = 0)
            => new VerticalGroupModel(title, priority);
    }
}