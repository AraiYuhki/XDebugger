using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace XDebugger.Terminal
{
    public sealed class TerminalCommandRegistry
    {
        private readonly Dictionary<string, TerminalCommandHandler> _handlers = new Dictionary<string, TerminalCommandHandler>(StringComparer.OrdinalIgnoreCase);

        public IEnumerable<TerminalCommandHandler> Handlers => _handlers.Values;

        public void Register(object target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            var type = target.GetType();
            foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var attribute = method.GetCustomAttribute<TerminalCommandAttribute>();
                if (attribute == null)
                {
                    continue;
                }

                var handler = TerminalCommandHandler.Create(attribute.Name, attribute.Description, target, method);
                _handlers[handler.Name] = handler;
            }
        }

        public bool TryGet(string name, out TerminalCommandHandler handler)
        {
            return _handlers.TryGetValue(name, out handler);
        }

        public Task<TerminalCommandResult> ExecuteAsync(string name, TerminalCommandContext context, string[] args)
        {
            if (!TryGet(name, out var handler))
            {
                return Task.FromResult(TerminalCommandResult.Fail($"Unknown command: {name}"));
            }

            return handler.ExecuteAsync(context, args);
        }
    }

    public sealed class TerminalCommandHandler
    {
        private readonly Func<TerminalCommandContext, string[], Task<TerminalCommandResult>> _executor;

        private TerminalCommandHandler(string name, string description, Func<TerminalCommandContext, string[], Task<TerminalCommandResult>> executor)
        {
            Name = name;
            Description = description;
            _executor = executor;
        }

        public string Name { get; }
        public string Description { get; }

        public Task<TerminalCommandResult> ExecuteAsync(TerminalCommandContext context, string[] args)
        {
            return _executor(context, args);
        }

        public static TerminalCommandHandler Create(string name, string description, object target, MethodInfo method)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Command name is required.", nameof(name));
            }

            var parameters = method.GetParameters();
            if (parameters.Length != 2 || parameters[0].ParameterType != typeof(TerminalCommandContext) || parameters[1].ParameterType != typeof(string[]))
            {
                throw new InvalidOperationException($"{method.DeclaringType?.Name}.{method.Name} has an invalid signature for terminal commands.");
            }

            if (method.ReturnType == typeof(TerminalCommandResult))
            {
                return new TerminalCommandHandler(name, description, (context, args) =>
                {
                    var result = (TerminalCommandResult)method.Invoke(target, new object[] { context, args });
                    return Task.FromResult(result);
                });
            }

            if (typeof(Task<TerminalCommandResult>).IsAssignableFrom(method.ReturnType))
            {
                return new TerminalCommandHandler(name, description, (context, args) =>
                {
                    return (Task<TerminalCommandResult>)method.Invoke(target, new object[] { context, args });
                });
            }

            throw new InvalidOperationException($"{method.DeclaringType?.Name}.{method.Name} must return TerminalCommandResult or Task<TerminalCommandResult>.");
        }
    }
}
