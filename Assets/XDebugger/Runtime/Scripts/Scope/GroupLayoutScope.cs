using System;

namespace Xeon.XDebugger.Model
{
    public abstract class GroupLayoutScope : IDisposable
    {
        protected PageModel parent;
        protected IGroupModel prevModel;

        protected ControlModelBase model;

        public ControlModelBase Model => model;

        public GroupLayoutScope(string title, PageModel parent, int priority = 0)
        {
            this.parent = parent;
            model = CreateModel(title, priority);
            prevModel = parent.CurrentGroup;
        }

        public void Dispose() => parent.SetGroup(prevModel);

        protected abstract ControlModelBase CreateModel(string title, int priority = 0);
    }
}
