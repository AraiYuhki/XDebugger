using Xeon.UniTerminal_X;

namespace Xeon.UniTerminal_X.Editor
{
    public static class TerminalCommandRegistryEditorExtensions
    {
        public static void RegisterEditorCommands(this TerminalCommandRegistry registry)
        {
            registry.Register(new TerminalUnityCommands());
        }
    }
}
