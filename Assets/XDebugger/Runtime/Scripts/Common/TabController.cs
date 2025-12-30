using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Xeon.XDebugger.Console;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.Model;

namespace Xeon.XDebugger.Common
{
    public class TabController : MonoBehaviour
    {
        [SerializeField]
        private Transform tabContainer;
        [SerializeField]
        private Transform pageContainer;
        [SerializeField]
        private ToggleGroup toggleGroup;

        [SerializeField]
        private List<TabData> tabList;

        public MainMenuTabPage MainMenuPage { get; private set; }

        // タブ切り替え時のイベント
        public event Action<TabData> OnTabChanged;

        private TabData activeTabData;

        private const int LogBufferCapacity = 1000;
        private static LogItemBuffer logDataList = new(LogBufferCapacity);
        private static int logIndex = 0;
        private static event Action<int> onAddInfoLog;
        private static event Action<int> onAddWarningLog;
        private static event Action<int> onAddErrorLog;

        private static float elapsed = 1f;

        public static void PreInitialize()
        {
            Application.logMessageReceived += OnReceivedLogMessage;
            Application.onBeforeRender += InternalUpdate;
            Application.quitting += () =>
            {
                Application.logMessageReceived -= OnReceivedLogMessage;
                Application.onBeforeRender -= InternalUpdate;
            };
        }

        // 公開: ページ側がログバッファへアクセスするためのプロパティ
        public static LogItemBuffer LogBuffer => logDataList;

        // 公開: ページ側がログカウント更新の購読/解除を行うためのヘルパー
        public static void RegisterLogHandlers(Action<int> onInfo, Action<int> onWarn, Action<int> onError)
        {
            onAddInfoLog += onInfo;
            onAddWarningLog += onWarn;
            onAddErrorLog += onError;
        }

        public static void UnregisterLogHandlers(Action<int> onInfo, Action<int> onWarn, Action<int> onError)
        {
            onAddInfoLog -= onInfo;
            onAddWarningLog -= onWarn;
            onAddErrorLog -= onError;
        }

        public void Setup(TabButton tabButtonPrefab, StaticPageControl[] pageList)
        {
            foreach (var tab in tabList)
            {
                Destroy(tab.TabButton);
                Destroy(tab.Content);
            }
            tabList.Clear();
            foreach (var tab in pageList)
            {
                var tabButton = Instantiate(tabButtonPrefab, tabContainer);
                var page = Instantiate(tab, pageContainer);

                // Ensure the instantiated page's RectTransform stretches to fill its parent
                var rectTransform = page.GetComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.sizeDelta = Vector2.zero;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;

                var data = new TabData(tabButton, page);
                data.Initialize(OnTabButtonChanged);
                tabButton.Toggle.group = toggleGroup;
                page.Setup(this);
                if (page is MainMenuTabPage mainMenuPage)
                    MainMenuPage = mainMenuPage;

                tabList.Add(data);
            }
            activeTabData = tabList.FirstOrDefault();
            activeTabData.TabButton.Toggle.isOn = true;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        /// <summary>
        /// シーン読み込み時の処理を委譲できるように public メソッドを用意
        /// </summary>
        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Additiveモードの場合は処理をスキップ
            if (mode == LoadSceneMode.Additive)
            {
                return;
            }

            // Singleモード（シーン切り替わり）の場合のみメインメニュータブの内容をリフレッシュ
            if (MainMenuPage != null && MainMenuPage.GetCurrentPage() != null)
            {
                MainMenuPage.RefreshCurrentPage(true);
            }
        }

        /// <summary>
        /// タブボタンが押された時のコールバック
        /// </summary>
        private void OnTabButtonChanged(TabData tabData)
        {
            activeTabData = tabData;
            OnTabChanged?.Invoke(tabData);
        }

        /// <summary>
        /// 現在アクティブなタブを取得
        /// </summary>
        public TabData GetActiveTabData() => activeTabData;

        private static void InternalUpdate()
        {
            elapsed -= Time.deltaTime;
            if (elapsed > 0f) return;
            switch (UnityEngine.Random.Range(0, 3))
            {
                case 0:
                    Debug.Log($"Test log {logDataList.InfoCount}");
                    break;
                case 1:
                    Debug.LogWarning($"Test warning {logDataList.WarnCount}");
                    break;
                default:
                    Debug.LogError($"Test error {logDataList.ErrorCount}");
                    break;
            }
            elapsed = 1f;
        }

        private static void OnReceivedLogMessage(string condition, string stackTrace, LogType type)
        {
            var data = new LogItemData(type, condition, stackTrace, logIndex, true);
            logIndex++;
            logDataList.Add(data);
            switch (type)
            {
                case LogType.Log:
                    onAddInfoLog?.Invoke(logDataList.InfoCount);
                    break;
                case LogType.Warning:
                    onAddWarningLog?.Invoke(logDataList.WarnCount);
                    break;
                default:
                    onAddErrorLog?.Invoke(logDataList.ErrorCount);
                    break;
            }
        }
    }
}
