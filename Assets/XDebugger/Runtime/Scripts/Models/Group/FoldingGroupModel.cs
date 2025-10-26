using System;
using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Model
{
    public class FoldingGroupModel : GroupModel
    {
        protected override string prefabAddress => "XDebugger/VerticalGroup";

        private bool isExpanded;
        private readonly Action<bool> onChangedExpanded;
        private FoldingGroupView view;

        public bool IsExpanded => isExpanded;

        public FoldingGroupModel(string title, bool expanded, Action<bool> onChangedExpanded, int priority = 0)
            : base(title, priority)
        {
            isExpanded = expanded;
            this.onChangedExpanded = onChangedExpanded;
        }

        public FoldingGroupModel(string title, bool expanded, Action<bool> onChangedExpanded, IGroupModel parent, int priority = 0)
            : base(title, parent, priority)
        {
            isExpanded = expanded;
            this.onChangedExpanded = onChangedExpanded;
        }

        public override ControlBase CreateControl(Transform parent, IUIFactory uiFactory)
        {
            var control = Instantiate<ContentGroup>(parent);
            control.Setup(this, parent.GetComponent<ContentGroup>());

            view = control.GetComponent<FoldingGroupView>() ?? control.gameObject.AddComponent<FoldingGroupView>();
            view.Initialize(control, this);

            childrenControlls.Clear();

            foreach (var child in children)
            {
                childrenControlls.Add(child.CreateControl(control.GetContent(), uiFactory));
            }

            view.ApplyState(isExpanded);
            control.SetInteractable(IsInteractable);

            return control;
        }

        public void RegisterView(FoldingGroupView foldingView)
        {
            view = foldingView;
            view.UpdateTitle(Title);
            view.ApplyState(isExpanded);
        }

        public void ToggleExpanded()
        {
            SetExpanded(!isExpanded, true);
        }

        private void SetExpanded(bool expanded, bool notify)
        {
            if (isExpanded == expanded)
                return;

            isExpanded = expanded;
            view?.ApplyState(isExpanded);

            if (notify)
                onChangedExpanded?.Invoke(isExpanded);
        }

        public void SetExpanded(bool expanded)
            => SetExpanded(expanded, false);
    }
}
