using UnityEngine;
namespace Xeon.Common.FlyweightScrollView
{
    public enum HorizontalAlignment
    {
        Left,
        Center,
        Right,
    }

    public class VerticalLayouter : Layouter
    {
        private HorizontalAlignment alignment;

        public VerticalLayouter(RectTransform container, FlyweightScrollViewParam param, Vector2 itemSize, HorizontalAlignment alignment)
            : base(container, param, itemSize)
        {
            this.alignment = alignment;
        }

        public override float ItemSize => itemSize.y + spacing;

        public void SetAlignment(HorizontalAlignment alignment)
        {
            this.alignment = alignment;
        }

        public override int GetTailIndex() => Mathf.CeilToInt(viewPort.rect.height / ItemSize);
        public override float GetContentSize(int itemCount) => itemCount * ItemSize + padding.vertical;
        public override Vector3 GetPosition(int index)
        {
            var value = alignment switch
            {
                HorizontalAlignment.Left => padding.left,
                HorizontalAlignment.Center => 0f,
                HorizontalAlignment.Right => -padding.right,
                _ => 0f
            };
            return new Vector3(value, -index * ItemSize - padding.top, 0f);
        }
        public override void SetItemSize(FlyweightScrollViewItemBase item)
        {
            item.SetFittingItemWidth(container.rect.width - padding.horizontal);
        }
        public override void UpdateContainerSize(int itemCount)
        {
            var size = container.sizeDelta;
            size.y = itemCount * ItemSize + padding.vertical;
            container.sizeDelta = size;
        }

        public override int CalculateIndex(int itemCount, float scrollPosition)
        {
            var totalContentHeight = itemCount * ItemSize + padding.vertical;
            var viewPortHeight = viewPort.rect.height;
            var maxScroll = totalContentHeight - viewPortHeight;
            var contentOffset = (1f - Mathf.Clamp01(scrollPosition)) * maxScroll;
            return Mathf.FloorToInt(contentOffset / ItemSize);
        }

        public override float GetContentOffset(int itemCount, float scrollPosition)
        {
            var totalContentHeight = itemCount * ItemSize + padding.vertical;
            var viewPortHeight = viewPort.rect.height;
            var maxScroll = Mathf.Max(0f, totalContentHeight - viewPortHeight);
            // vertical uses inverted normalized (1 at top)
            return (1f - Mathf.Clamp01(scrollPosition)) * maxScroll;
        }

        public override float GetScrollPositionFromOffset(int itemCount, float contentOffset)
        {
            var totalContentHeight = itemCount * ItemSize + padding.vertical;
            var viewPortHeight = viewPort.rect.height;
            var maxScroll = Mathf.Max(0f, totalContentHeight - viewPortHeight);
            if (maxScroll <= 0f) return 1f; // stick to top if no scrollable area
            var t = Mathf.Clamp01(contentOffset / maxScroll);
            return 1f - t;
        }
    }
}
