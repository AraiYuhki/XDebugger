using System;

namespace Xeon.XDebugger.Model
{
    public abstract class GroupLayoutScope : IDisposable
    {
        private readonly PageModel parent;
        private readonly IGroupModel previousGroup;

        protected GroupLayoutScope(PageModel parent, ControlModelBase model)
        {
            this.parent = parent;
            Model = model;
            previousGroup = parent.CurrentGroup;
        }

        public ControlModelBase Model { get; }

        public virtual void Dispose() => parent.SetGroup(previousGroup);
    }
}
