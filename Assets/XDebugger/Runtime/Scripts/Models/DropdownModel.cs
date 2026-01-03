using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Model
{
    public class DropdownModel<T> : ControlModelBase, IDropdownModel
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

        public T SelectedItem
        {
            get => options[selectedIndex];
        }

        public List<string> Labels => labels;

        public void SetOptions(IEnumerable<string> labels, IEnumerable<T> options, bool isRefreshControl = true)
        {
            this.labels = labels.ToList();
            this.options = options.ToArray();
            if (isRefreshControl)
                NotifyChanged();
        }

        public void SetOptions(IEnumerable<T> options, bool isRefreshControl = true)
        {
            this.options = options.ToArray();
            this.labels = options.Select(option => option.ToString()).ToList();
            if (isRefreshControl)
                NotifyChanged();
        }
        

        public DropdownModel(string title, int value, IEnumerable<string> labels, IEnumerable<T> options, Action<T> onChangedValue, int priority = 0) : base(title, priority)
        {
            Initialize(value, labels, options, onChangedValue);
        }

        public DropdownModel(string title, int value, IEnumerable<string> labels, IEnumerable<T> options, Action<T> onChangedValue, IGroupModel parent, int priority = 0)
            : base(title, priority)
        {
            Initialize(value, labels, options, onChangedValue);
        }

        public DropdownModel(string title, int value, IEnumerable<T> options, Action<T> onChangedValue, int priority = 0) : base(title, priority)
        {
            Initialize(value, options.Select(option => option.ToString()), options, onChangedValue);
        }

        public DropdownModel(string title, int value, IEnumerable<T> options, Action<T> onChangedValue, IGroupModel parent, int priority = 0)
            :base(title, parent, priority)
        {
            Initialize(value, options.Select(option => option.ToString()), options, onChangedValue);
        }

        private void Initialize(int value, IEnumerable<string> labels, IEnumerable<T> options, Action<T> onChangedValue)
        {
            selectedIndex = value;
            this.labels = labels.ToList();
            this.options = options.ToArray();
            this.onChangedValue = onChangedValue;
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
