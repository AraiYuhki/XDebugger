using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.Profiler;

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
            AddLabel("===== Debug Menu =====");

            AddPageLinkButton<SystemPageModel>("System Info");
            AddPageLinkButton<ProfilerPageModel>("Profiler");
            AddPageLinkButton<ConsolePageModel>("Console");

            AddButton("Start", () => Debug.Log("開始が選択されました"));
            AddButton("Exit", () => Debug.Log("終了が選択されました"));

            AddToggle("Enabled", false, v => Debug.Log($"有効化: {v}"), 1);

            AddIntSlider("Volume", 50, 0, 100, v => Debug.Log($"ボリューム: {v}"), 2);

            AddSlider("Brightness", 0.5f, 0f, 1f, 2, v => Debug.Log($"明るさ: {v:F2}"), 3);

            AddText("Username", "Player", v => Debug.Log($"ユーザー名: {v}"), 4);

            AddDropdown(
                "Mode Select",
                0,
                new[] { "Easy", "Normal", "Hard" },
                new[] { 0, 1, 2 },
                v => Debug.Log($"モード選択: {v}"),
                5
            );

            AddEnumDropdown("Color Select", TestColor.Red, v => Debug.Log($"色選択: {v}"), 6);

            AddPageLinkButton<TestPageModel>("Go to Detail Page", 10);

            using (HorizontalScope("Horizontal Group"))
            {
                AddLabel("Horizontal Label");
                AddButton("Horizontal Button", () => Debug.Log("水平ボタン押下"));
            }

            using (VerticalScope("Vertical Group"))
            {
                AddLabel("Vertical Label");
                AddToggle("Vertical Toggle", true, v => Debug.Log($"垂直トグル: {v}"), 7);
            }

            AddButton("Refresh", () => Refresh(true), 99);
        }

        public virtual void Update()
        {
        }

        public void SetGroup(IGroupModel model)
        {
            group = model;
        }

        public void OpenPage(Transform parent, PageModel pageModel)
        {
            Clear();
            Initialize();
            content = parent;
            control ??= Instantiate(parent);
            foreach (var model in modelList)
                controlList.Add(model.CreateControl(control.Content));
            control.Open(pageModel);
            OpenedPage();
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
        public override void Initialize()
        {
            AddLabel("Detail Page");
            AddButton("Back", () => XDebugger.Instance.ClosePage(this));
        }
    }
}
