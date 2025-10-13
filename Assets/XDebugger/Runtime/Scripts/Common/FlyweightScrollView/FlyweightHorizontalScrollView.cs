using UnityEngine;

namespace Xeon.Common
{
    [ExecuteInEditMode]
    public class FlyweightHorizontalScrollView : FlyweightScrollView
    {
        [SerializeField]
        private VerticalAlignment alignment = VerticalAlignment.Top;

        public override void Setup(FlyweightScrollViewControllerBase controller)
        {
            base.Setup(controller);
            controller.Setup(viewPort.RectTransform, content, padding, spacing, alignment, isControlChildSize, isReverse);
        }

        protected override void OnChangedScrollPosition(Vector2 position)
        {
            if (controller == null)
                return;
            var isNext = position.x - prevScrollPosition.x < 0;
            prevScrollPosition = position;
            controller.Update(isNext, position.x);
        }

        protected override void SetReverseMode()
        {
            if (!isReverse)
            {
                content.anchorMin = Vector2.zero;
                content.anchorMax = Vector2.up;
                content.pivot = Vector2.up;
            }
            else
            {
                content.anchorMin = Vector2.right;
                content.anchorMax = Vector2.one;
                content.pivot = Vector2.one;
            }
            controller?.SetIsReverse(isReverse);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            controller?.SetVerticalAlignment(alignment);
            base.OnValidate();
        }
#endif
    }
}
