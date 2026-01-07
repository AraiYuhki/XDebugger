using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Model
{
    public class MockNavigationPageModel : PageModel
    {
        public MockNavigationPageModel() : base("Mock Navigation Page")
        {
        }

        public override void Initialize(IUIFactory uiFactory)
        {
            base.Initialize(uiFactory);
            AddLabel("This is a mock page for navigation checks.");
            AddButton("Back to Default Page", () => XDebugger.Instance.ClosePage(this));
        }
    }
}
