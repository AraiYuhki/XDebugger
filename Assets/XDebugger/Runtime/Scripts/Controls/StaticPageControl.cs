using System;
using UnityEngine;
using Xeon.XDebugger.Common;

namespace Xeon.XDebugger.Control
{
    public class StaticPageControl : MonoBehaviour
    {
        private static readonly int OpenId = Animator.StringToHash("Open");
        private static readonly int CloseId = Animator.StringToHash("Close");

        public string Title => title;
        public Sprite TabIcon => tabIcon;

        [SerializeField]
        private string title = string.Empty;
        [SerializeField]
        private Sprite tabIcon;
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

        public virtual void Setup(TabController tabController)
        {
        }
    }
}
