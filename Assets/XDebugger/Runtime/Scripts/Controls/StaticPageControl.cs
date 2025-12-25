using System;
using UnityEngine;

namespace Xeon.XDebugger.Control
{
    public class StaticPageControl : MonoBehaviour
    {
        private static readonly int OpenId = Animator.StringToHash("Open");
        private static readonly int CloseId = Animator.StringToHash("Close");

        public virtual string Title => string.Empty;

        [SerializeField]
        private CanvasGroup canvasGroup;
        [SerializeField]
        private Animator animator;

        private Action onOpened;
        private Action onClosed;

        public float Alpha
        {
            get => canvasGroup.alpha;
            set => canvasGroup.alpha = value;
        }

        public virtual void Open(Action onOpened = null)
        {
            this.onOpened = onOpened;
            if (animator != null)
                animator.Play(OpenId);
        }

        public virtual void Close(Action onClosed = null)
        {
            this.onClosed = onClosed;
            if (animator != null)
                animator.Play(CloseId);
        }

        public virtual void SetActive(bool isActive) => gameObject.SetActive(isActive);

        public void OnOpened()
        {
            onOpened?.Invoke();
        }

        public void OnClosed()
        {
            onClosed?.Invoke();
        }
    }
}
