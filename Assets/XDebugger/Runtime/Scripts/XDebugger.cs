using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using Xeon.XDebugger.Common;
using Xeon.XDebugger.Model;
using Xeon.XDebugger.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;
#endif

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

        // TriggerPositionに対応するアンカー設定
        private static readonly Dictionary<TriggerPosition, Vector2> AnchorSettings = new()
        {
            { TriggerPosition.TopLeft, new Vector2(0, 1) },
            { TriggerPosition.TopCenter, new Vector2(0.5f, 1) },
            { TriggerPosition.TopRight, new Vector2(1, 1) },
            { TriggerPosition.MiddleLeft, new Vector2(0, 0.5f) },
            { TriggerPosition.MiddleRight, new Vector2(1, 0.5f) },
            { TriggerPosition.BottomLeft, new Vector2(0, 0) },
            { TriggerPosition.BottomCenter, new Vector2(0.5f, 0) },
            { TriggerPosition.BottomRight, new Vector2(1, 0) },
        };

        /// <summary>
        /// シングルトンインスタンス取得
        /// </summary>
        public static XDebugger Instance
        {
            get
            {
                if (instance == null)
                {
                    var prefab = Addressables.LoadAssetAsync<GameObject>(nameof(XDebugger)).WaitForCompletion();
                    if (prefab == null)
                    {
                        Debug.LogError("XDebugger prefab not found in Addressables.");
                        return null;
                    }
                    var go = Instantiate(prefab);
                    if (go == null)
                    {
                        Debug.LogError("Failed to instantiate XDebugger prefab.");
                        return null;
                    }
                    instance = go.GetComponent<XDebugger>();

                    if (instance == null)
                    {
                        Debug.LogError("XDebugger component not found on instantiated prefab.");
                        return null;
                    }
                }
                return instance;
            }
        }
        private static PageModel initialPage;

        [SerializeField]
        private XDebuggerSetting setting;

        [SerializeField]
        private GameObject mainObject;

        [SerializeField]
        private TabController tabController;

        [SerializeField]
        private TMP_Text titleLabel;

        [SerializeField]
        private Animator animator;

        [SerializeField]
        private Button trigger;

        private bool isShow = false; // メニュー表示状態

        // タップ系の状態管理
        private float tapElapsedTime = 0f; // タップ入力の経過時間
        private int clickedCount = 0;   // クリック回数カウント

        // ホールド系の状態管理
        private float holdStartTime = 0f; // ホールド開始時間
        private bool isHolding = false; // ホールド中かどうか

        public IReadOnlyList<TabData> TabList => tabController?.TabList;

        public string Title => titleLabel.text;

        public UIFactoryBase UIFactory => setting.UIFactory;

        /// <summary>
        /// UIFactoryを設定（依存注入用）
        /// 設定が変更された場合に呼び出します。
        /// </summary>
        public void SetUIFactory(UIFactoryBase uiFactory)
        {
            if (tabController == null)
                return;

            tabController.SetUIFactory(uiFactory);
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
            mainObject.SetActive(false);
            isShow = false;

            TabController.PreInitialize();

            DontDestroyOnLoad(gameObject); // シーン切り替えでも破棄しない
        }

        private void Start()
        {
            if (tabController == null)
            {
                Debug.LogError("TabController is not assigned to XDebugger");
                return;
            }
            tabController.Setup(setting.UIFactory, setting.TabButtonPrefab, setting.TopPageTabList, title => titleLabel.text = title);
            InitializeTrigger();
        }

        private void InitializeTrigger()
        {
            var isTriggerButton = setting.TriggerMode is TriggerMode.DoubleTap or TriggerMode.TripleTap;
            trigger.gameObject.SetActive(isTriggerButton);
            if (!isTriggerButton)
            {
                return;
            }

            var rectTransform = trigger.GetComponent<RectTransform>();

            if (AnchorSettings.TryGetValue(setting.TriggerPosition, out var anchor))
            {
                rectTransform.anchorMin = anchor;
                rectTransform.anchorMax = anchor;
                rectTransform.pivot = anchor;
            }

            rectTransform.anchoredPosition = Vector2.zero;
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

            var mainMenuTab = tabController.MainMenuPage;

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

        public void Refresh(bool doRecreate) => tabController.MainMenuPage.RefreshCurrentPage(doRecreate);

        /// <summary>
        /// メニュー非表示時の処理
        /// </summary>
        public void OnHidden()
        {
            isShow = false;
        }

        /// <summary>
        /// トリガーとなるクリック処理（Triggerボタン用）
        /// </summary>
        public void OnClickTrigger()
        {
            if (setting == null) return;

            var triggerMode = setting.TriggerMode;
            if (triggerMode != TriggerMode.DoubleTap && triggerMode != TriggerMode.TripleTap)
                return;

            tapElapsedTime = 0f;
            clickedCount++;

            int requiredClicks = triggerMode == TriggerMode.DoubleTap ? 2 : 3;
            if (clickedCount >= requiredClicks)
            {
                Show();
                clickedCount = 0;
            }
        }

        private void Update()
        {
            if (setting == null) return;

            var triggerMode = setting.TriggerMode;

            // タップ系の処理
            if (triggerMode == TriggerMode.DoubleTap || triggerMode == TriggerMode.TripleTap)
            {
                HandleTapMode(triggerMode);
            }
            // ホールド系の処理
            else if (triggerMode == TriggerMode.DoubleFingerHold || triggerMode == TriggerMode.TripleFingerHold)
            {
                HandleHoldMode(triggerMode);
            }
        }

        /// <summary>
        /// タップ系モードの処理
        /// </summary>
        private void HandleTapMode(TriggerMode triggerMode)
        {
            if (tapElapsedTime > setting.InputGraceTime)
                clickedCount = 0;
            else
                tapElapsedTime += Time.deltaTime;
        }

        /// <summary>
        /// ホールド系モードの処理
        /// </summary>
        private void HandleHoldMode(TriggerMode triggerMode)
        {
            bool isHoldingInput = false;

            if (IsMobilePlatform())
            {
                isHoldingInput = HandleMobileHoldMode(triggerMode);
            }
            else if (IsConsolePlatform())
            {
                isHoldingInput = HandleConsoleHoldMode(triggerMode);
            }
            else
            {
                isHoldingInput = HandlePCHoldMode(triggerMode);
            }

            ProcessHoldInput(isHoldingInput);
        }

#if ENABLE_INPUT_SYSTEM
        private bool HandleMobileHoldModeForInputSystem(int requiredFingers)
        {
            return Touchscreen.current.touches.Count(touch => touch.phase.value
                is TouchPhase.Stationary
                or TouchPhase.Moved
                or TouchPhase.Began) >= requiredFingers;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        private bool HandleMobileHoldModeForLegacyInputManager(int requiredFingers)
        {
            return Input.touches.Count(touch => touch.phase
                is UnityEngine.TouchPhase.Began
                or UnityEngine.TouchPhase.Moved
                or UnityEngine.TouchPhase.Stationary) >= requiredFingers;
        }
#endif

        /// <summary>
        /// モバイルプラットフォーム用のホールド処理
        /// </summary>
        private bool HandleMobileHoldMode(TriggerMode triggerMode)
        {
            int requiredFingers = triggerMode == TriggerMode.DoubleFingerHold ? 2 : 3;

            var result = false;
#if ENABLE_INPUT_SYSTEM
            result = HandleMobileHoldModeForInputSystem(requiredFingers);
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            result |= HandleMobileHoldModeForLegacyInputManager(requiredFingers);
#endif

            return result;

        }

        /// <summary>
        /// コンソールプラットフォーム用のホールド処理
        /// </summary>
        private bool HandleConsoleHoldMode(TriggerMode triggerMode)
        {
            // 設定されたボタンを一定時間押し続ける
            // Unityの標準的なボタン名を使用（Fire2 = 右トリガー/右スティックボタンなど）
            return Input.GetButton("Fire2");
        }

        /// <summary>
        /// PCプラットフォーム用のホールド処理
        /// </summary>
        private bool HandlePCHoldMode(TriggerMode triggerMode)
        {
            var result = false;
            // 右クリックを一定時間ホールド
#if ENABLE_INPUT_SYSTEM
            // InputSystemが有効な場合
            var mouse = Mouse.current;
            if (mouse != null)
            {
                result = mouse.rightButton.isPressed;
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            // Legacy Input Managerが有効な場合
            result |= Input.GetMouseButton(1);
#endif
            return result;
        }

        /// <summary>
        /// ホールド入力の処理（共通ロジック）
        /// </summary>
        private void ProcessHoldInput(bool isHoldingInput)
        {
            if (isHoldingInput)
            {
                if (!isHolding)
                {
                    // ホールド開始
                    isHolding = true;
                    holdStartTime = Time.time;
                }
                else
                {
                    // ホールド継続中
                    float holdDuration = Time.time - holdStartTime;
                    if (holdDuration >= setting.HoldTimeForShow)
                    {
                        Show();
                        isHolding = false; // 表示後はリセット
                    }
                }
            }
            else
            {
                // ホールド解除
                isHolding = false;
                holdStartTime = 0f;
            }
        }

        /// <summary>
        /// モバイルプラットフォームかどうかを判定
        /// </summary>
        private bool IsMobilePlatform()
        {
            return Application.isMobilePlatform ||
                   Application.platform == RuntimePlatform.Android ||
                   Application.platform == RuntimePlatform.IPhonePlayer;
        }

        /// <summary>
        /// コンソールプラットフォームかどうかを判定
        /// </summary>
        private bool IsConsolePlatform()
        {
            return Application.platform == RuntimePlatform.PS4 ||
                   Application.platform == RuntimePlatform.PS5 ||
                   Application.platform == RuntimePlatform.XboxOne ||
                   Application.platform == RuntimePlatform.GameCoreXboxSeries ||
                   Application.platform == RuntimePlatform.GameCoreXboxOne ||
                   Application.platform == RuntimePlatform.Switch;
        }

        /// <summary>
        /// ページを開く
        /// </summary>
        public void OpenPage<T>(T model = null) where T : PageModel, new()
        {
            if (tabController == null || tabController.MainMenuPage == null)
            {
                Debug.LogError("MainMenuTab is not assigned to XDebugger");
                return;
            }

            tabController.MainMenuPage.OpenPage(model);
        }

        /// <summary>
        /// ページを閉じる
        /// </summary>
        public void ClosePage(PageModel target)
        {
            if (tabController == null || tabController.MainMenuPage == null)
                return;

            tabController.MainMenuPage.ClosePage(target);
        }
    }
}
