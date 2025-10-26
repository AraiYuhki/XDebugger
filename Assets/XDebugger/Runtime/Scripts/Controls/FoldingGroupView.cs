using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Xeon.XDebugger.Model;

namespace Xeon.XDebugger.Control
{
    [DisallowMultipleComponent]
    public class FoldingGroupView : MonoBehaviour
    {
        [SerializeField]
        private ContentGroup contentGroup;

        private FoldingGroupModel model;
        private Button titleButton;
        private TMP_Text titleLabel;
        private string baseTitle = string.Empty;

        public void Initialize(ContentGroup group, FoldingGroupModel foldingModel)
        {
            contentGroup = group;
            model = foldingModel;
            titleLabel = contentGroup.TitleLabel;
            if (titleLabel != null)
                titleLabel.gameObject.SetActive(true);
            baseTitle = titleLabel != null ? titleLabel.text : string.Empty;

            SetupTitleButton();
            model.RegisterView(this);
            ApplyState(model.IsExpanded);
        }

        private void SetupTitleButton()
        {
            if (titleLabel == null)
                return;

            titleButton = titleLabel.GetComponent<Button>();
            if (titleButton == null)
            {
                titleButton = titleLabel.gameObject.AddComponent<Button>();
                titleButton.transition = Selectable.Transition.None;
                titleButton.targetGraphic = titleLabel;
            }

            titleButton.onClick.RemoveListener(OnTitleClicked);
            titleButton.onClick.AddListener(OnTitleClicked);
        }

        private void OnDestroy()
        {
            if (titleButton != null)
                titleButton.onClick.RemoveListener(OnTitleClicked);
        }

        private void OnTitleClicked()
        {
            model.ToggleExpanded();
        }

        public void ApplyState(bool expanded)
        {
            if (contentGroup == null)
                return;

            var content = contentGroup.GetContent();
            if (content != null)
                content.gameObject.SetActive(expanded);

            if (titleLabel != null)
            {
                var prefix = expanded ? "▼" : "▶";
                titleLabel.text = string.IsNullOrEmpty(baseTitle) ? prefix : $"{prefix} {baseTitle}";
            }
        }

        public void UpdateTitle(string title)
        {
            baseTitle = title;
            ApplyState(model.IsExpanded);
        }
    }
}
