using System.Collections.Generic;
using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Model
{
    public abstract class GroupModel : ControlModelBase, IGroupModel
    {
        protected List<ControlModelBase> children = new();
        protected List<ControlBase> childrenControlls = new();

        public GroupModel(string title, int priority = 0) : base(title, priority)
        {
        }

        public GroupModel(string title, IGroupModel parent, int priority = 0)
            : base(title, parent, priority)
        {
        }

        public void AddChild(ControlModelBase model)
        {
            if (model.Parent != this)
                model.SetParent(this);
            children.Add(model);
        }

        public IReadOnlyCollection<ControlModelBase> Children => children;

        public override ControlBase CreateControl(Transform parent, IUIFactory uiFactory)
        {
            var control = Instantiate<ContentGroup>(parent);
            control.Setup(this, parent.GetComponent<ContentGroup>());

            childrenControlls.Clear();

            foreach (var child in children)
            {
                childrenControlls.Add(child.CreateControl(control.GetContent(), uiFactory));
            }

            control.SetInteractable(IsInteractable);

            return control;
        }
    }
}