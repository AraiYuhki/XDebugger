using NUnit.Framework;
using XDebugger.Terminal;

namespace XDebugger.Terminal.Tests
{
    public sealed class TerminalCommandParserTests
    {
        [Test]
        public void Parse_SplitsTokensWithQuotes()
        {
            var parsed = TerminalCommandParser.Parse("echo \"hello world\" test");

            Assert.IsTrue(parsed.HasValue);
            Assert.AreEqual("echo", parsed.Name);
            Assert.AreEqual(2, parsed.Args.Length);
            Assert.AreEqual("hello world", parsed.Args[0]);
            Assert.AreEqual("test", parsed.Args[1]);
        }
    }
}
