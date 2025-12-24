using System;
using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.Console;
using Xeon.XDebugger.Model;

namespace Xeon.XDebugger.Common
{
    [Serializable]
    public struct TabData
    {
        [SerializeField]
        private TabButton tabButton;
        [SerializeField]
        private StaticPageControl tabContent;

        public TabButton TabButton => tabButton;
        public StaticPageControl Content => tabContent;
    }

    public class TabController : MonoBehaviour
    {
        [SerializeField]
        private TabData mainMenuTabData;

        [SerializeField]
        private TabData profilerTabData;
        [SerializeField]
        private TabData consoleTabData;

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

        public void Awake()
        {
            mainMenuTabData.TabButton.Toggle.onValueChanged.AddListener(mainMenuTabData.Content.gameObject.SetActive);
            profilerTabData.TabButton.Toggle.onValueChanged.AddListener(profilerTabData.Content.gameObject.SetActive);
            consoleTabData.TabButton.Toggle.onValueChanged.AddListener(consoleTabData.Content.gameObject.SetActive);
            
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

        private void OnDestroy()
        {
            if (consolePage != null)
            {
                onAddInfoLog -= consolePage.Controller.OnAddInfoLog;
                onAddWarningLog -= consolePage.Controller.OnAddWarningLog;
                onAddErrorLog -= consolePage.Controller.OnAddErrorLog;
            }
        }

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
