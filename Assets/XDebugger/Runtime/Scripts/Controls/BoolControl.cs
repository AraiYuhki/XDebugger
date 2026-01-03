using UnityEngine;
using UnityEngine.UI;
using Xeon.XDebugger.Model;

namespace Xeon.XDebugger.Control
{
    public class BoolControl : ControlBase
    {
        [SerializeField]
        private Toggle toggle;

        protected BoolModel model;

        public void Setup(BoolModel model)
        {
            Setup(model.Title);
            if (this.model != null)
                this.model.Changed -= OnModelChanged;

            this.model = model;
            this.model.Changed += OnModelChanged;

            toggle.onValueChanged.RemoveListener(OnValueChanged);
            toggle.onValueChanged.AddListener(OnValueChanged);

            Refresh();
        }

        public override void Refresh()
        {
            toggle.SetIsOnWithoutNotify(model.IsOn);
        }

        private void OnValueChanged(bool flag)
            => model.NotifyValueChangedFromView(flag);

        private void OnModelChanged() => Refresh();

        private void OnDestroy()
        {
            if (toggle != null)
                toggle.onValueChanged.RemoveListener(OnValueChanged);

            if (model != null)
                model.Changed -= OnModelChanged;
        }
    }
}
