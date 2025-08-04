using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

namespace Xeon.XDebugger.Model
{
    public class PageModel
    {
        protected Transform content;
        protected Transform rootContent;
        protected List<ControlModelBase> modelList = new List<ControlModelBase>();
        protected string title;

        public string Title => title;

        public ReadOnlyCollection<ControlModelBase> ModelList => new ReadOnlyCollection<ControlModelBase>(modelList.OrderBy(model => model.Priority).ToList());

        public ControlModelBase this[int index] => modelList[index];
        public int Count => modelList.Count;

        public PageModel() { }
        public PageModel(string title) => this.title = title;

        public PageModel(string title, PageModel other)
        {
            this.title = title;
            var copyItemModels = new ControlModelBase[other.Count];
            other.modelList.CopyTo(copyItemModels);
            modelList = copyItemModels.ToList();
        }

        protected virtual void Initialize()
        {

        }

        public void OpenPage(Transform parent)
        {
        }

        public void AddLabel(LabelModel model) => modelList.Add(model);
        public void AddLabel(string text, int priority = 0) => AddLabel(new LabelModel(text, priority));

        public void AddButton(ActionModel model) => modelList.Add(model);
        public void AddButton(string text, Action action, int priority = 0) => AddButton(new ActionModel(text, action, priority));
        
        public void AddNumber(NumberModel model) => modelList.Add(model);
        public void AddNumber(string text, float value, float step, Action<float> onChangedValue, int priority = 0) => AddNumber(new NumberModel(text, value, step, onChangedValue, priority));

        public void AddSlider(FloatSliderModel model) => modelList.Add(model);
        public void AddSlider(string text, float value, float min, float max, Action<float> onChangedValue, int digits = 2, int priority = 0) => AddSlider(new FloatSliderModel(text, value, min, max, onChangedValue, digits, priority));

        public void AddIntSlider(IntSliderModel model) => modelList.Add(model);
        public void AddIntSlider(string text, int value, int min, int max, Action<int> onChangedValue, int priority = 0) => AddIntSlider(new IntSliderModel(text, value, min, max, onChangedValue, priority));

        public void AddDropdown<T>(DropdownModel<T> model) => modelList.Add(model);
        public void AddDropdown<T>(string text, int value, IEnumerable<string> labels, IEnumerable<T> options, Action<T> onChangedValue, int priority = 0)
            => AddDropdown(new DropdownModel<T>(text, value, labels, options, onChangedValue, priority));

        public void AddEnumDropdown<T>(EnumDropdownModel<T> model) where T : Enum
            => modelList.Add(model);

        public void AddEnumDropdown<T>(string text, T value, Action<T> onChangedValue, int priority = 0) where T : Enum
            => AddEnumDropdown(new EnumDropdownModel<T>(text, value, onChangedValue, priority));

    }
}
