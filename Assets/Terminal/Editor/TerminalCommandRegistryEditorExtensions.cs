using XDebugger.Terminal;

namespace XDebugger.TerminalEditor
{
    public static class TerminalCommandRegistryEditorExtensions
    {
        public static void RegisterEditorCommands(this TerminalCommandRegistry registry)
        {
            registry.Register(new TerminalUnityCommands());
        }
    }
}
