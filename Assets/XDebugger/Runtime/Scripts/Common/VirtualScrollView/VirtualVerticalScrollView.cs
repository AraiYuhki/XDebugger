using System.Collections.ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Xeon.Common.Debug;
using Xeon.XDebugger.Console;

namespace Xeon.Common
{
    [ExecuteInEditMode]
    public class VirtualVerticalScrollView : MonoBehaviour
    {
        public enum Alignment
        {
            Left,
            Center,
            Right,
        }

        [SerializeField]
        private ScrollRect scrollView;
        [SerializeField]
        private VirtualScrollViewport viewPort;
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
            controller.Setup(viewPort.RectTransform, content, padding, spacing, alignment, isControlChildSize);
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

        [SerializeField]
        private DebugVirtualTestItem debugPrefab;

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
