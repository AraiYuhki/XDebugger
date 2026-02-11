using UnityEngine.UI;

namespace Xeon.Style
{
    /// <summary>
    /// LayoutElementの推奨幅を設定するスタイル
    /// </summary>
    public class PreferredWidth : IStyle
    {
        private float width = -1f;

        /// <summary>
        /// 指定した値で推奨幅スタイルを初期化する
        /// </summary>
        /// <param name="width">推奨幅</param>
        public PreferredWidth(float width) => this.width = width;
        /// <inheritdoc/>
        public void Apply(LayoutElement layoutElement, Image background)
        {
            if (layoutElement == null) return;
            layoutElement.preferredWidth = width;
        }
    }

    /// <summary>
    /// LayoutElementの最小幅を設定するスタイル
    /// </summary>
    public class MinWidth : IStyle
    {
        private float width = -1f;
        /// <summary>
        /// 指定した値で最小幅スタイルを初期化する
        /// </summary>
        /// <param name="width">最小幅</param>
        public MinWidth(float width) => this.width = width;

        /// <inheritdoc/>
        public void Apply(LayoutElement layoutElement, Image background)
        {
            if (layoutElement == null) return;
            layoutElement.minWidth = width;
        }
    }

    /// <summary>
    /// LayoutElementのフレキシブル幅を設定するスタイル
    /// </summary>
    public class FlexibleWidth : IStyle
    {
        private float width = -1f;

        /// <summary>
        /// 指定した値でフレキシブル幅スタイルを初期化する
        /// </summary>
        /// <param name="width">フレキシブル幅</param>
        public FlexibleWidth(float width)
        {
            this.width = width;
        }

        /// <inheritdoc/>
        public void Apply(LayoutElement layoutElement, Image background)
        {
            if (layoutElement == null) return;
            layoutElement.flexibleWidth = width;
        }
    }
}
