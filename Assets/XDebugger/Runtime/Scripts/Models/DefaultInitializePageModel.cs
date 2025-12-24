using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Model
{
    public class DefaultInitializePageModel : PageModel
    {
        public DefaultInitializePageModel() : base("Initial Page")
        {
        }
        
        public override void Initialize(IUIFactory uiFactory)
        {
            base.Initialize(uiFactory);
            AddPageLinkButton<SystemPageModel>("System Info");
        }
    }
}
