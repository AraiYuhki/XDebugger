using System;

namespace XDebugger.Terminal
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class TerminalCommandAttribute : Attribute
    {
        public TerminalCommandAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public string Description { get; set; } = string.Empty;
    }
}
