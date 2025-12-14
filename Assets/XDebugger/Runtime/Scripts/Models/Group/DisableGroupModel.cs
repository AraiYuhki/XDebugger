using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Model
{
    public class DisableGroupModel : GroupModel
    {
        private bool isDisabled = false;
        private DisableGroup control;

        public bool IsDisabled 
        {
            get => isDisabled;
            set
            {
                isDisabled = value;
                control?.SetDisable(isDisabled);
            }
        }

        protected override string prefabAddress => "XDebugger/DisableGroup";

        public DisableGroupModel(string title, int priority = 0) : base(title, priority)
        {
        }

        public override ControlBase CreateControl(Transform parent, IUIFactory uiFactory)
        {
            control = uiFactory.CreateDisableGroup(parent);
            control.Setup(this, parent.GetComponent<ContentGroup>());

            ResetChildren(control, uiFactory);

            return control;
        }

    }
}
