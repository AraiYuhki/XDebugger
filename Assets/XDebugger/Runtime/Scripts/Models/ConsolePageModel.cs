using System;
using UnityEngine;
using Xeon.XDebugger.Console;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Model
{
    public class ConsolePageModel : PageModel
    {
        private const int LogBufferCapacity = 1000;
        private static LogItemBuffer logDataList = new(LogBufferCapacity);
        private static int logIndex = 0;
        private static event Action<int> onAddInfoLog;
        private static event Action<int> onAddWarningLog;
        private static event Action<int> onAddErrorLog;

        protected override string prefabAddress => $"XDebugger/{nameof(ConsolePage)}";
        private ConsolePage page;


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


        public override void Initialize(IUIFactory uiFactory)
        {
            this.uiFactory = uiFactory;
        }

        private static float elapsed = 1f;
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
            ;
            elapsed = 1f;
        }

        protected override void CreateControl(Transform parent, IUIFactory uiFactory)
        {
            control ??= uiFactory.CreatePage<ConsolePage>(parent);
        }

        protected override void OpenedPage()
        {
            page = control as ConsolePage;
            page.Initialize(logDataList);
            onAddInfoLog += page.Controller.OnAddInfoLog;
            onAddWarningLog += page.Controller.OnAddWarningLog;
            onAddErrorLog += page.Controller.OnAddErrorLog;
        }

        protected override void ClosedPage()
        {
            onAddInfoLog -= page.Controller.OnAddInfoLog;
            onAddWarningLog -= page.Controller.OnAddWarningLog;
            onAddErrorLog -= page.Controller.OnAddErrorLog;
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
