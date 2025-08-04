using System.Collections.Generic;

namespace Xeon.XDebugger.Model
{
    public interface IDropdownModel
    {
        int SelectedIndex { get; set; }
        List<string> Labels { get; }
        string Title { get; }
    }
}
