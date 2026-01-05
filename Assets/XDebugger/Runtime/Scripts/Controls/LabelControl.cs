using Xeon.XDebugger.Model;

namespace Xeon.XDebugger.Control
{
    public class LabelControl : ControlBase
    {
        protected LabelModel model;

        public void Setup(LabelModel model)
        {
            if (this.model != null)
                this.model.Changed -= OnModelChanged;

            this.model = model;
            this.model.Changed += OnModelChanged;
            Refresh();
        }

        public override void Refresh()
        {
            Setup(model.Title);
        }

        private void OnModelChanged() => Refresh();

        private void OnDestroy()
        {
            if (model != null)
                model.Changed -= OnModelChanged;
        }
    }
}
