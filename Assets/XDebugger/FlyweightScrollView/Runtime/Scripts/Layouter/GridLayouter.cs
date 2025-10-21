using UnityEngine;

namespace Xeon.Common.FlyweightScrollView
{
    public enum GridScrollDirection
    {
        Vertical,
        Horizontal,
    }

    public class GridLayouter : Layouter
    {
        private readonly GridScrollDirection direction;
        private readonly int constraintCount;
        private float crossSpacing;
        private HorizontalAlignment horizontalAlignment;
        private VerticalAlignment verticalAlignment;

        private int ItemsPerLine => Mathf.Max(1, constraintCount);
        private float LineSpacing => spacing;

        public GridLayouter(
            RectTransform container,
            FlyweightScrollViewParam param,
            Vector2 itemSize,
            GridScrollDirection direction,
            int constraintCount,
            HorizontalAlignment horizontalAlignment,
            VerticalAlignment verticalAlignment)
            : base(container, param, itemSize)
        {
            this.direction = direction;
            this.constraintCount = Mathf.Max(1, constraintCount);
            this.horizontalAlignment = horizontalAlignment;
            this.verticalAlignment = verticalAlignment;
            var gridSpacing = param.GridSpacing;
            crossSpacing = direction == GridScrollDirection.Vertical ? gridSpacing.x : gridSpacing.y;
        }

        public override float ItemSize => direction == GridScrollDirection.Vertical
            ? itemSize.y + LineSpacing
            : itemSize.x + LineSpacing;

        public override int GetTailIndex()
        {
            var perLine = ItemsPerLine;
            var lineSize = ItemSize;
            if (lineSize <= 0f)
                return perLine - 1;
            var viewportSize = direction == GridScrollDirection.Vertical
                ? viewPort.rect.height
                : viewPort.rect.width;
            var visibleLine = Mathf.CeilToInt(viewportSize / lineSize);
            return Mathf.Max(perLine - 1, perLine * (visibleLine + 1) - 1);
        }

        public override void SetItemSize(FlyweightScrollViewItemBase item)
        {
            var perLine = ItemsPerLine;
            if (direction == GridScrollDirection.Vertical)
            {
                var availableWidth = container.rect.width - padding.horizontal - crossSpacing * Mathf.Max(0, perLine - 1);
                var width = perLine <= 0 ? 0f : availableWidth / perLine;
                width = Mathf.Max(0f, width);
                item.SetFittingItemWidth(width);
                itemSize.x = width;
            }
            else
            {
                var availableHeight = container.rect.height - padding.vertical - crossSpacing * Mathf.Max(0, perLine - 1);
                var height = perLine <= 0 ? 0f : availableHeight / perLine;
                height = Mathf.Max(0f, height);
                item.SetFittingItemHeight(height);
                itemSize.y = height;
            }
        }

        public override void UpdateContainerSize(int itemCount)
        {
            var size = container.sizeDelta;
            if (direction == GridScrollDirection.Vertical)
            {
                var rows = Mathf.CeilToInt(itemCount / (float)ItemsPerLine);
                var contentHeight = CalculateAxisLength(rows, itemSize.y, LineSpacing) + padding.vertical;
                size.y = Mathf.Max(0f, contentHeight);
            }
            else
            {
                var columns = Mathf.CeilToInt(itemCount / (float)ItemsPerLine);
                var contentWidth = CalculateAxisLength(columns, itemSize.x, LineSpacing) + padding.horizontal;
                size.x = Mathf.Max(0f, contentWidth);
            }
            container.sizeDelta = size;
        }

        public override int CalculateIndex(int itemCount, float scrollPosition)
        {
            if (itemCount <= 0)
                return 0;

            var perLine = ItemsPerLine;
            var lineSize = ItemSize;
            if (lineSize <= 0f)
                return 0;

            if (direction == GridScrollDirection.Vertical)
            {
                var totalRows = Mathf.CeilToInt(itemCount / (float)perLine);
                var contentHeight = CalculateAxisLength(totalRows, itemSize.y, LineSpacing) + padding.vertical;
                var viewportHeight = viewPort.rect.height;
                var maxScroll = Mathf.Max(0f, contentHeight - viewportHeight);
                var offset = (1f - Mathf.Clamp01(scrollPosition)) * maxScroll;
                var row = Mathf.FloorToInt(offset / lineSize);
                var visibleRows = Mathf.CeilToInt(viewPort.rect.height / lineSize);
                var maxRow = Mathf.Max(0, totalRows - (visibleRows + 1));
                row = Mathf.Clamp(row, 0, maxRow);
                return row * perLine;
            }
            else
            {
                var totalColumns = Mathf.CeilToInt(itemCount / (float)perLine);
                var contentWidth = CalculateAxisLength(totalColumns, itemSize.x, LineSpacing) + padding.horizontal;
                var viewportWidth = viewPort.rect.width;
                var maxScroll = Mathf.Max(0f, contentWidth - viewportWidth);
                var offset = Mathf.Clamp01(scrollPosition) * maxScroll;
                var column = Mathf.FloorToInt(offset / lineSize);
                var visibleColumns = Mathf.CeilToInt(viewPort.rect.width / lineSize);
                var maxColumn = Mathf.Max(0, totalColumns - (visibleColumns + 1));
                column = Mathf.Clamp(column, 0, maxColumn);
                return column * perLine;
            }
        }

