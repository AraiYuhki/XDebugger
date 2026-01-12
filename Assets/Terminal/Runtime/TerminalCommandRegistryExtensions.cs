namespace XDebugger.Terminal
{
    public static class TerminalCommandRegistryExtensions
    {
        public static void RegisterDefaultCommands(this TerminalCommandRegistry registry)
        {
            registry.Register(new TerminalFileCommands());
        }
    }
}
