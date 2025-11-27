using UnityEngine;
using Xeon.XDebugger.Control;

namespace Xeon.XDebugger.Console
{
    public class ConsolePage : PageControl
    {
        [SerializeField]
        private ConsoleController controller;

        public ConsoleController Controller => controller;

        public void Initialize(ILogDataBuffer logDataList)
        {
            controller.Initialize(logDataList);
        }

    }
}
