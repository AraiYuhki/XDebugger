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
            if (controller is IFlyweightGridScrollViewController gridController)
            {
                gridController.ConfigureForVertical(alignment);
            }
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

            var isPositionLast = position.y <= float.Epsilon;
            controller.Update(isNext, position.y, isPositionLast);
        }

        protected override void SetReverseMode()
        {
            if (param.IsReverse)
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
