using UnityEngine;
using Xeon.XDebugger.Common;
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
        private ScriptablePageModel pageModel;

        private ScriptablePageModel displayedPage;

        private void Awake()
        {
            // uiFactoryが設定されていない場合は、SetUIFactoryが呼ばれたときに初期化される
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
            if (uiFactory == null)
            {
                Debug.LogError("UIFactory is not set. Please ensure XDebuggerSetting has a UIFactory assigned and it is injected via XDebugger.");
                return;
            }

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
        /// UIFactoryを設定
        /// </summary>
        public override void SetUIFactory(UIFactoryBase uiFactory)
        {
            base.SetUIFactory(uiFactory);
            // UIFactoryが設定された後、pageModelが設定されているがまだ表示されていない場合は初期化
            if (pageModel != null && content != null && displayedPage == null)
            {
                CreateAndDisplayPage();
            }
            // 既にページが表示されている場合は再生成
            else if (displayedPage != null && content != null)
            {
                displayedPage.Close();
                CreateAndDisplayPage();
            }
        }

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
