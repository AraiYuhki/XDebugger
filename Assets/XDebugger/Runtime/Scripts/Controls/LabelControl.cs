using Xeon.XDebugger.Model;

namespace Xeon.XDebugger.Control
{
    /// <summary>
    /// テキストラベルを表示する読み取り専用コントロール
    /// </summary>
    public class LabelControl : ControlBase
    {
        protected LabelModel model;

        /// <summary>
        /// ラベルモデルを設定し、変更通知を購読する
        /// </summary>
        /// <param name="model">ラベルのデータモデル</param>
        public void Setup(LabelModel model)
        {
            if (this.model != null)
                this.model.Changed -= OnModelChanged;

            this.model = model;
            this.model.Changed += OnModelChanged;
            Refresh();
        }

        /// <summary>
        /// モデルのタイトルでラベルテキストを更新する
        /// </summary>
        public override void Refresh()
        {
            Setup(model.Title);
        }

        private void OnModelChanged() => Refresh();

        private void OnDestroy()
        {
            if (model != null)
                model.Changed -= OnModelChanged;
        }
    }
}
