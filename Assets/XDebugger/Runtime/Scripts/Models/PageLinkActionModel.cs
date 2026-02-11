using System;
using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Model
{
    /// <summary>
    /// ボタン押下時に指定ページへ遷移するアクションモデル。
    /// </summary>
    public class PageLinkActionModel : ActionModel
    {
        /// <summary>
        /// 遷移先のページモデル。
        /// </summary>
        public PageModel PageModel { get; private set; }

        /// <summary>
        /// <see cref="PageLinkActionModel"/> のコンストラクタ。
        /// </summary>
        /// <param name="title">表示タイトル。</param>
        /// <param name="pageModel">遷移先のページモデル。</param>
        /// <param name="priority">表示優先度。</param>
        public PageLinkActionModel(string title, PageModel pageModel, int priority = 0)
             : base(title, () => XDebugger.Instance.OpenPage(pageModel), priority)
        {
            PageModel = pageModel;
        }
    }
}