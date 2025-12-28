using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Xeon.XDebugger.Common;
using Xeon.XDebugger.Control;
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
        private XDebuggerSetting setting;

        [SerializeField]
        private GameObject mainObject;

        [SerializeField]
        private TabController tabController;

        [SerializeField]
        private MainMenuTabPage mainMenuTab;

        [SerializeField]
        private TMP_Text titleLabel;

        [SerializeField]
        private Animator animator;

        [Header("Trigger")]
        [SerializeField]
        private int clickCount = 3; // メニュー表示のためのクリック回数
        [SerializeField]
        private float inputGraceTime = 0.2f; // 入力受付猶予時間

        private bool isShow = false; // メニュー表示状態
        private float elapsedTime = 0f; // 経過時間
        private int clickedCount = 0;   // クリック回数カウント

        public void SetUIFactory(UIFactoryBase uiFactory) => mainMenuTab?.SetUIFactory(uiFactory);
        
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
            mainObject.SetActive(false);
            isShow = false;
            
            TabController.PreInitialize();

            DontDestroyOnLoad(gameObject); // シーン切り替えでも破棄しない
        }

        private void Start()
        {
            tabController.Setup(setting.TabButtonPrefab, setting.TopPageTabList);
            mainMenuTab = tabController.MainMenuPage;
            // メインメニュータブを初期化
            if (mainMenuTab != null)
            {
                mainMenuTab.Initialize();
                // MainMenuTabPage のページ変更イベントを購読
                mainMenuTab.OnPageChanged += OnMainMenuPageChanged;
            }

            // タブコントローラーのイベントを購読
            if (tabController != null)
            {
                tabController.OnTabChanged += OnActiveTabChanged;
            }

            // SceneManager のシーン読み込みイベントを購読
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (mainMenuTab != null)
            {
                mainMenuTab.OnPageChanged -= OnMainMenuPageChanged;
            }

            if (tabController != null)
            {
                tabController.OnTabChanged -= OnActiveTabChanged;
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        /// <summary>
        /// シーンが読み込まれた時のコールバック
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Additiveモードの場合は処理をスキップ
            if (mode == LoadSceneMode.Additive)
            {
                return;
            }

            // Singleモード（シーン切り替わり）の場合のみリフレッシュ
            OnSceneChanged();
        }

        /// <summary>
        /// メインメニュータブ内のページが変更された時のコールバック
        /// </summary>
        private void OnMainMenuPageChanged(PageModel page)
        {
            if (titleLabel != null && page != null)
            {
                titleLabel.text = page.Title;
            }
        }

        /// <summary>
        /// アクティブなタブが変更された時のコールバック
        /// </summary>
        private void OnActiveTabChanged(TabData tabData)
        {
            // MainMenuTabPage の場合は何もしない（OnPageChanged で管理）
            // それ以外のタブの場合はそのタブのタイトルを表示
            if (!(tabData.Content is MainMenuTabPage))
            {
                if (titleLabel != null)
                {
                    titleLabel.text = tabData.Title;
                }
            }
        }

        /// <summary>
        /// シーンが切り替わった時のコールバック
        /// </summary>
        private void OnSceneChanged()
        {
            // MainMenuTabPageの内容をリフレッシュ
            if (mainMenuTab != null && mainMenuTab.GetCurrentPage() != null)
            {
                mainMenuTab.RefreshCurrentPage(true);
            }
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

            isShow = true;
            mainObject.SetActive(true);
            animator.Play(OpenId);

            if (mainMenuTab.GetCurrentPage() == null)
                OpenPage(GetOrCreateInitialPage());
            else
                mainMenuTab.RefreshCurrentPage(false);
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

        public void Refresh(bool doRecreate) => mainMenuTab.RefreshCurrentPage(doRecreate);

        /// <summary>
        /// メニュー非表示時の処理
        /// </summary>
        public void OnHidden()
        {
            isShow = false;
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
            if (mainMenuTab == null)
            {
                Debug.LogError("MainMenuTab is not assigned to XDebugger");
                return;
            }

            mainMenuTab.OpenPage(model);
        }

        /// <summary>
        /// ページを閉じる
        /// </summary>
        public void ClosePage(PageModel target)
        {
            if (mainMenuTab == null)
                return;

            mainMenuTab.ClosePage(target);
        }
    }
}
