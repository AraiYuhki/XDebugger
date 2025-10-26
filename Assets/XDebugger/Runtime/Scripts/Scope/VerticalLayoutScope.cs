namespace Xeon.XDebugger.Model
{
    public class VerticalLayoutScope : GroupLayoutScope
    {
        public VerticalLayoutScope(string title, PageModel parent, int priority = 0)
            : base(parent, new VerticalGroupModel(title, priority))
        {
        }
    }
}
