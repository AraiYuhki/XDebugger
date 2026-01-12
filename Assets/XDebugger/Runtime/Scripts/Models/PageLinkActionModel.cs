using System;
using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Model
{
    public class PageLinkActionModel : ActionModel
    {
        public PageModel PageModel { get; private set; }
        public PageLinkActionModel(string title, PageModel pageModel, int priority = 0)
             : base(title, () => XDebugger.Instance.OpenPage(pageModel), priority)
        {
            PageModel = pageModel;
        }
    }
}