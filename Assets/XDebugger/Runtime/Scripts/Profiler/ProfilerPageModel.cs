using UnityEngine;
using Xeon.XDebugger.Profiler;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Model
{
    public class ProfilerPageModel : PageModel
    {
        public ProfilerPageModel() : base("Profiler")
        {
        }
        
        public override void Initialize(IUIFactory uiFactory)
        {
            this.uiFactory = uiFactory;
        }

        protected override void CreateControl(Transform parent, IUIFactory uiFactory)
        {
            control ??= uiFactory.CreatePage<ProfilerPage>(parent);
        }
    }
}
