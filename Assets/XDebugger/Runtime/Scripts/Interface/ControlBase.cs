using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Xeon.Style;

namespace Xeon.XDebugger.Control
{
    /// <summary>
    /// 全てのUIコントロールの抽象基底クラス
    /// </summary>
    public abstract class ControlBase : MonoBehaviour
    {
        [SerializeField]
        protected Image background;
        [SerializeField]
        protected TMP_Text title;
        [SerializeField]
        protected LayoutElement layoutElement;

        /// <summary>
        /// タイトルテキストを設定する
        /// </summary>
        /// <param name="title">表示するタイトル文字列</param>
        public virtual void Setup(string title)
            => this.title.text = title;

        /// <summary>
        /// コントロールの表示を最新の状態に更新する
        /// </summary>
        public virtual void Refresh() { }

        /// <summary>
        /// スタイルを適用してレイアウトや背景の外観を変更する
        /// </summary>
        /// <param name="styles">適用するスタイルの配列</param>
        public void ApplyStyle(params IStyle[] styles)
        {
            foreach (var style in styles)
                style.Apply(layoutElement, background);
        }
    }
}
