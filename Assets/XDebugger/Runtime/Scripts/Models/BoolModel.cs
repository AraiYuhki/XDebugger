using UnityEngine;
using Xeon.XDebugger.Control;

namespace Xeon.XDebugger.Model
{
    public class BoolModel : ControlModelBase
    {
        protected override string prefabAddress => $"XDebugger/{nameof(BoolControl)}";

        private BoolControl control;
        private bool isOn = false;

        public bool IsOn
        {
            get => isOn;
            set
            {
                isOn = value;
                control?.Refresh();
            }
        }

        public BoolModel(string title, bool isOn, int priority = 0) : base(title, priority)
        {
            this.isOn = isOn;
        }

        public override ControlBase CreateControl(Transform parent)
        {
            var control = Instantiate<BoolControl>(parent);
            return control;
        }
    }
}