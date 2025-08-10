using UnityEngine;

namespace Xeon.XDebugger.Model
{
    public class HorizontalGroupModel : GroupModel
    {
        protected override string prefabAddress => "XDebugger/HorizontalGroup";

        public HorizontalGroupModel(string title, int priority = 0): base(title, priority) { }
        public HorizontalGroupModel(string title, IGroupModel parent, int priority = 0) : base(title, parent, priority) { }
    }
}