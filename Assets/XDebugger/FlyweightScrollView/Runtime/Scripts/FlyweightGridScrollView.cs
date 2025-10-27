using UnityEngine;

namespace Xeon.Common.FlyweightScrollView
{
    [ExecuteInEditMode]
    public class FlyweightGridScrollView : FlyweightScrollView
    {
        [SerializeField]
        private GridScrollOrientation orientation = GridScrollOrientation.Vertical;
        [SerializeField]
        private HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left;
        [SerializeField]
        private VerticalAlignment verticalAlignment = VerticalAlignment.Top;
        [SerializeField]
        private Vector2Int gridSize = Vector2Int.one;

        public override void Setup(FlyweightScrollViewControllerBase controller)
        {
            base.Setup(controller);

            if (orientation == GridScrollOrientation.Horizontal)
            {
                controller.Setup(scrollView, param, content, verticalAlignment);
            }
            else
            {
                controller.Setup(scrollView, param, content, horizontalAlignment);
            }

            ApplyConfiguration();
        }

        public void SetColumnCount(int count)
        {
            gridSize = new Vector2Int(Mathf.Max(1, count), Mathf.Max(1, gridSize.y));
            controller?.SetGridSize(gridSize);
        }

        public void SetRowCount(int count)
        {
            gridSize = new Vector2Int(Mathf.Max(1, gridSize.x), Mathf.Max(1, count));
            controller?.SetGridSize(gridSize);
        }

        public void SetGridSize(int columns, int rows)
        {
            gridSize = new Vector2Int(Mathf.Max(1, columns), Mathf.Max(1, rows));
            controller?.SetGridSize(gridSize);
        }

        protected override void OnChangedScrollPosition(Vector2 position)
        {
            if (controller == null)
                return;

            bool isNext;
            float normalized;
            bool isPositionLast;

            switch (orientation)
            {
                case GridScrollOrientation.Horizontal:
                    isNext = position.x - prevScrollPosition.x < 0f;
                    normalized = position.x;
                    isPositionLast = position.x <= float.Epsilon;
                    break;
                default:
                    isNext = position.y - prevScrollPosition.y < 0f;
                    if (position.y < float.Epsilon)
                        isNext = true;
                    else if (position.y >= 1f)
                        isNext = false;
                    normalized = position.y;
                    isPositionLast = position.y <= float.Epsilon;
                    break;
            }

            prevScrollPosition = position;
            controller.Update(isNext, normalized, isPositionLast);
        }

        protected override void SetReverseMode()
        {
            if (orientation == GridScrollOrientation.Horizontal)
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
            }
            else
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
            }

            controller?.SetIsReverse(param.IsReverse);
        }

        private void ApplyConfiguration()
        {
            if (controller == null)
                return;

            controller.SetHorizontalAlignment(horizontalAlignment);
            controller.SetVerticalAlignment(verticalAlignment);

            gridSize = new Vector2Int(Mathf.Max(1, gridSize.x), Mathf.Max(1, gridSize.y));
            controller.SetGridSize(gridSize);

            switch (orientation)
            {
                case GridScrollOrientation.Horizontal:
                    controller.ConfigureForHorizontal(verticalAlignment);
                    break;
                case GridScrollOrientation.Both:
                    controller.ConfigureForBoth(horizontalAlignment, verticalAlignment);
                    break;
                default:
                    controller.ConfigureForVertical(horizontalAlignment);
                    break;
            }

            SetReverseMode();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            gridSize = new Vector2Int(Mathf.Max(1, gridSize.x), Mathf.Max(1, gridSize.y));
            ApplyConfiguration();
            base.OnValidate();
        }
#endif
    }
}

