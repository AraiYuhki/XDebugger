using UnityEngine;

namespace Xeon.Common
{
    [ExecuteInEditMode]
    public class FlyweightVerticalScrollView : FlyweightScrollView
    {
        [SerializeField]
        private HorizontalAlignment alignment = HorizontalAlignment.Left;

        public override void Setup(FlyweightScrollViewControllerBase controller)
        {
            base.Setup(controller);
            controller.Setup(viewPort.RectTransform, content, padding, spacing, alignment, isControlChildSize, isReverse);
        }

        protected override void OnChangedScrollPosition(Vector2 position)
        {
            if (controller == null)
                return;

            var isNext = position.y - prevScrollPosition.y < 0;
            if (position.y < float.Epsilon)
                isNext = true;
            else if (position.y >= 1f)
                isNext = false;
            prevScrollPosition = position;

            controller.Update(isNext, position.y);
        }

        protected override void SetReverseMode()
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
        protected override void OnValidate()
        {
            controller?.SetHorizontalAlignment(alignment);
            base.OnValidate();
        }
#endif
    }
}
