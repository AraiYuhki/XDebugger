using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Xeon.XDebugger.Model;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger
{
    /// <summary>
    /// デバッグ用UIを管理するシングルトンクラス。
    /// デバッグメニューの表示・非表示やページ遷移を制御します。
    /// </summary>
    public class XDebugger : MonoBehaviour
    {
        // アニメーションのステートID
        private static readonly int OpenId = Animator.StringToHash("Open");
        private static readonly int CloseId = Animator.StringToHash("Close");
        private static XDebugger instance;

        /// <summary>
        /// シングルトンインスタンス取得
        /// </summary>
        public static XDebugger Instance => instance;
        private static PageModel initialPage;

        [SerializeField]
        private GameObject mainMenu; // デバッグメニューのルートオブジェクト
        [SerializeField]
        private TMP_Text titleLabel; // タイトル表示用
        [SerializeField]
        private Animator animator;   // メニュー表示アニメーション
        [SerializeField]
        private Button backButton;   // 戻るボタン

        [Header("UI")]
        [SerializeField]
        private UIFactoryBase uiFactory;

        [Header("Trigger")]
        [SerializeField]
        private int clickCount = 3; // メニュー表示のためのクリック回数
        [SerializeField]
        private Transform content;  // ページ内容表示用
        [SerializeField]
        private float inputGraceTime = 0.2f; // 入力受付猶予時間

        private bool isShow = false; // メニュー表示状態
        private float elapsedTime = 0f; // 経過時間
        private int clickedCount = 0;   // クリック回数カウント
        private PageModel currentPage;  // 現在表示中のページ

        private List<PageModel> pageStack = new (); // ページ履歴スタック

        public void SetUIFactory(UIFactoryBase uiFactory)
        {
            this.uiFactory = uiFactory;
        }

        /// <summary>
        /// インスタンス初期化。シングルトン化と初期状態設定。
        /// </summary>
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;

            mainMenu.SetActive(false);
            isShow = false;
            ConsolePageModel.PreInitialize();

            DontDestroyOnLoad(gameObject); // シーン切り替えでも破棄しない
        }

        public static void SetInitialPage(PageModel model) => initialPage = model;

        /// <summary>
        /// 初期ページを取得または生成
        /// </summary>
        public static PageModel GetOrCreateInitialPage()
        {
            initialPage ??= new DefaultInitializePageModel();
            return initialPage;
        }

        /// <summary>
        /// デバッグメニューを表示
        /// </summary>
        public void Show()
        {
            if (isShow)
                return;
            
            backButton.gameObject.SetActive(pageStack.Count > 1);
            isShow = true;
            mainMenu.SetActive(true);
            animator.Play(OpenId);
            if (currentPage == null)
                OpenPage(GetOrCreateInitialPage());
            else
                currentPage.Refresh();
        }

        /// <summary>
        /// デバッグメニューを非表示
        /// </summary>
        public void Hide()
        {
            if (!isShow)
                return;

            isShow = false;
            animator.Play(CloseId);
        }

        /// <summary>
        /// ページ履歴を戻る
        /// </summary>
        public void Back()
        {
            if (pageStack.Count > 1)
                ClosePage(currentPage);
            backButton.gameObject.SetActive(pageStack.Count > 1);
        }

        /// <summary>
        /// メニュー非表示時の処理
        /// </summary>
        public void OnHidden()
        {
            isShow = false;
            mainMenu.SetActive(false);
        }

        /// <summary>
        /// トリガーとなるクリック処理
        /// </summary>
        public void OnClickTrigger()
        {
            elapsedTime = 0f;
            clickedCount++;
            if (clickedCount >= clickCount)
            {
                Show();
                clickedCount = 0;
            }
        }

        private void Update()
        {
            if (elapsedTime > inputGraceTime)
                clickedCount = 0;
            else
                elapsedTime += Time.deltaTime;
        }

        /// <summary>
        /// ページを開く
        /// </summary>
        public void OpenPage<T>(T model = null) where T : PageModel, new()
        {
            if (currentPage != null)
            {
                currentPage.Hide(() => CreatePage(model));
                return;
            }
            CreatePage(model);
        }

        /// <summary>
        /// ページを生成して表示
        /// </summary>
        private void CreatePage<T>(T model) where T : PageModel, new()
        {
            model ??= new T();
            model.Initialize(uiFactory);
            model.OpenPage(content, model, uiFactory);
            pageStack.Add(model);
            currentPage = model;
            titleLabel.text = currentPage.Title;
            backButton.gameObject.SetActive(pageStack.Count > 1);
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
                currentPage.Show(true);
                titleLabel.text = currentPage.Title;
            });
            backButton.gameObject.SetActive(pageStack.Count > 1);
        }
    }
}
