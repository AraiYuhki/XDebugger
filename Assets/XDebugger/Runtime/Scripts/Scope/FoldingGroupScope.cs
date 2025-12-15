using UnityEngine;

namespace Xeon.XDebugger.Model
{
    public class FoldingGroupScope : GroupLayoutScope
    {
        public FoldingGroupScope(string title, PageModel parent, int priority = 0)
            : base(title, parent, priority)
        {
        }

        protected override ControlModelBase CreateModel(string title, int priority = 0)
            => new FoldingGroupModel(title, false, priority);
    }
}
