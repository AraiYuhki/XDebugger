using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.Common;

namespace Xeon.XDebugger.Console
{
    public class ConsolePage : StaticPageControl
    {
        [SerializeField]
        private ConsoleController controller;

        public override string Title => "Console";

        public ConsoleController Controller => controller;

        public void Initialize(ILogDataBuffer logDataList)
        {
            controller.Initialize(logDataList);
        }

        public override void Setup(TabController tabController)
        {
            if (controller == null)
                return;

            // Initialize controller with shared log buffer from TabController
            controller.Initialize(TabController.LogBuffer);

            // Register for count update callbacks
            TabController.RegisterLogHandlers(controller.OnAddInfoLog, controller.OnAddWarningLog, controller.OnAddErrorLog);
            controller.OnAddInfoLog(TabController.LogBuffer.InfoCount);
            controller.OnAddWarningLog(TabController.LogBuffer.WarnCount);
            controller.OnAddErrorLog(TabController.LogBuffer.ErrorCount);
        }

        private void OnDestroy()
        {
            if (controller == null)
                return;

            // Unregister handlers to avoid dangling references
            TabController.UnregisterLogHandlers(controller.OnAddInfoLog, controller.OnAddWarningLog, controller.OnAddErrorLog);
        }

    }
}
