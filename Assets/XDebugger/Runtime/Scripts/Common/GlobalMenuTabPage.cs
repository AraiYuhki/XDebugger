using UnityEngine;
using Xeon.XDebugger.Model;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Control
{
    /// <summary>
    /// グローバルメニュータブ。Awakeで設定されたPageModelから自動で単一のページを生成・表示します。
    /// </summary>
    public class GlobalMenuTabPage : StaticPageControl
    {
        [SerializeField]
        private Transform content;

        [SerializeField]
        private UIFactoryBase uiFactory;

        [SerializeField]
        private ScriptablePageModel pageModel;

        private ScriptablePageModel displayedPage;

        private void Awake()
        {
            if (pageModel != null && content != null && uiFactory != null)
            {
                CreateAndDisplayPage();
            }
        }

        /// <summary>
        /// ページを生成して表示
        /// </summary>
        private void CreateAndDisplayPage()
        {
            displayedPage = pageModel;
            displayedPage.Initialize(uiFactory);
            displayedPage.OpenPage(content, displayedPage, uiFactory);
        }

        /// <summary>
        /// 表示中のページを更新
        /// </summary>
        public void RefreshPage()
        {
            if (displayedPage != null)
            {
                displayedPage.Refresh();
            }
        }

        /// <summary>
        /// 表示中のページを取得
        /// </summary>
        public IPageModel GetDisplayedPage() => displayedPage;

        /// <summary>
        /// PageModelを設定（Awakeの後に呼び出す場合用）
        /// </summary>
        public void SetPageModel(ScriptablePageModel model)
        {
            if (displayedPage != null)
            {
                displayedPage.Close();
            }

            pageModel = model;
            if (content != null && uiFactory != null)
            {
                CreateAndDisplayPage();
            }
        }
    }
}
