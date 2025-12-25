using UnityEngine;
using Xeon.XDebugger.Control;

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

    }
}
