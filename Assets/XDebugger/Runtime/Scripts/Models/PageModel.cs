using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Model
{
    public class PageModel
    {
        protected IUIFactory uiFactory;

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

        public virtual void Initialize(IUIFactory uiFactory)
        {
            this.uiFactory = uiFactory;
        }

        public virtual void Update()
        {
        }

        public void SetGroup(IGroupModel model)
        {
            group = model;
        }

        public void OpenPage(Transform parent, PageModel pageModel, IUIFactory uiFactory)
        {
            Clear();
            Initialize(uiFactory);
            content = parent;
            CreateControl(parent, uiFactory);
            foreach (var model in modelList)
                controlList.Add(model.CreateControl(control.Content, uiFactory));
            control.Open(pageModel);
            OpenedPage();
        }

        protected virtual void CreateControl(Transform parent, IUIFactory uiFactory)
        {
            if (control == null)
                control = uiFactory.CreatePage<PageControl>(parent);
        }

        protected virtual void OpenedPage()
        {
        }

        public virtual void Close(Action onClose = null)
        {
            control.Close(() =>
            {
                ClosedPage();
                Clear();
                XDebugger.Instance.ClosePage(this);
                onClose?.Invoke();
                GameObject.Destroy(control.gameObject);
                content = null;
                control = null;
            });
        }

        protected virtual void ClosedPage()
        {
        }

        public void Show(bool isRefresh = false)
        {
            if (isRefresh)
                Refresh();
            control.gameObject.SetActive(true);
            control.Open();
        }

        public void Hide(Action onHidden = null)
        {
            control.Close(() =>
            {
                control.gameObject.SetActive(false);
                onHidden?.Invoke();
            });   
        }

        public virtual void Refresh(bool doRecreate = false)
        {

            if (doRecreate)
            {
                Clear();
                Initialize(uiFactory);
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

        public GroupLayoutScope HorizontalScope(string title = "", int priority = 0)
        {
            var scope = new HorizontalLayoutScope(title, this, priority);
            AddChild(scope.Model);
            SetGroup(scope.Model as IGroupModel);
            return scope;
        }

        public GroupLayoutScope VerticalScope(string title = "", int priority = 0)
        {
            var scope = new VerticalLayoutScope(title, this, priority);
            AddChild(scope.Model);
            SetGroup(scope.Model as IGroupModel);
            return scope;
        }

        public DisableGroupScope　DisableScope(out DisableGroupModel model, string title = "", bool isDisabled = false, int priority = 0)
        {
            var scope = new DisableGroupScope(title, this, priority);
            model = scope.Model as DisableGroupModel;
            AddChild(scope.Model);
            SetGroup(scope.Model as IGroupModel);
            return scope;
        }

        public FoldingGroupScope FoldingScope(out FoldingGroupModel model, string title = "", bool isFolding = false,
            int priority = 0)
        {
            var scope = new FoldingGroupScope(title, this, priority);
            model = scope.Model as FoldingGroupModel;
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
        public LabelModel AddLabel(string text, int priority = 0)
        {
            var model = new LabelModel(text, priority);
            AddLabel(model);
            return model;
        }

        public void AddButton(ActionModel model) => AddChild(model);
        public ActionModel AddButton(string text, Action action, int priority = 0)
        {
            var model = new ActionModel(text, action, priority);
            AddButton(model);
            return model;
        }


        public ActionModel AddPageLinkButton<T>(string text, int priority = 0) where T : PageModel, new()
        {
            var model = new ActionModel(text, () => XDebugger.Instance.OpenPage<T>(), priority);
            modelList.Add(model);
            return model;
        }

        public void AddText(StringModel model) => AddChild(model);
        public StringModel AddText(string title, string text, Action<string> onChangedValue = null, int priority = 0)
        {
            var model = new StringModel(title, text, onChangedValue, priority);
            AddText(model);
            return model;
        }
        
        public void AddNumber(NumberModel model) => AddChild(model);
        public NumberModel AddNumber(string title, float value, float step, Action<float> onChangedValue = null, int priority = 0)
        {
            var model = new NumberModel(title, value, step, onChangedValue, priority);
            AddNumber(model);
            return model;
        }

        public void AddSlider(FloatSliderModel model) => AddChild(model);
        public FloatSliderModel AddSlider(string title, float value, float min, float max, int digits = 2, Action<float> onChangedValue = null, int priority = 0)
        {
            var model = new FloatSliderModel(title, value, min, max, onChangedValue, digits, priority);
            AddSlider(model);
            return model;
        }

        public void AddIntSlider(IntSliderModel model) => AddChild(model);
        public IntSliderModel AddIntSlider(string title, int value, int min, int max, Action<int> onChangedValue = null, int priority = 0)
        {
            var model = new IntSliderModel(title, value, min, max, onChangedValue, priority);
            AddIntSlider(model);
            return model;
        }

        public void AddToggle(BoolModel model) => AddChild(model);
        public BoolModel AddToggle(string title, bool value, Action<bool> onChangedValue = null, int priority = 0)
        {
            var model = new BoolModel(title, value, onChangedValue, priority);
            AddToggle(model);
            return model;
        }

        public void AddDropdown<T>(DropdownModel<T> model) => AddChild(model);
        public DropdownModel<T> AddDropdown<T>(string text, int value, IEnumerable<string> labels, IEnumerable<T> options, Action<T> onChangedValue = null, int priority = 0)
        {
            var model = new DropdownModel<T>(text, value, labels, options, onChangedValue, priority);
            AddDropdown(model);
            return model;
        }

        public void AddEnumDropdown<T>(EnumDropdownModel<T> model) where T : Enum
            => AddChild(model);

        public EnumDropdownModel<T> AddEnumDropdown<T>(string text, T value, Action<T> onChangedValue = null, int priority = 0) where T : Enum
        {
            var model = new EnumDropdownModel<T>(text, value, onChangedValue, priority);
            AddEnumDropdown(model);
            return model;
        }

    }

    // テスト用Enum
    public enum TestColor
    {
        Red,
        Green,
        Blue
    }

    // テスト用ページ
    public class TestPageModel : PageModel
    {
        public override void Initialize(IUIFactory uiFactory)
        {
            this.uiFactory = uiFactory;
            AddLabel("Detail Page");
            AddButton("Back", () => XDebugger.Instance.ClosePage(this));
        }
    }
}
