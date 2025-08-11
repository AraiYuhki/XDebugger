namespace Xeon.XDebugger.Model
{
    public class HorizontalLayoutScope : GroupLayoutScope
    {
        public HorizontalLayoutScope(string title, PageModel parent, int priority = 0)
            : base(title, parent, priority)
        {
        }

        protected override ControlModelBase CreateModel(string title, int priority = 0)
            => new HorizontalGroupModel(title, priority);
    }
}
