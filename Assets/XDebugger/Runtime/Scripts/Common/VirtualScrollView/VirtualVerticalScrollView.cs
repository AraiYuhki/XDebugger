using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Xeon.XDebugger.Console;

namespace Xeon.Common
{
    public class VirtualVerticalScrollView : MonoBehaviour
    {
        public enum Alignment
        {
            Left,
            Center,
            Right,
        }

        [SerializeField]
        private LogItem prefab;
        [SerializeField]
        private ScrollRect scrollView;
        [SerializeField]
        private RectTransform viewPort;
        [SerializeField]
        private RectTransform content;
        [SerializeField]
        private RectOffset padding = new();
        [SerializeField]
        private float spacing = 0f;
        [SerializeField]
        private Alignment alignment = Alignment.Left;
        [SerializeField]
        private bool isControlChildSize = false;

        private VirtualScrollViewControllerBase controller;
        private Vector2 prevScrollPosition = Vector2.zero;

        public float Spacing
        {
            get => spacing;
            set
            {
                spacing = value;
                controller.SetSpacing(spacing);
            }
        }

        public void Setup(VirtualScrollViewControllerBase controller)
        {
            scrollView.onValueChanged.RemoveListener(OnChangedScrollPosition);
            scrollView.onValueChanged.AddListener(OnChangedScrollPosition);
            prevScrollPosition = scrollView.normalizedPosition;
            this.controller = controller;
            controller.Setup(viewPort, content, padding, spacing, alignment, isControlChildSize);
        }

        private void OnChangedScrollPosition(Vector2 position)
        {
            if (controller == null)
                return;

            var isNext = position.y - prevScrollPosition.y < 0;
            prevScrollPosition = position;

            controller.Update(isNext, position.y);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            controller?.SetSpacing(spacing);
            controller?.SetHorizontalAlignment(alignment);
        }
#endif
    }
}
