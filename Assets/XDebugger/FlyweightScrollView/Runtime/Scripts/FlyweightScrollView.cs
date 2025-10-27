using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;
using Xeon.Common.Debug;

namespace Xeon.Common.FlyweightScrollView
{
    public abstract class FlyweightScrollView : MonoBehaviour
    {
        [SerializeField]
        protected ScrollRect scrollView;
        [SerializeField]
        protected FlyweightScrollViewport viewPort;
        [SerializeField]
        protected RectTransform content;
        [SerializeField]
        protected FlyweightScrollViewParam param = new();

        protected FlyweightScrollViewControllerBase controller;
        protected Vector2 prevScrollPosition = Vector2.zero;

        public float Spacing
        {
            get => param.Spacing;
            set
            {
                param.Spacing = value;
                controller?.SetSpacing(param.Spacing);
            }
        }

        public Vector2 normalizedPosition
        {
            get => scrollView.normalizedPosition;
            set => scrollView.normalizedPosition = value;
        }

        public virtual void Setup(FlyweightScrollViewControllerBase controller)
        {
            scrollView.onValueChanged.RemoveListener(OnChangedScrollPosition);
            scrollView.onValueChanged.AddListener(OnChangedScrollPosition);
            viewPort.OnRectTransformDimensionsChanged -= controller.UpdateViewportSize;
            viewPort.OnRectTransformDimensionsChanged += controller.UpdateViewportSize;
            prevScrollPosition = scrollView.normalizedPosition;
            this.controller = controller;
            if (param.IsAtLastSticky)
                controller.SetIsPositionLast(true);
        }

        private void OnDestroy()
        {
            controller?.Dispose();
            controller = null;
        }

        private void Update()
        {
            if (controller.IsDirty)
            {
                controller.UpdateView();
                controller.IsDirty = false;
            }
        }

        protected abstract void SetReverseMode();
        protected abstract void OnChangedScrollPosition(Vector2 position);

        protected static (bool isNext, bool isPositionLast) EvaluateHorizontalScroll(Vector2 current, Vector2 previous)
        {
            var isNext = current.x - previous.x < 0f;
            var isPositionLast = current.x <= float.Epsilon;
            return (isNext, isPositionLast);
        }

        protected static (bool isNext, bool isPositionLast) EvaluateVerticalScroll(Vector2 current, Vector2 previous)
        {
            var isNext = current.y - previous.y < 0f;
            if (current.y < float.Epsilon)
                isNext = true;
            else if (current.y >= 1f)
                isNext = false;

            var isPositionLast = current.y <= float.Epsilon;
            return (isNext, isPositionLast);
        }

        protected static void ApplyHorizontalAnchors(RectTransform target, bool isReverse)
        {
            if (!isReverse)
            {
                target.anchorMin = Vector2.zero;
                target.anchorMax = Vector2.up;
                target.pivot = Vector2.up;
            }
            else
            {
                target.anchorMin = Vector2.right;
                target.anchorMax = Vector2.one;
                target.pivot = Vector2.one;
            }
        }

        protected static void ApplyVerticalAnchors(RectTransform target, bool isReverse)
        {
            if (isReverse)
            {
                target.anchorMin = Vector2.zero;
                target.anchorMax = Vector2.right;
                target.pivot = new Vector2(0.5f, 0f);
            }
            else
            {
                target.anchorMin = Vector2.up;
                target.anchorMax = Vector2.one;
                target.pivot = new Vector2(0.5f, 1f);
            }
        }

#if UNITY_EDITOR
        [SerializeField]
        protected DebugVirtualTestItem debugPrefab;
        [SerializeField, HideInInspector]
        protected bool prevIsReverse = false;

        protected virtual void OnValidate()
        {
            controller?.SetSpacing(Spacing);
            if (prevIsReverse != param.IsReverse)
            {
                SetReverseMode();
                prevIsReverse = param.IsReverse;
            }
        }


        private void DebugCreate()
        {
            Clear();
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += Clear;
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            var dataList = new ObservableCollection<int>();
            for (var count = 0; count < 100; count++)
                dataList.Add(count);
            var adapter = new FlyweightScrollViewDataAdapter<int>(dataList);
            var controller = new FlyweightScrollViewController<int, DebugVirtualTestItem>(debugPrefab, adapter);
            Setup(controller);
            controller.UpdateContainerSize();
            controller.UpdateView();
        }

        private void Clear()
        {
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload -= Clear;
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            controller?.Dispose();
            controller = null;
            UnityEngine.Debug.Log("Clear");
        }

        private void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange stateChange)
        {
            if (stateChange is UnityEditor.PlayModeStateChange.ExitingEditMode)
            {
                Clear();
            }
        }

        [UnityEditor.CustomEditor(typeof(FlyweightScrollView), true)]
        private class FlyweightScrollViewEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                if (GUILayout.Button("Debug Simulate"))
                    (target as FlyweightScrollView).DebugCreate();
                if (GUILayout.Button("Clear"))
                    (target as FlyweightScrollView).Clear();
            }
        }
#endif

    }
}
