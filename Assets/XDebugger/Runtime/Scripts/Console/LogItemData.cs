using UnityEngine;

namespace Xeon.XDebugger.Console
{
    public struct LogItemData
    {
        public LogType Type { get; }
        public string Contents { get; }
        public string StackTrace { get; }
        public bool IsVisible { get; set; }

        public int Index { get; }

        public LogItemData(LogType type, string contents, string stackTrace, int index, bool isVisible = true)
        {
            Type = type;
            Contents = contents;
            StackTrace = stackTrace;
            IsVisible = isVisible;
            Index = index;
        }

        public override string ToString()
        {
            var prefix = Type switch
            {
                LogType.Log => "[info]",
                LogType.Warning => "<color=yellow>[warning]</color>",
                LogType.Error => "<color=red>[error]</color>",
                LogType.Exception => "<color=red>[exception]</color>",
                LogType.Assert => "<color=red>[assert]</color>",
                _ => "<color=red>[unknown]</color>"
            };

            return $"{prefix} {Contents}\nStack trace: {StackTrace}";
        }
    }
}
