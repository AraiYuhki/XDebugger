using System;

namespace Xeon.XDebugger.Model
{
    public sealed class FoldingLayoutScope : GroupLayoutScope
    {
        private readonly Action<bool> disposeCallback;

        public new FoldingGroupModel Model => (FoldingGroupModel)base.Model;

        public FoldingLayoutScope(string title, PageModel parent, bool initialState, Action<bool> onChanged, int priority = 0)
            : base(parent, new FoldingGroupModel(title, initialState, onChanged, priority))
        {
        }

        public FoldingLayoutScope(string title, PageModel parent, bool initialState, Action<bool> onChanged, Action<bool> onDispose, int priority = 0)
            : this(title, parent, initialState, onChanged, priority)
        {
            disposeCallback = onDispose;
        }

        public bool IsExpanded => Model.IsExpanded;

        public override void Dispose()
        {
            base.Dispose();
            disposeCallback?.Invoke(Model.IsExpanded);
        }
    }
}
