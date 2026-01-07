using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Model
{
    /// <summary>
    /// ScriptableObject版のページモデル。IPageModelインターフェイスを実装します。
    /// </summary>
    public class ScriptablePageModel : ScriptableObject, IPageModel
    {
        [SerializeField]
        private string pageTitle = "Scriptable Page";

        private IUIFactory uiFactory;
        private Transform content;
        private IGroupModel group;
        private List<ControlModelBase> modelList = new List<ControlModelBase>();
        private List<ControlBase> controlList = new();
        private MonoBehaviour control;

        public string Title => pageTitle;

        public Transform Content
        {
            get => content;
            set => content = value;
        }

        public IGroupModel CurrentGroup => group;

        public ReadOnlyCollection<ControlModelBase> ModelList => new ReadOnlyCollection<ControlModelBase>(modelList.OrderBy(model => model.Priority).ToList());

        public ControlModelBase this[int index] => modelList[index];
        public int Count => modelList.Count;

        /// <summary>
        /// ページを初期化
        /// </summary>
        public virtual void Initialize(IUIFactory uiFactory)
        {
            this.uiFactory = uiFactory;
        }

        /// <summary>
        /// ページを更新
        /// </summary>
        public virtual void Update()
        {
        }

        /// <summary>
        /// グループモデルを設定
        /// </summary>
        public void SetGroup(IGroupModel model)
        {
            group = model;
        }

        /// <summary>
        /// 制御コンポーネントを取得
        /// </summary>
        public MonoBehaviour GetControl() => control;

        /// <summary>
        /// ページを開く
        /// </summary>
        public void OpenPage(Transform parent, IUIFactory uiFactory)
        {
            Clear();
            Initialize(uiFactory);
            content = parent;
            CreateControl(parent, uiFactory);
            OpenedPage();
        }

        /// <summary>
        /// 制御コンポーネントを作成
        /// </summary>
        private void CreateControl(Transform parent, IUIFactory uiFactory)
        {
            if (control == null)
                control = uiFactory.CreatePage<PageControl>(parent);
            if (control != null && control is PageControl pageControl)
            {
                foreach (var model in modelList)
                    controlList.Add(model.CreateControl(pageControl.Content, uiFactory));
                // ScriptablePageModel は PageModel ではないため、null を渡す
                // または abstract class を使用する必要がある場合は要検討
                pageControl.Open(this);
            }
        }

        /// <summary>
        /// ページが開かれた時のコールバック
        /// </summary>
        protected virtual void OpenedPage()
        {
        }

        /// <summary>
        /// ページを閉じる
        /// </summary>
        public virtual void Close(Action onClose = null)
        {
            if (control is PageControl pageControl)
            {
                pageControl.Close(() =>
                {
                    ClosedPage();
                    Clear();
                    onClose?.Invoke();
                    GameObject.Destroy(pageControl.gameObject);
                    content = null;
                    control = null;
                });
            }
            else if (control is StaticPageControl staticPageControl)
            {
                staticPageControl.Close(() =>
                {
                    ClosedPage();
                    staticPageControl.gameObject.SetActive(false);
                    onClose?.Invoke();
                    content = null;
                    control = null;
                });
            }
            else
            {
                ClosedPage();
                onClose?.Invoke();
            }
        }

        /// <summary>
        /// ページが閉じられた時のコールバック
        /// </summary>
        protected virtual void ClosedPage()
        {
        }

        /// <summary>
        /// ページを表示
        /// </summary>
        public void Show(bool isRefresh = false)
        {
            if (isRefresh)
                Refresh();
            if (control != null)
            {
                control.gameObject.SetActive(true);
                OpenPage();
            }
        }

        /// <summary>
        /// ページを非表示
        /// </summary>
        public void Hide(Action onHidden = null)
        {
            if (control != null)
            {
                if (control is PageControl pageControl)
                {
                    pageControl.Close(() =>
                    {
                        control.gameObject.SetActive(false);
                        onHidden?.Invoke();
                    });
                }
                else if (control is StaticPageControl staticPageControl)
                {
                    staticPageControl.Close(() =>
                    {
                        control.gameObject.SetActive(false);
                        onHidden?.Invoke();
                    });
                }
            }
            else
            {
                onHidden?.Invoke();
            }
        }

        /// <summary>
        /// ページをリフレッシュ
        /// </summary>
        public virtual void Refresh()
        {
            foreach (var control in controlList)
                control.Refresh();
        }

        public virtual void RefreshCurrentPage(Transform parent)
        {
            Clear();
            Initialize(uiFactory);
            CreateControl(parent, uiFactory);
            OpenPage();
            Refresh();
        }

        protected void OpenPage()
        {
            if (control is PageControl pageControl)
            {
                pageControl.Open();
            }
            else if (control is StaticPageControl staticPageControl)
            {
                staticPageControl.Open();
            }
        }

        /// <summary>
        /// 内容をクリア
        /// </summary>
        protected virtual void Clear()
        {
            foreach (var control in controlList)
             {
                if (control != null)
                    GameObject.Destroy(control.gameObject);
            }
            controlList.Clear();
            modelList.Clear();
        }

        #region Add Control Methods

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

        #endregion

        /// <summary>
        /// 子モデルを追加
        /// </summary>
        private void AddChild(ControlModelBase model)
        {
            if (group is null)
                modelList.Add(model);
            else
                group.AddChild(model);
        }
    }
}
