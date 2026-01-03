using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.UI;

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

        public int SelectedIndex
        {
            get => selectedIndex;
            set => SetSelectedIndex(value, false);
        }

        public List<string> Labels => labels;

        public EnumDropdownModel(string title, int selectedIndex , Action<T> onChangedValue, int priority = 0) : base(title, priority)
        {
            Initialize(selectedIndex, onChangedValue);
        }

        public EnumDropdownModel(string title, int selectedIndex, Action<T> onChangedValue, IGroupModel parent, int priority): base(title, parent, priority)
        {
            Initialize(selectedIndex, onChangedValue);
        }

        public EnumDropdownModel(string title, T value, Action<T> onChangedValue, int priority = 0) : base(title, priority)
        {
            Initialize(IndexOf(value), onChangedValue);
        }
        public EnumDropdownModel(string title, T value, Action<T> onChangedValue, IGroupModel parent, int priority = 0) : base(title, parent, priority)
        {
            Initialize(IndexOf(value), onChangedValue);
        }

        private void Initialize(int selectedIndex, Action<T> onChangedValue)
        {
            labels = Enum.GetNames(typeof(T)).ToList();
            options = Enum.GetValues(typeof(T)).Cast<T>().ToArray();
            this.selectedIndex = selectedIndex;
            this.onChangedValue = onChangedValue;
        }

        private int IndexOf(T target)
        {
            foreach (var (value, index) in Enum.GetValues(typeof(T)).Cast<T>().Select((value, index) => (value, index)))
            {
                if (target.Equals(value))
                    return index;
            }
            return -1;
        }


        public override ControlBase CreateControl(Transform parent, IUIFactory uiFactory)
        {
            var control = uiFactory.CreateControl<DropdownControl>(parent);
            control.Setup(this);
            return control;
        }

        public void NotifySelectedIndexChangedFromView(int index) => SetSelectedIndex(index, true);

        private void SetSelectedIndex(int index, bool notifyCallback)
        {
            selectedIndex = Mathf.Clamp(index, 0, GetMaxIndex());
            NotifyChanged();
            if (notifyCallback && options.Length > 0)
                onChangedValue?.Invoke(options[selectedIndex]);
        }

        private int GetMaxIndex() => Mathf.Max(0, options.Length - 1);
    }
}
