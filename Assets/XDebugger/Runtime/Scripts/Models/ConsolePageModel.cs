using Xeon.XDebugger.Console;

namespace Xeon.XDebugger.Model
{
    public class ConsolePageModel : PageModel
    {
        protected override string prefabAddress => $"XDebugger/{nameof(ConsolePage)}";
    }
}
