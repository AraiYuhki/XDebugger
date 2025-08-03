using System;
using UnityEngine;
using UnityEngine.UI;
using Xeon.XDebugger.Model;

namespace Xeon.XDebugger.Control
{
    public class BoolControl : ControlBase
    {
        [SerializeField]
        private Toggle toggle;

        protected BoolModel model;
        protected Action<bool> onValueChanged;


        public void Setup(BoolModel model, Action<bool> onValueChanged)
        {
            Setup(model.Title);
            this.model = model;
            this.onValueChanged = onValueChanged;

            toggle.onValueChanged.RemoveListener(OnValueChanged);
            toggle.onValueChanged.AddListener(OnValueChanged);

            Refresh();
        }

        public override void Refresh()
        {
            toggle.SetIsOnWithoutNotify(model.IsOn);
        }

        private void OnValueChanged(bool flag)
            => onValueChanged?.Invoke(flag);
    }
}
