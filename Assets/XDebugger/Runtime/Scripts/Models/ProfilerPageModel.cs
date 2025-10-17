using Xeon.XDebugger.Profiler;

namespace Xeon.XDebugger.Model
{
    public class ProfilerPageModel : PageModel
    {
        protected override string prefabAddress => $"XDebugger/{nameof(ProfilerPage)}";
    }
}
