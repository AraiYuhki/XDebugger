using System;

namespace XDebugger.Terminal
{
    public sealed class TerminalCommandContext
    {
        public TerminalCommandContext(Func<string> getCurrentDirectory, Action<string> setCurrentDirectory)
        {
            GetCurrentDirectory = getCurrentDirectory ?? throw new ArgumentNullException(nameof(getCurrentDirectory));
            SetCurrentDirectory = setCurrentDirectory ?? throw new ArgumentNullException(nameof(setCurrentDirectory));
        }

        public Func<string> GetCurrentDirectory { get; }
        public Action<string> SetCurrentDirectory { get; }
    }
}
