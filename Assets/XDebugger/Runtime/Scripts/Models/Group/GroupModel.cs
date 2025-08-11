using System.Collections.Generic;
using UnityEngine;
using Xeon.XDebugger.Control;

namespace Xeon.XDebugger.Model
{
    public abstract class GroupModel : ControlModelBase, IGroupModel
    {
        protected List<ControlModelBase> children = new();

        public GroupModel(string title, int priority = 0) : base(title, priority)
        {
        }

        public GroupModel(string title, IGroupModel parent, int priority = 0)
            : base(title, parent, priority)
        {
        }

        public void AddChild(ControlModelBase model) => children.Add(model);

        public IReadOnlyCollection<ControlModelBase> Children => children;

        public override ControlBase CreateControl(Transform parent)
        {
            var control = Instantiate<ContentGroup>(parent);
            control.Setup(this, parent.GetComponent<ContentGroup>());
            return control;
        }
    }
}