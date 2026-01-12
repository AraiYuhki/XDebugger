using System;
using System.IO;
using System.Linq;
using System.Text;

namespace XDebugger.Terminal
{
    public sealed class TerminalFileCommands
    {
        [TerminalCommand("pwd", Description = "現在のディレクトリを表示します")]
        private TerminalCommandResult PrintWorkingDirectory(TerminalCommandContext context, string[] args)
        {
            return TerminalCommandResult.Ok(context.GetCurrentDirectory());
        }

        [TerminalCommand("cd", Description = "ディレクトリを移動します")]
        private TerminalCommandResult ChangeDirectory(TerminalCommandContext context, string[] args)
        {
            if (args.Length == 0)
            {
                return TerminalCommandResult.Fail("cd: パスを指定してください");
            }

            var current = context.GetCurrentDirectory();
            var target = ResolvePath(current, args[0]);
            if (!Directory.Exists(target))
            {
                return TerminalCommandResult.Fail($"cd: ディレクトリが存在しません: {args[0]}");
            }

            context.SetCurrentDirectory(target);
            return TerminalCommandResult.Ok(target);
        }

        [TerminalCommand("ls", Description = "ディレクトリの内容を一覧表示します")]
        private TerminalCommandResult ListDirectory(TerminalCommandContext context, string[] args)
        {
            var current = context.GetCurrentDirectory();
            var target = args.Length > 0 ? ResolvePath(current, args[0]) : current;
            if (!Directory.Exists(target))
            {
                return TerminalCommandResult.Fail($"ls: ディレクトリが存在しません: {target}");
            }

            var entries = Directory.GetFileSystemEntries(target)
                .Select(Path.GetFileName)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase);

            var output = string.Join("\n", entries);
            return TerminalCommandResult.Ok(output);
        }

        [TerminalCommand("cat", Description = "ファイルを表示します")]
        private TerminalCommandResult CatFile(TerminalCommandContext context, string[] args)
        {
            if (args.Length == 0)
            {
                return TerminalCommandResult.Fail("cat: ファイルを指定してください");
            }

            var path = ResolvePath(context.GetCurrentDirectory(), args[0]);
            if (!File.Exists(path))
            {
                return TerminalCommandResult.Fail($"cat: ファイルが存在しません: {path}");
            }

            return TerminalCommandResult.Ok(File.ReadAllText(path));
        }

        [TerminalCommand("less", Description = "ファイルの先頭部分を表示します")]
        private TerminalCommandResult LessFile(TerminalCommandContext context, string[] args)
        {
            if (args.Length == 0)
            {
                return TerminalCommandResult.Fail("less: ファイルを指定してください");
            }

            var path = ResolvePath(context.GetCurrentDirectory(), args[0]);
            if (!File.Exists(path))
            {
                return TerminalCommandResult.Fail($"less: ファイルが存在しません: {path}");
            }

            var lineLimit = 20;
            if (args.Length > 1 && int.TryParse(args[1], out var parsedLimit))
            {
                lineLimit = Math.Max(1, parsedLimit);
            }

            var lines = File.ReadLines(path).Take(lineLimit);
            return TerminalCommandResult.Ok(string.Join("\n", lines));
        }

        [TerminalCommand("grep", Description = "ファイル内を検索します")]
        private TerminalCommandResult GrepFile(TerminalCommandContext context, string[] args)
        {
            if (args.Length < 2)
            {
                return TerminalCommandResult.Fail("grep: パターンとファイルを指定してください");
            }

            var ignoreCase = args[0] == "-i";
            var patternIndex = ignoreCase ? 1 : 0;
            if (args.Length <= patternIndex + 0)
            {
                return TerminalCommandResult.Fail("grep: パターンを指定してください");
            }

            var pattern = args[patternIndex];
            if (args.Length <= patternIndex + 1)
            {
                return TerminalCommandResult.Fail("grep: ファイルを指定してください");
            }

            var path = ResolvePath(context.GetCurrentDirectory(), args[patternIndex + 1]);
            if (!File.Exists(path))
            {
                return TerminalCommandResult.Fail($"grep: ファイルが存在しません: {path}");
            }

            var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var builder = new StringBuilder();
            foreach (var line in File.ReadLines(path))
            {
                if (line.IndexOf(pattern, comparison) >= 0)
                {
                    if (builder.Length > 0)
                    {
                        builder.Append('\n');
                    }

                    builder.Append(line);
                }
            }

            return TerminalCommandResult.Ok(builder.ToString());
        }

        private static string ResolvePath(string current, string input)
        {
            return Path.GetFullPath(Path.Combine(current, input));
        }
    }
}
