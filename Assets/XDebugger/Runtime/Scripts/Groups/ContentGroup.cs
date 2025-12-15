using System.Collections.Generic;
using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.Model;

namespace Xeon.XDebugger
{

    public class ContentGroup : ControlBase
    {
        [SerializeField]
        protected Transform content;

        protected IGroupModel model;
        protected ContentGroup parent;
        protected List<ControlBase> children;

        public Transform GetContent() => content;

        public void Setup(IGroupModel model, ContentGroup parent)
        {
            Setup(model.Title);
            title.gameObject.SetActive(!string.IsNullOrEmpty(model.Title));
            this.model = model;
            this.parent = parent;
        }

        public override void Refresh()
        {
            if (children == null) return;

            foreach (var control in children)
                control.Refresh();
        }
    }
}
