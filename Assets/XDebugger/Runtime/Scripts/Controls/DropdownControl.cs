using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Xeon
{

    public class DropdownControl : ControlBase
    {
        [SerializeField]
        protected TMP_Dropdown dropdown;

        protected int selectedIndex = 0;

        protected Func<int> getter;
        protected Action<int> setter;

        public void Setup(string title, int value, string[] labels, Func<int> getter, Action<int> setter)
        {
            Setup(title);
            this.getter = getter;
            this.setter = setter;

            selectedIndex = value;

            dropdown.ClearOptions();
            var options = new List<TMP_Dropdown.OptionData>();
            foreach (var label in labels)
                options.Add(new TMP_Dropdown.OptionData(label));
            dropdown.AddOptions(options);

            dropdown.SetValueWithoutNotify(selectedIndex);
            dropdown.onValueChanged.RemoveListener(OnValueChanged);
            dropdown.onValueChanged.AddListener(OnValueChanged);
        }

        public override void Refresh()
        {
            if (getter == null) return;

            selectedIndex = getter();
            dropdown.SetValueWithoutNotify(selectedIndex);
        }

        protected virtual void OnValueChanged(int index)
        {
            selectedIndex = index;
            setter?.Invoke(selectedIndex);
        }
    }
}
