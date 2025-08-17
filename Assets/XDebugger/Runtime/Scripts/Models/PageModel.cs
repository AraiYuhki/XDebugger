using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Xeon.XDebugger.Control;

namespace Xeon.XDebugger.Model
{
    public class PageModel
    {
        protected virtual string prefabAddress => "XDebugger/Page";

        protected Transform content;

        protected IGroupModel group;

        protected List<ControlModelBase> modelList = new List<ControlModelBase>();
        protected List<ControlBase> controlList = new();
        protected string title;
        protected PageControl control;

        public string Title => title;

        public Transform Content
        {
            get => content;
            set => content = value;
        }

        public IGroupModel CurrentGroup => group;

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

        public virtual void Initialize()
        {
            AddLabel("test");
            AddNumber("number test", 0, 1, null);
            using (HorizontalScope("test horizontal scope"))
            {
                AddLabel("test label in horizontal scope");
                AddIntSlider("test int slider", 10, 0, 255, null);
                using (VerticalScope("test vertical scope"))
                {
                    AddLabel("test label in vertical scope recursive");
                    AddText("test string", "New Text", null);
                }
                using (VerticalScope("test vertical scope2"))
                {
                    AddLabel("test label in vertical scope2");
                    AddButton("Test button", null);
                }
                AddButton("Test button in horizontal scope", null);
            }
            AddButton("Footer button", null);
        }

        public void SetGroup(IGroupModel model)
        {
            group = model;
        }

        public void OpenPage(Transform parent)
        {
            Clear();
            Initialize();
            content = parent;
            control ??= Instantiate(parent);
            foreach (var model in modelList)
                controlList.Add(model.CreateControl(control.Content));
            control.Open();
        }

        public virtual void Close(Action onClose = null)
        {
            control.Close(() =>
            {
                Clear();
                XDebugger.Instance.ClosePage(this);
                onClose?.Invoke();
            });
        }

        public void Show(bool isRefresh = false)
        {
            if (isRefresh)
                Refresh();
            control.Open();
        }

        public void Hide() => control.Close();

        public virtual void Refresh(bool doRecreate = false)
        {

            if (doRecreate)
            {
                Clear();
                Initialize();
            }
            foreach (var control in controlList)
                control.Refresh();
        }

        protected virtual void Clear()
        {
            foreach (var control in controlList)
                GameObject.Destroy(control.gameObject);
            controlList.Clear();
            modelList.Clear();
        }

        public PageControl Instantiate(Transform parent)
        {
            var prefab = Addressables.LoadAssetAsync<GameObject>(prefabAddress).WaitForCompletion();
            var instance = GameObject.Instantiate(prefab, parent);
            return instance.GetComponent<PageControl>();
        }

        public GroupLayoutScope HorizontalScope(string title, int priority = 0)
        {
            var scope = new HorizontalLayoutScope(title, this, priority);
            AddChild(scope.Model);
            SetGroup(scope.Model as IGroupModel);
            return scope;
        }

        public GroupLayoutScope VerticalScope(string title, int priority = 0)
        {
            var scope = new VerticalLayoutScope(title, this, priority);
            AddChild(scope.Model);
            SetGroup(scope.Model as IGroupModel);
            return scope;
        }

        private void AddChild(ControlModelBase model)
        {
            if (group is null)
                modelList.Add(model);
            else
                group.AddChild(model);
        }

        public void AddLabel(LabelModel model) => AddChild(model);
        public void AddLabel(string text, int priority = 0) => AddLabel(new LabelModel(text, priority));

        public void AddButton(ActionModel model) => AddChild(model);
        public void AddButton(string text, Action action, int priority = 0) => AddButton(new ActionModel(text, action, priority));

        public void AddPageLinkButton<T>(string text, int priority = 0) where T : PageModel, new()
        {
            var model = new ActionModel(text, () => XDebugger.Instance.OpenPage<T>(), priority);
            modelList.Add(model);
        }

        public void AddText(StringModel model) => AddChild(model);
        public void AddText(string title, string text, Action<string> onChangedValue, int priority = 0) => AddText(new StringModel(title, text, onChangedValue, priority));
        
        public void AddNumber(NumberModel model) => AddChild(model);
        public void AddNumber(string title, float value, float step, Action<float> onChangedValue, int priority = 0) => AddNumber(new NumberModel(title, value, step, onChangedValue, priority));

        public void AddSlider(FloatSliderModel model) => AddChild(model);
        public void AddSlider(string title, float value, float min, float max, Action<float> onChangedValue, int digits = 2, int priority = 0) => AddSlider(new FloatSliderModel(title, value, min, max, onChangedValue, digits, priority));

        public void AddIntSlider(IntSliderModel model) => AddChild(model);
        public void AddIntSlider(string title, int value, int min, int max, Action<int> onChangedValue, int priority = 0) => AddIntSlider(new IntSliderModel(title, value, min, max, onChangedValue, priority));

        public void AddToggle(BoolModel model) => AddChild(model);
        public void AddToggle(string title, bool value, Action<bool> onChangedValue, int priority)
            => AddToggle(new BoolModel(title, value, onChangedValue, priority));

        public void AddDropdown<T>(DropdownModel<T> model) => AddChild(model);
        public void AddDropdown<T>(string text, int value, IEnumerable<string> labels, IEnumerable<T> options, Action<T> onChangedValue, int priority = 0)
            => AddDropdown(new DropdownModel<T>(text, value, labels, options, onChangedValue, priority));

        public void AddEnumDropdown<T>(EnumDropdownModel<T> model) where T : Enum
            => AddChild(model);

        public void AddEnumDropdown<T>(string text, T value, Action<T> onChangedValue, int priority = 0) where T : Enum
            => AddEnumDropdown(new EnumDropdownModel<T>(text, value, onChangedValue, priority));

    }
}
