using UnityEngine;

namespace Xeon.XDebugger.Profiler
{
    public interface IProfilerPage
    {
        // Public title of the page (read-only)
        string Title { get; }

        // Indicates whether Mono memory profiling is available on this platform
        bool IsMonoSupported { get; }

        // Request cleanup of unused memory (runs coroutine internally)
        void TriggerCleanupMemory();

        // Force a GC collection and refresh displayed mono memory info
        void GCCollect();
    }
}
