using TMPro;
using UnityEngine;
using Xeon.XDebugger.Model;

namespace Xeon.XDebugger.Control
{
    public class StringControl : ControlBase
    {
        [SerializeField]
        protected TMP_InputField inputField;

        protected StringModel model;

        public void Setup(StringModel model)
        {
            Setup(model.Title);
            if (this.model != null)
                this.model.Changed -= OnModelChanged;

            this.model = model;
            this.model.Changed += OnModelChanged;

            inputField.onEndEdit.RemoveListener(OnEndEdit);
            inputField.onEndEdit.AddListener(OnEndEdit);

            Refresh();
        }

        public override void Refresh()
        {
            inputField.SetTextWithoutNotify(model.Text);
        }

        private void OnEndEdit(string text) => model.NotifyTextChangedFromView(text);

        private void OnModelChanged() => Refresh();

        private void OnDestroy()
        {
            if (inputField != null)
                inputField.onEndEdit.RemoveListener(OnEndEdit);

            if (model != null)
                model.Changed -= OnModelChanged;
        }
    }
}
