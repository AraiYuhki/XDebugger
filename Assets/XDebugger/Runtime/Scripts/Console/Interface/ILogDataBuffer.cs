using Xeon.Common.FlyweightScrollView;

namespace Xeon.XDebugger.Console
{
    public interface ILogDataBuffer : IObservableCollection<LogItemData>
    {
        bool VisibleInfo { get; set; }
        bool VisibleWarn { get; set; }
        bool VisibleError { get; set; }

        int InfoCount { get; }
        int WarnCount { get; }
        int ErrorCount { get; }
    }
}