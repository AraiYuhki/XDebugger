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
            controller.Setup(scrollView, param, content, gridCounts, orientation, horizontalAlignment, verticalAlignment);

        }

        protected override void OnChangedScrollPosition(Vector2 position)
        {
            if (controller == null)
                return;

        }

        protected override void SetReverseMode()
        {
            if (param.IsReverse)
            {

            }
            else
            {

            }

            controller?.SetIsReverse(param.IsReverse);
        }
#if UNITY_EDITOR
        protected override void OnValidate()
        {
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
            base.OnValidate();
        }
#endif
    }
}
