using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Xeon.Style;

namespace Xeon.XDebugger.Control
{
    public abstract class ControlBase : MonoBehaviour
    {
        [SerializeField]
        protected Image background;
        [SerializeField]
        protected TMP_Text title;
        [SerializeField]
        protected LayoutElement layoutElement;

        private Selectable[] selectableCache;
        private TMP_InputField[] inputFieldCache;
        private CanvasGroup canvasGroup;
        private bool cacheInitialized;

        public virtual void Setup(string title)
            => this.title.text = title;

        public virtual void Refresh() { }

        private void EnsureCache()
        {
            if (cacheInitialized)
                return;

            selectableCache = GetComponentsInChildren<Selectable>(true);
            inputFieldCache = GetComponentsInChildren<TMP_InputField>(true);
            canvasGroup = GetComponent<CanvasGroup>();
            cacheInitialized = true;
        }

        public virtual void SetInteractable(bool interactable)
        {
            EnsureCache();

            if (canvasGroup != null)
            {
                canvasGroup.interactable = interactable;
                canvasGroup.blocksRaycasts = interactable;
                canvasGroup.alpha = interactable ? 1f : 0.5f;
            }

            if (selectableCache != null)
            {
                foreach (var selectable in selectableCache)
                    selectable.interactable = interactable;
            }

            if (inputFieldCache != null)
            {
                foreach (var inputField in inputFieldCache)
                    inputField.interactable = interactable;
            }
        }

        public void ApplyStyle(params IStyle[] styles)
        {
            foreach (var style in styles)
                style.Apply(layoutElement, background);
        }
    }
}
