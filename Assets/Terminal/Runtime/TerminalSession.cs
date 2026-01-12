using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Xeon.UniTerminal_X
{
    public sealed class TerminalSession
    {
        private string _currentDirectory;

        public TerminalSession(TerminalCommandRegistry registry, string initialDirectory = null)
        {
            Registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _currentDirectory = string.IsNullOrEmpty(initialDirectory) ? Application.dataPath : initialDirectory;
        }

        public TerminalCommandRegistry Registry { get; }

        public string CurrentDirectory => _currentDirectory;

        public Task<TerminalCommandResult> ExecuteAsync(string input)
        {
            var parsed = TerminalCommandParser.Parse(input);
            if (!parsed.HasValue)
            {
                return Task.FromResult(TerminalCommandResult.Ok());
            }

            var context = new TerminalCommandContext(() => _currentDirectory, SetCurrentDirectory);
            return Registry.ExecuteAsync(parsed.Name, context, parsed.Args);
        }

        private void SetCurrentDirectory(string path)
        {
            _currentDirectory = path;
        }
    }
}
