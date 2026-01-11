using UnityEngine;
using Xeon.XDebugger.Model;
using Xeon.XDebugger.UI;
using Xeon.XDebugger.Common;
using Xeon.XDebugger.Editor.Model;

namespace Xeon.XDebugger.Control
{
    /// <summary>
    /// グローバルメニュータブ。Awakeで設定されたPageModelから自動で単一のページを生成・表示します。
    /// </summary>
    public class GlobalMenuTabPage : StaticPageControl, IGetPageModel
    {
        [SerializeField]
        private Transform content;

        [SerializeField]
        private ScriptablePageModel pageModel;

        public IPageModel GetPageModel() => pageModel;

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

            if (pageModel == null)
            {
                Debug.LogError("PageModel is not set. Please ensure global menu tab page prefab has a PageModel assigned.");
                return;
            }
            pageModel.Initialize(uiFactory);
            pageModel.OpenPage(content, uiFactory);
        }

        /// <summary>
        /// 表示中のページを更新
        /// </summary>
        public void RefreshPage()
        {
            if (pageModel != null)
                pageModel.Refresh();
        }

        /// <summary>
        /// UIFactoryを設定
        /// </summary>
        public override void SetUIFactory(UIFactoryBase uiFactory)
        {
            base.SetUIFactory(uiFactory);
            // UIFactoryが設定された後、pageModelが設定されているがまだ表示されていない場合は初期化
            if (pageModel != null && content != null)
            {
                CreateAndDisplayPage();
            }
            // 既にページが表示されている場合は再生成
            else if (pageModel != null && content != null)
            {
                pageModel.Close();
                CreateAndDisplayPage();
            }
        }

        /// <summary>
        /// PageModelを設定（Awakeの後に呼び出す場合用）
        /// </summary>
        public void SetPageModel(ScriptablePageModel model)
        {
            if (pageModel != null)
                pageModel.Close();

            pageModel = model;
            if (content != null && uiFactory != null)
            {
                CreateAndDisplayPage();
            }
        }
    }
}
