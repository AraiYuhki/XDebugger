namespace Xeon.XDebugger.Model
{
    public class DisableGroupScope : GroupLayoutScope
    {
        public DisableGroupScope(string title, PageModel parent, int priority = 0)
            : base(title, parent, priority)
        {
        }

        protected override ControlModelBase CreateModel(string title, int priority = 0)
            => new DisableGroupModel(title, priority);
    }
}
