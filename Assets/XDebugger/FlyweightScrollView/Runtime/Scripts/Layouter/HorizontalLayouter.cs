using UnityEngine;
namespace Xeon.Common.FlyweightScrollView
{
    public enum VerticalAlignment
    {
        Top,
        Middle,
        Bottom,
    }

    public class HorizontalLayouter : Layouter
    {
        private VerticalAlignment alignment;
        public HorizontalLayouter(RectTransform container, FlyweightScrollViewParam param, Vector2 itemSize, VerticalAlignment alignment)
            : base(container, param, itemSize)
        {
            this.alignment = alignment;
        }

        public override float ItemSize => itemSize.x + spacing;

        public void SetAlignment(VerticalAlignment alignment)
        {
            this.alignment = alignment;
        }

        public override int GetTailIndex() => Mathf.CeilToInt(viewPort.rect.width / ItemSize);
        public override float GetContentSize(int itemCount) => itemCount * ItemSize + padding.horizontal;
        public override Vector3 GetPosition(int index)
        {
            var value = alignment switch
            {
                VerticalAlignment.Top => -padding.top,
                VerticalAlignment.Middle => 0,
                VerticalAlignment.Bottom => padding.bottom,
                _ => 0f
            };
            return new Vector3(index * ItemSize + padding.left, value, 0f);
        }
        public override void SetItemSize(FlyweightScrollViewItemBase item)
        {
            item.SetFittingItemHeight(container.rect.height - padding.vertical);
        }
        public override void UpdateContainerSize(int itemCount)
        {
            var size = container.sizeDelta;
            size.x = itemCount * ItemSize + padding.horizontal;
            container.sizeDelta = size;
        }

        public override int CalculateIndex(int itemCount, float scrollPosition)
        {
            var totalContentWidth = itemCount * ItemSize + padding.horizontal;
            var viewPortWidth = viewPort.rect.width;
            var maxScroll = totalContentWidth - viewPortWidth;
            var contentOffset = Mathf.Clamp01(scrollPosition) * maxScroll;
            return Mathf.FloorToInt(contentOffset / ItemSize);
        }
    }
}
