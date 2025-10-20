using UnityEngine;
using Xeon.Common.FlyweightScrollView;
using Xeon.XDebugger.Control;

namespace Xeon.XDebugger.Console
{
    public class ConsolePage : PageControl
    {
        [SerializeField]
        private ConsoleController controller;

        public ConsoleController Controller => controller;

        public void Initialize(IObservableCollection<LogItemData> logDataList)
        {
            controller.Initialize(logDataList);
        }

    }
}
