using System.Collections.Generic;
using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.Model;

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

        public void Setup(IGroupModel model, ContentGroup parent)
        {
            Setup(model.Title);
            this.model = model;
            this.parent = parent;
        }
    }
}
