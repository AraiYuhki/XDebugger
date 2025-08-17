using System;
using UnityEngine;

namespace Xeon.XDebugger.Control
{
    public class PageControl : MonoBehaviour
    {
        private static readonly int OpenId = Animator.StringToHash("Open");
        private static readonly int CloseId = Animator.StringToHash("Close");

        [SerializeField]
        private Transform content;
        [SerializeField]
        private CanvasGroup canvasGroup;
        [SerializeField]
        private Animator animator;

        private Action onOpened;
        private Action onClosed;

        public Transform Content => content;

        public void Open(Action onOpened = null)
        {
            this.onOpened = onOpened;
            animator.Play(OpenId);
        }

        public void Close(Action onClosed = null)
        {
            this.onClosed = onClosed;
            animator.Play(CloseId);
        }

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
