using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Xeon.XDebugger.Model;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Control
{
    /// <summary>
    /// メインメニュータブ。ページスタック管理と動的ページ表示を担当します。
    /// </summary>
    public class MainMenuTabPage : StaticPageControl, IMainMenuTabPage
    {
        [SerializeField]
        private Transform content;

        [SerializeField]
        private Button backButton;

        [SerializeField]
        private UIFactoryBase uiFactory;

        public override string Title => currentPage == null ? "Main Menu" : currentPage.Title;

        // ページが変更された時のイベント
        public event Action<PageModel> OnPageChanged;

        private List<PageModel> pageStack = new();
        private PageModel currentPage;

        /// <summary>
        /// タブを初期化
        /// </summary>
        public void Initialize()
        {
            if (backButton != null)
                backButton.onClick.AddListener(Back);

            UpdateBackButtonState();
        }

        public void SetUIFactory(UIFactoryBase uiFactory) => this.uiFactory = uiFactory;

        /// <summary>
        /// ページを開く
        /// </summary>
        public void OpenPage<T>(T model = null) where T : PageModel, new()
        {
            if (currentPage != null)
            {
                currentPage.Hide(() => CreateAndShowPage(model));
                return;
            }

            CreateAndShowPage(model);
        }

        public void RefreshCurrentPage(bool doRecreate)
        {
            if (currentPage == null)
            {
                return;
            }
            if (doRecreate)
            {
                currentPage.Refresh(content);
                return;
            }
            currentPage.Refresh();
        }

        /// <summary>
        /// ページを生成して表示
        /// </summary>
        private void CreateAndShowPage<T>(T model) where T : PageModel, new()
        {
            model ??= new T();
            model.Initialize(uiFactory);
            model.OpenPage(content, model, uiFactory);
            pageStack.Add(model);
            currentPage = model;

            UpdateBackButtonState();
            OnPageChanged?.Invoke(currentPage);
        }

        /// <summary>
        /// ページを閉じる
        /// </summary>
        public void ClosePage(PageModel target)
        {
            pageStack.Remove(target);

            if (currentPage != target)
                return;

            currentPage = null;
            target.Close(() =>
            {
                currentPage = pageStack.LastOrDefault();
                if (currentPage != null)
                {
                    currentPage.Show(true);
                    OnPageChanged?.Invoke(currentPage);
                }
                else
                {
                    OnPageChanged?.Invoke(null);
                }

                UpdateBackButtonState();
            });
        }

        /// <summary>
        /// ページ履歴を戻る
        /// </summary>
        public void Back()
        {
            if (pageStack.Count > 1)
                ClosePage(currentPage);
        }

        /// <summary>
        /// 戻るボタンの表示状態を更新
        /// </summary>
        private void UpdateBackButtonState()
        {
            if (backButton != null)
                backButton.gameObject.SetActive(pageStack.Count > 1);
        }

        /// <summary>
        /// 現在のページを取得
        /// </summary>
        public PageModel GetCurrentPage() => currentPage;

        /// <summary>
        /// ページスタックをクリア
        /// </summary>
        public void ClearPageStack()
        {
            foreach (var page in pageStack)
            {
                if (page != null)
                    GameObject.Destroy(page.GetControl()?.gameObject);
            }
            pageStack.Clear();
            currentPage = null;
            UpdateBackButtonState();
        }
    }
}
