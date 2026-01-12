using System.Threading.Tasks;
using NUnit.Framework;
using Xeon.UniTerminal_X;

namespace Xeon.UniTerminal_X.Tests
{
    public sealed class TerminalCommandRegistryTests
    {
        private sealed class SampleCommands
        {
            [TerminalCommand("ping")]
            private TerminalCommandResult Ping(TerminalCommandContext context, string[] args)
            {
                return TerminalCommandResult.Ok("pong");
            }

            [TerminalCommand("async")]
            private Task<TerminalCommandResult> Async(TerminalCommandContext context, string[] args)
            {
                return Task.FromResult(TerminalCommandResult.Ok("done"));
            }
        }

        [Test]
        public async Task ExecuteAsync_RunsRegisteredCommand()
        {
            var registry = new TerminalCommandRegistry();
            registry.Register(new SampleCommands());
            var context = new TerminalCommandContext(() => "", _ => { });

            var result = await registry.ExecuteAsync("ping", context, new string[0]);

            Assert.IsTrue(result.Success);
            Assert.AreEqual("pong", result.Output);
        }

        [Test]
        public async Task ExecuteAsync_RunsAsyncCommand()
        {
            var registry = new TerminalCommandRegistry();
            registry.Register(new SampleCommands());
            var context = new TerminalCommandContext(() => "", _ => { });

            var result = await registry.ExecuteAsync("async", context, new string[0]);

            Assert.IsTrue(result.Success);
            Assert.AreEqual("done", result.Output);
        }
    }
}
