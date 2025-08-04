
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEngine;
using Xeon.XDebugger.Control;

namespace Xeon.XDebugger.Model
{
    public class EnumDropdownModel<T> : ControlModelBase, IDropdownModel
        where T : Enum
    {
        protected override string prefabAddress => $"XDebugger/{nameof(DropdownControl)}";

        private int selectedIndex = 0;
        private List<string> labels;
        private T[] options;

        private Action<T> onChangedValue;

        private DropdownControl control;

        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                selectedIndex = Mathf.Clamp(value, 0, options.Length);
                control?.Refresh();
            }
        }

        public List<string> Labels => labels;

        public EnumDropdownModel(string title, int selectedIndex , Action<T> onChangedValue, int priority = 0) : base(title, priority)
        {
            labels = Enum.GetNames(typeof(T)).ToList();
            options = Enum.GetValues(typeof(T)).Cast<T>().ToArray();
            this.selectedIndex = selectedIndex;
            this.onChangedValue = onChangedValue;
        }

        public EnumDropdownModel(string title, T value, Action<T> onChangedValue, int priority = 0) : base(title, priority)
        {
            labels = Enum.GetNames(typeof(T)).ToList();
            options = Enum.GetValues(typeof(T)).Cast<T>().ToArray();

            var index = 0;
            for (var i = 0; i < options.Length; i++)
            {
                var option = options[i];
                if (option.Equals(value))
                {
                    index = i;
                    break;
                }
            }
            selectedIndex = index;
            this.onChangedValue = onChangedValue;
        }


        public override ControlBase CreateControl(Transform parent)
        {
            control = Instantiate<DropdownControl>(parent);
            control.Setup(this, OnChangedValue);
            return control;
        }

        private void OnChangedValue(int index)
        {
            selectedIndex = index;
            onChangedValue?.Invoke(options[selectedIndex]);
        }
    }
}