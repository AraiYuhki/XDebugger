using UnityEngine;

namespace Xeon.XDebugger.Model.Console
{
    public class ConsolePageModel : PageModel
    {
        public override void Initialize()
        {
            using (HorizontalScope())
            {
                AddToggle("Info", true);
                AddToggle("Warning", true);
                AddToggle("Error", true);
            }
        }
    }
}
