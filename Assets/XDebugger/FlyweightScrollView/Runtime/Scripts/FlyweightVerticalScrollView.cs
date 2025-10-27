using UnityEngine;

namespace Xeon.Common.FlyweightScrollView
{
    [ExecuteInEditMode]
    public class FlyweightVerticalScrollView : FlyweightScrollView
    {
        [SerializeField]
        private HorizontalAlignment alignment = HorizontalAlignment.Left;

        public override void Setup(FlyweightScrollViewControllerBase controller)
        {
            base.Setup(controller);
            controller.Setup(scrollView, param, content, alignment);
        }

        protected override void OnChangedScrollPosition(Vector2 position)
        {
            if (controller == null)
                return;
            var (isNext, isPositionLast) = EvaluateVerticalScroll(position, prevScrollPosition);
            prevScrollPosition = position;
            controller.Update(isNext, position.y, isPositionLast);
        }

        protected override void SetReverseMode()
        {
            ApplyVerticalAnchors(content, param.IsReverse);
            controller?.SetIsReverse(param.IsReverse);
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
