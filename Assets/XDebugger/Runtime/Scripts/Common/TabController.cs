using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Xeon.XDebugger.Console;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.Model;

namespace Xeon.XDebugger.Common
{
    public class TabController : MonoBehaviour
    {
        [SerializeField]
        private TabData mainMenuTabData;
        [SerializeField]
        private TabData globalMenuTabData;
        [SerializeField]
        private TabData systemInfoTabData;
        [SerializeField]
        private TabData profilerTabData;
        [SerializeField]
        private TabData consoleTabData;

        [SerializeField]
        private Transform tabContainer;
        [SerializeField]
        private Transform pageContainer;
        [SerializeField]
        private ToggleGroup toggleGroup;

        [SerializeField]
        private List<TabData> tabList;

        public MainMenuTabPage MainMenuPage => mainMenuTabData == null ? null : mainMenuTabData.Content as MainMenuTabPage;

        // タブ切り替え時のイベント
        public event Action<TabData> OnTabChanged;

        private TabData activeTabData;

        private const int LogBufferCapacity = 1000;
        private static LogItemBuffer logDataList = new(LogBufferCapacity);
        private static int logIndex = 0;
        private static event Action<int> onAddInfoLog;
        private static event Action<int> onAddWarningLog;
        private static event Action<int> onAddErrorLog;

        private ConsolePage consolePage;
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

        public void Setup(TabButton tabButtonPrefab, StaticPageControl[] pageList)
        {
            foreach (var tab in tabList)
            {
                Destroy(tab.TabButton);
                Destroy(tab.Content);
            }
            tabList.Clear();
            consolePage = null;
            foreach (var tab in pageList)
            {
                var tabButton = Instantiate(tabButtonPrefab, tabContainer);
                var page = Instantiate(tab, pageContainer);
                var data = new TabData(tabButton, page);
                data.Initialize(OnTabButtonChanged);
                tabButton.Toggle.group = toggleGroup;
                if (page is ConsolePage consolePage)
                    InitializeConsolePage(consolePage);
                else if (page is MainMenuTabPage mainMenuPage)
                    mainMenuTabData = data;

                tabList.Add(data);
            }
            activeTabData = tabList.FirstOrDefault();
            activeTabData.TabButton.Toggle.isOn = true;
        }

        private void InitializeConsolePage(ConsolePage page)
        {
            if (consolePage != null)
            {
                throw new Exception("ConsolePage is already initialized.");
            }
            consolePage = consoleTabData.Content as ConsolePage;
            if (consolePage != null)
            {
                consolePage.Initialize(logDataList);
                onAddInfoLog += consolePage.Controller.OnAddInfoLog;
                onAddWarningLog += consolePage.Controller.OnAddWarningLog;
                onAddErrorLog += consolePage.Controller.OnAddErrorLog;

                // 既存のログカウントを初期表示
                consolePage.Controller.OnAddInfoLog(logDataList.InfoCount);
                consolePage.Controller.OnAddWarningLog(logDataList.WarnCount);
                consolePage.Controller.OnAddErrorLog(logDataList.ErrorCount);
            }
        }

        public void Awake()
        {
            mainMenuTabData.Initialize(OnTabButtonChanged);
            globalMenuTabData.Initialize(OnTabButtonChanged);
            systemInfoTabData.Initialize(OnTabButtonChanged);
            profilerTabData.Initialize(OnTabButtonChanged);
            consoleTabData.Initialize(OnTabButtonChanged);

            foreach (var tabData in tabList)
                tabData.Initialize(OnTabButtonChanged);
            
            consolePage = consoleTabData.Content as ConsolePage;
            if (consolePage != null)
            {
                consolePage.Initialize(logDataList);
                onAddInfoLog += consolePage.Controller.OnAddInfoLog;
                onAddWarningLog += consolePage.Controller.OnAddWarningLog;
                onAddErrorLog += consolePage.Controller.OnAddErrorLog;
                
                // 既存のログカウントを初期表示
                consolePage.Controller.OnAddInfoLog(logDataList.InfoCount);
                consolePage.Controller.OnAddWarningLog(logDataList.WarnCount);
                consolePage.Controller.OnAddErrorLog(logDataList.ErrorCount);
            }

            // 初期タブをメインメニューに設定
            activeTabData = mainMenuTabData;
        }

        private void OnDestroy()
        {
            if (consolePage != null)
            {
                onAddInfoLog -= consolePage.Controller.OnAddInfoLog;
                onAddWarningLog -= consolePage.Controller.OnAddWarningLog;
                onAddErrorLog -= consolePage.Controller.OnAddErrorLog;
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
