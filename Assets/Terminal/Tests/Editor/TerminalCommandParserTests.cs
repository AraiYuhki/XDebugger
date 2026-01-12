using NUnit.Framework;
using Xeon.UniTerminal_X;

namespace Xeon.UniTerminal_X.Tests
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
