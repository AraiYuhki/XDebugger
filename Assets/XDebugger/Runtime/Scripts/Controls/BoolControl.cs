using System;
using UnityEngine;
using UnityEngine.UI;

namespace Xeon
{
    public class BoolControl : ControlBase
    {
        [SerializeField]
        private Toggle toggle;

        protected Func<bool> getter;
        protected Action<bool> setter;

        public void Setup(string title, bool isOn, Func<bool> getter, Action<bool> setter)
        {
            Setup(title);

            this.getter = getter;
            this.setter = setter;

            toggle.SetIsOnWithoutNotify(isOn);
            toggle.onValueChanged.RemoveListener(OnValueChanged);
            toggle.onValueChanged.AddListener(OnValueChanged);
        }

        public override void Refresh()
        {
            if (getter == null) return;
            toggle.SetIsOnWithoutNotify(getter());
        }

        private void OnValueChanged(bool flag)
            => setter?.Invoke(flag);
    }
}
