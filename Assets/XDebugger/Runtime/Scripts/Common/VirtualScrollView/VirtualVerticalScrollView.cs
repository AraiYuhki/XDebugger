using System.Collections.ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Xeon.Common.Debug;
using Xeon.XDebugger.Console;

namespace Xeon.Common
{
    public enum HorizontalAlignment
    {
        Left,
        Center,
        Right,
    }

    [ExecuteInEditMode]
    public class VirtualVerticalScrollView : MonoBehaviour
    {
        [SerializeField]
        private ScrollRect scrollView;
        [SerializeField]
        private VirtualScrollViewport viewPort;
        [SerializeField]
        private RectTransform content;
        [SerializeField]
        private Scrollbar scrollbar;
        [SerializeField]
        private RectOffset padding = new();
        [SerializeField]
        private float spacing = 0f;
        [SerializeField]
        private HorizontalAlignment alignment = HorizontalAlignment.Left;
        [SerializeField]
        private bool isControlChildSize = false;
        [SerializeField]
        private bool isReverse = false;

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
            controller.Setup(viewPort.RectTransform, content, padding, spacing, alignment, isControlChildSize, isReverse);
        }

        private void OnChangedScrollPosition(Vector2 position)
        {
            if (controller == null)
                return;

            var isNext = position.y - prevScrollPosition.y < 0;
            prevScrollPosition = position;

            controller.Update(isNext, position.y);
        }

        private void SetReverseMode()
        {
            if (isReverse)
            {
                content.anchorMin = Vector2.zero;
                content.anchorMax = Vector2.right;
                content.pivot = new Vector2(0.5f, 0f);
            }
            else
            {
                content.anchorMin = Vector2.up;
                content.anchorMax = Vector2.one;
                content.pivot = new Vector2(0.5f, 1f);
            }
            controller?.SetIsReverse(isReverse);
        }

#if UNITY_EDITOR
        [SerializeField]
        private DebugVirtualTestItem debugPrefab;
        [SerializeField, HideInInspector]
        private bool prevIsReverse = false;

        private void OnValidate()
        {
            controller?.SetSpacing(spacing);
            controller?.SetHorizontalAlignment(alignment);
            if (prevIsReverse != isReverse)
            {
                SetReverseMode();
                prevIsReverse = isReverse;
            }
        }


        private void DebugCreate()
        {
            var dataList = new ObservableCollection<int>();
            for (var count = 0; count < 100; count++)
                dataList.Add(count);
            var controller = new VirtualScrollViewController<int, DebugVirtualTestItem>(debugPrefab, dataList);
            Setup(controller);
            controller.UpdateContainerSize();
            controller.UpdateView();
        }

        private void Clear()
        {
            controller.Dispose();
            controller = null;
        }

        [UnityEditor.CustomEditor(typeof(VirtualVerticalScrollView))]
        private class VirtualVerticalScrollViewEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                if (GUILayout.Button("Debug Simulate"))
                    (target as VirtualVerticalScrollView).DebugCreate();
                if (GUILayout.Button("Clear"))
                    (target as VirtualVerticalScrollView).Clear();
            }
        }
#endif
    }
}
