using UnityEngine.UI;

namespace Xeon.Style
{
    /// <summary>
    /// LayoutElementの推奨高さを設定するスタイル
    /// </summary>
    public class PreferredHeight : IStyle
    {
        private float height = -1;

        /// <summary>
        /// 指定した値で推奨高さスタイルを初期化する
        /// </summary>
        /// <param name="height">推奨高さ</param>
        public PreferredHeight(float height) => this.height = height;

        /// <inheritdoc/>
        public void Apply(LayoutElement layoutElement, Image background)
        {
            if (layoutElement == null) return;
            layoutElement.preferredHeight = height;
        }
    }

    /// <summary>
    /// LayoutElementの最小高さを設定するスタイル
    /// </summary>
    public class MinHeight : IStyle
    {
        private float height = -1f;

        /// <summary>
        /// 指定した値で最小高さスタイルを初期化する
        /// </summary>
        /// <param name="height">最小高さ</param>
        public MinHeight(float height) => this.height = height;

        /// <inheritdoc/>
        public void Apply(LayoutElement layoutElement, Image background)
        {
            if (layoutElement == null) return;
            layoutElement.minHeight = height;
        }
    }

    /// <summary>
    /// LayoutElementのフレキシブル高さを設定するスタイル
    /// </summary>
    public class FlexibleHeight : IStyle
    {
        private float height = -1f;

        /// <summary>
        /// 指定した値でフレキシブル高さスタイルを初期化する
        /// </summary>
        /// <param name="height">フレキシブル高さ</param>
        public FlexibleHeight(float height) => this.height = height;

        /// <inheritdoc/>
        public void Apply(LayoutElement layoutElement, Image background)
        {
            if (layoutElement == null) return;
            layoutElement.flexibleHeight = height;
        }
    }
}
