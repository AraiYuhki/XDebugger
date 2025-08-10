namespace Xeon.XDebugger.Model
{
    public class VerticalGroupModel : GroupModel
    {
        public VerticalGroupModel(string title, int priority = 0) : base(title, priority)
        {
        }

        public VerticalGroupModel(string title, IGroupModel parent, int priority = 0)
            : base(title, parent, priority)
        {
        }

        protected override string prefabAddress => "XDebugger/VerticalGroup";
    }
}