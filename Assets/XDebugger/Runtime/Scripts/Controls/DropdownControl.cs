using TMPro;
using UnityEngine;
using Xeon.XDebugger.Model;

namespace Xeon.XDebugger.Control
{

    public class DropdownControl : ControlBase
    {
        [SerializeField]
        protected TMP_Dropdown dropdown;

        protected IDropdownModel model;

        public void Setup(IDropdownModel model)
        {
            Setup(model.Title);
            if (this.model != null)
                this.model.Changed -= OnModelChanged;

            this.model = model;
            this.model.Changed += OnModelChanged;

            dropdown.onValueChanged.RemoveListener(OnChangedValue);
            dropdown.onValueChanged.AddListener(OnChangedValue);
            Refresh();
        }

        public override void Refresh()
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(model.Labels);
            dropdown.SetValueWithoutNotify(model.SelectedIndex);
        }

        protected virtual void OnChangedValue(int index) => model.NotifySelectedIndexChangedFromView(index);

        private void OnModelChanged() => Refresh();

        private void OnDestroy()
        {
            if (dropdown != null)
                dropdown.onValueChanged.RemoveListener(OnChangedValue);

            if (model != null)
                model.Changed -= OnModelChanged;
        }
    }
}
