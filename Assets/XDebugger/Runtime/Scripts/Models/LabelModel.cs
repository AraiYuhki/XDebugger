using UnityEngine;
using Xeon.XDebugger.Control;

namespace Xeon.XDebugger.Model
{
    public class LabelModel : ControlModelBase
    {
        protected override string prefabAddress => $"XDebugger/{nameof(LabelControl)}";

        private LabelControl control;

        public LabelModel(string title, int priority = 0) : base(title, priority)
        {
        }

        public override ControlBase CreateControl(Transform parent)
        {
            control = Instantiate<LabelControl>(parent);
            control.Setup(Title);

            return control;
        }

        public void SetTitle(string title)
        {
            control?.Setup(title);
        }
    }
}