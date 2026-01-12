using System.Collections.Generic;
using System.Text;

namespace XDebugger.Terminal
{
    public static class TerminalCommandParser
    {
        public static TerminalParsedCommand Parse(string input)
        {
            var tokens = Tokenize(input);
            if (tokens.Count == 0)
            {
                return TerminalParsedCommand.Empty;
            }

            var name = tokens[0];
            var args = new string[tokens.Count - 1];
            for (var i = 1; i < tokens.Count; i++)
            {
                args[i - 1] = tokens[i];
            }

            return new TerminalParsedCommand(name, args);
        }

        private static List<string> Tokenize(string input)
        {
            var tokens = new List<string>();
            if (string.IsNullOrWhiteSpace(input))
            {
                return tokens;
            }

            var builder = new StringBuilder();
            var inQuotes = false;
            for (var i = 0; i < input.Length; i++)
            {
                var c = input[i];
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }

                if (!inQuotes && char.IsWhiteSpace(c))
                {
                    if (builder.Length > 0)
                    {
                        tokens.Add(builder.ToString());
                        builder.Clear();
                    }

                    continue;
                }

                builder.Append(c);
            }

            if (builder.Length > 0)
            {
                tokens.Add(builder.ToString());
            }

            return tokens;
        }
    }

    public readonly struct TerminalParsedCommand
    {
        public static readonly TerminalParsedCommand Empty = new TerminalParsedCommand(string.Empty, new string[0]);

        public TerminalParsedCommand(string name, string[] args)
        {
            Name = name;
            Args = args;
        }

        public string Name { get; }
        public string[] Args { get; }

        public bool HasValue => !string.IsNullOrEmpty(Name);
    }
}
