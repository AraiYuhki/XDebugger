using UnityEngine;
using UnityEngine.UI;
using Xeon.XDebugger.Model;

namespace Xeon.XDebugger.Control
{
    public class ActionControl : ControlBase
    {
        [SerializeField]
        protected Button button;

        protected ActionModel model;

        public void Setup(ActionModel model)
        {
            Setup(model.Title);
            
            this.model = model;

            button.onClick.RemoveListener(model.ExecuteMethod);
            button.onClick.AddListener(model.ExecuteMethod);
        }
    }
}
