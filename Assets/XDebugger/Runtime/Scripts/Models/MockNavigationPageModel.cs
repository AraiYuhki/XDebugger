namespace Xeon.XDebugger.Model
{
    public class MockNavigationPageModel : PageModel
    {
        public MockNavigationPageModel() : base("Mock Navigation Page")
        {
        }

        protected override void InitializeInternal()
        {
            AddLabel("This is a mock page for navigation checks.");
            AddButton("Back to Default Page", () => XDebugger.Instance.ClosePage(this));
        }
    }
}
