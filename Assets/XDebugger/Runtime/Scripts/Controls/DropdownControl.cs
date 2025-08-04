using System;
using System.Linq;
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

        protected Action<int> onChangedValue;

        public void Setup(IDropdownModel model, Action<int> onChangedValue)
        {
            Setup(model.Title);
            this.model = model;
            this.onChangedValue = onChangedValue;

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

        protected virtual void OnChangedValue(int index) => onChangedValue?.Invoke(index);
    }
}
