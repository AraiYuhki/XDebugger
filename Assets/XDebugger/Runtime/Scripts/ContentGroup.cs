using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.Model;
using TMPro;
using System.Collections.Generic;

namespace Xeon.XDebugger
{

    public class ContentGroup : ControlBase
    {
        [SerializeField]
        private Transform content;

        private IGroupModel model;
        private ContentGroup parent;
        private List<ControlBase> children;

        public Transform GetContent() => content;

        public TMP_Text TitleLabel => title;

        public void SetChildren(List<ControlBase> controls)
            => children = controls;

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
