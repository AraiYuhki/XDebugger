using System;
using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Model
{
    public class FoldingGroupModel : GroupModel
    {
        private bool isFolding = false;
        private FoldingGroup control;

        private Action<bool> onChangedFolding; 
        
        public bool IsFolding
        {
            get => isFolding;
            set
            {
                isFolding = value;
                control?.SetFolding(value);
                onChangedFolding?.Invoke(value);
            }
        }
        
        public event Action<bool> OnChangedFolding
        {
            add
            {
                onChangedFolding -= value;
                onChangedFolding += value;
                value?.Invoke(isFolding);
            }

            remove => onChangedFolding -= value;
        }

        protected override string prefabAddress => "XDebugger/FoldGroup";

        public FoldingGroupModel(string title, bool isFolding, int priority = 0) : base(title, priority)
        {
            this.isFolding = isFolding;
        }

        public override ControlBase CreateControl(Transform parent, IUIFactory uiFactory)
        {
            control = uiFactory.CreateFoldingGroup(parent);
            control.Setup(this, parent.GetComponent<ContentGroup>(), isFolding);
            
            ResetChildren(control, uiFactory);

            return control;
        }

        public void SetFoldingWithoutNotify(bool isFolding)
        {
            this.isFolding = isFolding;
            onChangedFolding?.Invoke(isFolding);
        }
    }
}
