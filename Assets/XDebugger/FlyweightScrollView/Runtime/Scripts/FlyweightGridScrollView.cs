using UnityEngine;

namespace Xeon.Common.FlyweightScrollView
{
    public class FlyweightGridScrollView : FlyweightScrollView
    {
        [SerializeField]
        private GridScrollOrientation orientation;
        [SerializeField]
        private HorizontalAlignment horizontalAlignment;
        [SerializeField]
        private VerticalAlignment verticalAlignment;
        [SerializeField]
        private Vector2Int gridCounts;


        public override void Setup(FlyweightScrollViewControllerBase controller)
        {
            base.Setup(controller);
            gridCounts = new Vector2Int(Mathf.Max(1, gridCounts.x), Mathf.Max(1, gridCounts.y));
            controller.Setup(scrollView, param, content, gridCounts, orientation, horizontalAlignment, verticalAlignment);
        }

        protected override void OnChangedScrollPosition(Vector2 position)
        {
            if (controller == null)
                return;

            bool isNext;
            bool isPositionLast;
            float normalizedPosition;

            switch (orientation)
            {
                case GridScrollOrientation.Horizontal:
                    (isNext, isPositionLast) = EvaluateHorizontalScroll(position, prevScrollPosition);
                    normalizedPosition = position.x;
                    break;
                case GridScrollOrientation.Vertical:
                case GridScrollOrientation.Both:
                    (isNext, isPositionLast) = EvaluateVerticalScroll(position, prevScrollPosition);
                    normalizedPosition = position.y;
                    break;
                default:
                    throw new System.Exception($"{orientation} is not supported");
            }

            prevScrollPosition = position;
            controller.Update(isNext, normalizedPosition, isPositionLast);
        }

        protected override void SetReverseMode()
        {
            switch (orientation)
            {
                case GridScrollOrientation.Horizontal:
                    ApplyHorizontalAnchors(content, param.IsReverse);
                    break;
                case GridScrollOrientation.Vertical:
                case GridScrollOrientation.Both:
                    ApplyVerticalAnchors(content, param.IsReverse);
                    break;
                default:
                    throw new System.Exception($"{orientation} is not supported");
            }

            controller?.SetIsReverse(param.IsReverse);
        }
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            gridCounts.x = Mathf.Max(1, gridCounts.x);
            gridCounts.y = Mathf.Max(1, gridCounts.y);
            controller?.SetGridCounts(gridCounts);
            switch (orientation)
            {
                case GridScrollOrientation.Horizontal:
                    controller?.ConfigureForHorizontal(verticalAlignment);
                    break;
                case GridScrollOrientation.Vertical:
                    controller?.ConfigureForVertical(horizontalAlignment);
                    break;
                case GridScrollOrientation.Both:
                    controller?.ConfigureForBoth(horizontalAlignment, verticalAlignment);
                    break;
                default:
                    throw new System.Exception($"{orientation} is not supported");
            }
            SetReverseMode();
            base.OnValidate();
        }
#endif
    }
}
