namespace Xeon.XDebugger.Console
{
    public interface IConsolePage
    {
        // Public title of the page (read-only)
        string Title { get; }

        // Expose the internal controller so users can customize behaviour if needed
        ConsoleController Controller { get; }

        // Initialize the page with a log buffer
        void Initialize(ILogDataBuffer logDataList);
    }
}
