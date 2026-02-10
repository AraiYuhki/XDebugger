using UnityEngine;
using UnityEngine.UI;

namespace Xeon.Style
{
    /// <summary>
    /// 背景色を設定するスタイル。アルファが0の場合は背景を非表示にする。
    /// </summary>
    public class BgColor : IStyle
    {
        private Color color;

        /// <summary>
        /// 指定した色で背景色スタイルを初期化する
        /// </summary>
        /// <param name="color">背景色</param>
        public BgColor(Color color) => this.color = color;
        /// <inheritdoc/>
        public void Apply(LayoutElement element, Image background)
        {
            if (background == null) return;
            background.color = color;
            background.gameObject.SetActive(color.a > 0f);
        }
    }
}