        public override Vector3 GetPosition(int index)
        {
            var perLine = ItemsPerLine;
            if (direction == GridScrollDirection.Vertical)
            {
                var row = Mathf.FloorToInt(index / (float)perLine);
                var column = index % perLine;
                var x = CalculateHorizontalOffset(column, perLine);
                var y = -padding.top - row * ItemSize;
                return new Vector3(x, y, 0f);
            }
            else
            {
                var column = Mathf.FloorToInt(index / (float)perLine);
                var row = index % perLine;
                var x = padding.left + column * ItemSize;
                var y = CalculateVerticalOffset(row, perLine);
                return new Vector3(x, y, 0f);
            }
        }

        public override float GetContentSize(int itemCount)
        {
            if (itemCount <= 0)
                return direction == GridScrollDirection.Vertical ? padding.vertical : padding.horizontal;

            var lines = Mathf.CeilToInt(itemCount / (float)ItemsPerLine);
            var baseSize = direction == GridScrollDirection.Vertical
                ? CalculateAxisLength(lines, itemSize.y, LineSpacing) + padding.vertical
                : CalculateAxisLength(lines, itemSize.x, LineSpacing) + padding.horizontal;
            return Mathf.Max(0f, baseSize);
        }

        public override float GetContentOffset(int itemCount, float scrollPosition)
        {
            if (itemCount <= 0)
                return 0f;

            var contentSize = GetContentSize(itemCount);
            var viewportSize = direction == GridScrollDirection.Vertical ? viewPort.rect.height : viewPort.rect.width;
            var maxScroll = Mathf.Max(0f, contentSize - viewportSize);
            if (maxScroll <= 0f)
                return 0f;

            if (direction == GridScrollDirection.Vertical)
                return (1f - Mathf.Clamp01(scrollPosition)) * maxScroll;
            return Mathf.Clamp01(scrollPosition) * maxScroll;
        }

        public override float GetScrollPositionFromOffset(int itemCount, float contentOffset)
        {
            if (itemCount <= 0)
                return direction == GridScrollDirection.Vertical ? 1f : 0f;

            var contentSize = GetContentSize(itemCount);
            var viewportSize = direction == GridScrollDirection.Vertical ? viewPort.rect.height : viewPort.rect.width;
            var maxScroll = Mathf.Max(0f, contentSize - viewportSize);
            if (maxScroll <= 0f)
                return direction == GridScrollDirection.Vertical ? 1f : 0f;

            var normalized = Mathf.Clamp01(contentOffset / maxScroll);
            return direction == GridScrollDirection.Vertical ? 1f - normalized : normalized;
        }

        public void SetHorizontalAlignment(HorizontalAlignment alignment)
        {
            horizontalAlignment = alignment;
        }

        public void SetVerticalAlignment(VerticalAlignment alignment)
        {
            verticalAlignment = alignment;
        }

        private static float CalculateAxisLength(int lineCount, float elementSize, float spacing)
        {
            if (lineCount <= 0)
                return 0f;
            return lineCount * elementSize + Mathf.Max(0, lineCount - 1) * spacing;
        }

        private float CalculateHorizontalOffset(int column, int columns)
        {
            var distance = itemSize.x + crossSpacing;
            return horizontalAlignment switch
            {
                HorizontalAlignment.Left => padding.left + column * distance,
                HorizontalAlignment.Center => (column - (columns - 1) / 2f) * distance + (padding.left - padding.right) * 0.5f,
                HorizontalAlignment.Right => -padding.right - (columns - 1 - column) * distance,
                _ => padding.left + column * distance,
            };
        }

        private float CalculateVerticalOffset(int row, int rows)
        {
            var distance = itemSize.y + crossSpacing;
            return verticalAlignment switch
            {
                VerticalAlignment.Top => -padding.top - row * distance,
                VerticalAlignment.Middle => -(row - (rows - 1) / 2f) * distance + (padding.bottom - padding.top) * 0.5f,
                VerticalAlignment.Bottom => padding.bottom + (rows - 1 - row) * distance,
                _ => -padding.top - row * distance,
            };
        }
    }
}
