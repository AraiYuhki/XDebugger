using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.Common;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Console
{
    public class ConsolePage : StaticPageControl, IConsolePage
    {
        [SerializeField]
        private ConsoleController controller;

        private bool isSetup = false;

        public ConsoleController Controller => controller;

        /// <summary>
        /// 外部からログバッファを指定して初期化する場合に使用します。
        /// Setup()が呼ばれる前に使用することを想定しています。
        /// </summary>
        public void Initialize(ILogDataBuffer logDataList)
        {
            if (controller == null)
                return;

            controller.Initialize(logDataList);
        }

        public override void Setup(TabController tabController, UIFactoryBase uiFactory)
        {
            if (controller == null)
                return;

            // 既にSetup済みの場合は、ハンドラの再登録のみ行う
            if (isSetup)
            {
                // 一度解除してから再登録（重複防止）
                TabController.UnregisterLogHandlers(controller.OnAddInfoLog, controller.OnAddWarningLog, controller.OnAddErrorLog);
                TabController.RegisterLogHandlers(controller.OnAddInfoLog, controller.OnAddWarningLog, controller.OnAddErrorLog);
                return;
            }

            base.Setup(tabController, uiFactory);

            // Initialize controller with shared log buffer from TabController
            controller.Initialize(TabController.LogBuffer);

            // Register for count update callbacks
            TabController.RegisterLogHandlers(controller.OnAddInfoLog, controller.OnAddWarningLog, controller.OnAddErrorLog);

            isSetup = true;
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
