using System;
using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Model
{
    public class ActionModel : ControlModelBase
    {
        protected override string prefabAddress => $"XDebugger/{nameof(ActionControl)}";

        private ActionControl control;
        private Action action;

        public ActionModel(string title, Action action, int priority = 0) : base(title, priority)
        {
            this.action = action;
        }

        public void ExecuteMethod() => action?.Invoke();

        public override ControlBase CreateControl(Transform parent, IUIFactory uiFactory)
        {
            control = uiFactory.CreateControl<ActionControl>(parent);
            control.Setup(this);
            control.SetInteractable(IsInteractable);
            return control;
        }
    }
}