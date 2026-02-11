namespace Xeon.XDebugger.Model
{
    /// <summary>
    /// 水平レイアウトグループのスコープを管理するクラス
    /// </summary>
    public class HorizontalLayoutScope : GroupLayoutScope
    {
        /// <summary>
        /// 水平レイアウトスコープを初期化する
        /// </summary>
        /// <param name="title">グループのタイトル</param>
        /// <param name="parent">親ページモデル</param>
        /// <param name="priority">表示優先度</param>
        public HorizontalLayoutScope(string title, PageModel parent, int priority = 0)
            : base(title, parent, priority)
        {
        }

        protected override ControlModelBase CreateModel(string title, int priority = 0)
            => new HorizontalGroupModel(title, priority);
    }
}
