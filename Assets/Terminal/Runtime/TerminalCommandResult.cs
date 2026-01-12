namespace Xeon.UniTerminal_X
{
    public sealed class TerminalCommandResult
    {
        private TerminalCommandResult(bool success, string output, string error)
        {
            Success = success;
            Output = output;
            Error = error;
        }

        public bool Success { get; }
        public string Output { get; }
        public string Error { get; }

        public static TerminalCommandResult Ok(string output = "")
        {
            return new TerminalCommandResult(true, output, string.Empty);
        }

        public static TerminalCommandResult Fail(string error)
        {
            return new TerminalCommandResult(false, string.Empty, error);
        }
    }
}
