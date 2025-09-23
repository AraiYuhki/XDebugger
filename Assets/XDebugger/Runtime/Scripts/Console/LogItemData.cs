using UnityEngine;

namespace Xeon.XDebugger.Console
{
    public struct LogItemData
    {
        public LogType Type { get; }
        public string Contents { get; }
        public string StackTrace { get; }
        public bool IsVisible { get; set; }

        public LogItemData(LogType type, string contents, string stackTrace, bool isVisible = true)
        {
            Type = type;
            Contents = contents;
            StackTrace = stackTrace;
            IsVisible = isVisible;
        }
    }
}
