using System;

namespace Xeon.XDebugger.Model
{
    public sealed class FoldingLayoutScope : GroupLayoutScope
    {
        private static bool pendingInitialState;
        private static Action<bool> pendingCallback;

        public FoldingGroupModel FoldingModel => model as FoldingGroupModel;

        private FoldingLayoutScope(string title, PageModel parent, int priority)
            : base(title, parent, priority)
        {
        }

        public static FoldingLayoutScope Create(string title, PageModel parent, bool initialState, Action<bool> onChanged, int priority)
        {
            pendingInitialState = initialState;
            pendingCallback = onChanged;
            var scope = new FoldingLayoutScope(title, parent, priority);
            pendingInitialState = false;
            pendingCallback = null;
            return scope;
        }

        protected override ControlModelBase CreateModel(string title, int priority = 0)
            => new FoldingGroupModel(title, pendingInitialState, pendingCallback, priority);

        public bool IsExpanded => (model as FoldingGroupModel)?.IsExpanded ?? false;
    }
}
