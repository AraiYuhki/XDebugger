using System;
using UnityEngine;
using Xeon.Common;
using Xeon.XDebugger.Console;

namespace Xeon.XDebugger.Model
{
    public class ConsolePageModel : PageModel
    {
        private const int LogBufferCapacity = 1000;
        private static CircularBuffer<LogItemData> logDataList = new(LogBufferCapacity);
        private static int logIndex = 0;
        private static int infoCount = 0;
        private static int warningCount = 0;
        private static int errorCount = 0;

        protected override string prefabAddress => $"XDebugger/{nameof(ConsolePage)}";
        private ConsolePage page;

        private event Action<int> onAddInfoLog;
        private event Action<int> onAddWarningLog;
        private event Action<int> onAddErrorLog;

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


        public override void Initialize()
        {
        }

        private static float elapsed = 1f;
        private static void InternalUpdate()
        {
            elapsed -= Time.deltaTime;
            if (elapsed > 0f) return;
            switch (UnityEngine.Random.Range(0, 3))
            {
                case 0:
                    Debug.Log($"Test log {infoCount}");
                    break;
                case 1:
                    Debug.LogWarning($"Test warning {warningCount}");
                    break;
                default:
                    Debug.LogError($"Test error {errorCount}");
                    break;
            }
            ;
            elapsed = 0.5f;
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
            logDataList.PushBack(data);
            switch (type)
            {
                case LogType.Log:
                    infoCount++;
                    break;
                case LogType.Warning:
                    warningCount++;
                    break;
                default:
                    errorCount++;
                    break;
            }
        }
    }
}
