using UnityEngine;

namespace Xeon.Common.FlyweightScrollView
{
    [ExecuteInEditMode]
    public class FlyweightHorizontalScrollView : FlyweightScrollView
    {
        [SerializeField]
        private VerticalAlignment alignment = VerticalAlignment.Top;

        public override void Setup(FlyweightScrollViewControllerBase controller)
        {
            base.Setup(controller);
            controller.Setup(scrollView, param, content, alignment);
            controller.ConfigureForHorizontal(alignment);
        }

        protected override void OnChangedScrollPosition(Vector2 position)
        {
            if (controller == null)
                return;
            var isNext = position.x - prevScrollPosition.x < 0;
            prevScrollPosition = position;
            var isPositionLast = position.x <= float.Epsilon;
            controller.Update(isNext, position.x, isPositionLast);
        }

        protected override void SetReverseMode()
        {
            if (!param.IsReverse)
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
            controller?.SetIsReverse(param.IsReverse);
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
