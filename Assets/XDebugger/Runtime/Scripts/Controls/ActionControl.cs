using System;
using UnityEngine;
using UnityEngine.UI;

namespace Xeon
{
    public class ActionControl : ControlBase
    {
        [SerializeField]
        protected Button button;

        protected Action action;

        public ActionControl(string title, Action action)
        {
            Setup(title);
            this.action = action;
            button.onClick.RemoveListener(OnClicked);
            button.onClick.AddListener(OnClicked);

        }

        protected virtual void OnClicked()
        {
            action?.Invoke();
        }
    }
}
