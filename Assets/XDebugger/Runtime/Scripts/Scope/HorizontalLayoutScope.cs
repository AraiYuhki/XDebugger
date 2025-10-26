namespace Xeon.XDebugger.Model
{
    public class HorizontalLayoutScope : GroupLayoutScope
    {
        public HorizontalLayoutScope(string title, PageModel parent, int priority = 0)
            : base(parent, new HorizontalGroupModel(title, priority))
        {
        }
    }
}
