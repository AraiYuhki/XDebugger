using System;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace Xeon.UniTerminal_X
{
    public sealed class TerminalController : MonoBehaviour
    {
        [SerializeField]
        private string initialDirectory = string.Empty;

        private TerminalCommandRegistry _registry;
        private TerminalSession _session;

        public TerminalCommandRegistry Registry => _registry;
        public string CurrentDirectory => _session?.CurrentDirectory ?? string.Empty;

        private void Awake()
        {
            EnsureInitialized();
        }

        public void EnsureInitialized()
        {
            if (_registry != null)
            {
                return;
            }

            _registry = new TerminalCommandRegistry();
            _registry.RegisterDefaultCommands();
            _session = new TerminalSession(_registry, string.IsNullOrWhiteSpace(initialDirectory) ? null : initialDirectory);
        }

        public void RegisterCommands(object provider)
        {
            EnsureInitialized();
            _registry.Register(provider);
        }

        public void TryRegisterEditorCommands()
        {
            EnsureInitialized();
            var editorExtensionType = Type.GetType("Xeon.UniTerminal_X.Editor.TerminalCommandRegistryEditorExtensions, UniTerminalX.Editor");
            var method = editorExtensionType?.GetMethod("RegisterEditorCommands", BindingFlags.Public | BindingFlags.Static);
            method?.Invoke(null, new object[] { _registry });
        }

        public Task<TerminalCommandResult> ExecuteAsync(string input)
        {
            EnsureInitialized();
            return _session.ExecuteAsync(input);
        }
    }
}
