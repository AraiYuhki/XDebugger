namespace Xeon.XDebugger.Model
{
    public class VerticalLayoutScope : GroupLayoutScope
    {
        public VerticalLayoutScope(string title, PageModel parent, int priority = 0)
            : base(title, parent, priority)
        {
        }

        protected override ControlModelBase CreateModel(string title, int priority = 0)
            => new VerticalGroupModel(title, priority);
    }
}