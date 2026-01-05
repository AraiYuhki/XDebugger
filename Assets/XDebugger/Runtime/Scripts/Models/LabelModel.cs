using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Model
{
    public class LabelModel : ControlModelBase
    {
        protected override string prefabAddress => $"XDebugger/{nameof(LabelControl)}";

        public LabelModel(string title, int priority = 0) : base(title, priority)
        {
        }

        public override ControlBase CreateControl(Transform parent, IUIFactory uiFactory)
        {
            var control = uiFactory.CreateControl<LabelControl>(parent);
            control.Setup(this);
            return control;
        }

        public void SetText(string title)
        {
            Title = title;
            NotifyChanged();
        }
    }
}
