using System;
using System.Collections.Generic;

namespace Xeon.XDebugger.Model
{
    public interface IDropdownModel
    {
        event Action Changed;
        int SelectedIndex { get; set; }
        List<string> Labels { get; }
        string Title { get; }
        void NotifySelectedIndexChangedFromView(int index);
    }
}
