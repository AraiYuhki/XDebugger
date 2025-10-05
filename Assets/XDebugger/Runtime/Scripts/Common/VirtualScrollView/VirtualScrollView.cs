using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;
using Xeon.Common.Debug;

namespace Xeon.Common
{
    public enum HorizontalAlignment
    {
        Left,
        Center,
        Right,
    }

    public enum VerticalAlignment
    {
        Top,
        Middle,
        Bottom,
    }

    public abstract class VirtualScrollView : MonoBehaviour
    {
        [SerializeField]
        protected ScrollRect scrollView;
        [SerializeField]
        protected VirtualScrollViewport viewPort;
        [SerializeField]
        protected RectTransform content;
        [SerializeField]
        protected RectOffset padding = new();
        [SerializeField]
        protected float spacing = 0f;
        [SerializeField]
        protected bool isControlChildSize = false;
        [SerializeField]
        protected bool isReverse = false;

        protected VirtualScrollViewControllerBase controller;
        protected Vector2 prevScrollPosition = Vector2.zero;

        public float Spacing
        {
            get => spacing;
            set
            {
                spacing = value;
                controller?.SetSpacing(spacing);
            }
        }

        public virtual void Setup(VirtualScrollViewControllerBase controller)
        {
            scrollView.onValueChanged.RemoveListener(OnChangedScrollPosition);
            scrollView.onValueChanged.AddListener(OnChangedScrollPosition);
            prevScrollPosition = scrollView.normalizedPosition;
            this.controller = controller;
        }

        private void OnDestroy()
        {
            controller?.Dispose();
            controller = null;
        }

        protected abstract void SetReverseMode();
        protected abstract void OnChangedScrollPosition(Vector2 position);

#if UNITY_EDITOR
        [SerializeField]
        protected DebugVirtualTestItem debugPrefab;
        [SerializeField, HideInInspector]
        protected bool prevIsReverse = false;

        protected virtual void OnValidate()
        {
            controller?.SetSpacing(spacing);
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
            controller?.Dispose();
            controller = null;
        }

        [UnityEditor.CustomEditor(typeof(VirtualScrollView), true)]
        private class VirtualScrollViewEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                if (GUILayout.Button("Debug Simulate"))
                    (target as VirtualScrollView).DebugCreate();
                if (GUILayout.Button("Clear"))
                    (target as VirtualScrollView).Clear();
            }
        }
#endif

    }
}
