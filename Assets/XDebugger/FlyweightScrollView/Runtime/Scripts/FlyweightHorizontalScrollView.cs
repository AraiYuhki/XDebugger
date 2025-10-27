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
        }

        protected override void OnChangedScrollPosition(Vector2 position)
        {
            if (controller == null)
                return;
            var (isNext, isPositionLast) = EvaluateHorizontalScroll(position, prevScrollPosition);
            prevScrollPosition = position;
            controller.Update(isNext, position.x, isPositionLast);
        }

        protected override void SetReverseMode()
        {
            ApplyHorizontalAnchors(content, param.IsReverse);
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
