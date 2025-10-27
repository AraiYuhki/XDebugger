using UnityEngine;

namespace Xeon.Common.FlyweightScrollView
{
    public enum GridScrollOrientation
    {
        Vertical,
        Horizontal,
        Both,
    }

    public class GridLayouter : Layouter
    {
        private GridScrollOrientation orientation;
        private Vector2Int gridCounts = Vector2Int.one;
        private HorizontalAlignment horizontalAlignment;
        private VerticalAlignment verticalAlignment;
        private int lastItemCount;

        private float CellWidth => itemSize.x + spacing;
        private float CellHeight => itemSize.y + spacing;
        private float GetTotalWidth(int columns) => Mathf.Max(0, columns) * itemSize.x + Mathf.Max(0, columns - 1) * spacing;
        private float GetTotalHeight(int rows) => Mathf.Max(0, rows) * itemSize.y + Mathf.Max(0, rows - 1) * spacing;

        public override float ItemSize => orientation == GridScrollOrientation.Horizontal ? CellWidth : CellHeight;

        public GridLayouter(
            RectTransform container,
            FlyweightScrollViewParam param,
            Vector2 itemSize,
            GridScrollOrientation orientation,
            Vector2Int gridCounts,
            HorizontalAlignment horizontalAlignment,
            VerticalAlignment verticalAlignment) : base(container, param, itemSize)
        {
            this.orientation = orientation;
            this.gridCounts = new Vector2Int(Mathf.Max(1, gridCounts.x), Mathf.Max(1, gridCounts.y));
            this.horizontalAlignment = horizontalAlignment;
            this.verticalAlignment = verticalAlignment;
        }

        public void SetOrientation(GridScrollOrientation orientation)
        {
            this.orientation = orientation;
        }

        public void SetHorizontalAlignment(HorizontalAlignment alignment)
        {
            horizontalAlignment = alignment;
        }

        public void SetVerticalAlignment(VerticalAlignment alignment)
        {
            verticalAlignment = alignment;
        }

        public void SetGridCounts(Vector2Int gridCounts)
        {
            SetColumnCount(gridCounts.x);
            SetRowCount(gridCounts.y);
        }

        public void SetColumnCount(int count)
        {
            gridCounts.x = Mathf.Max(1, count);
        }

        public void SetRowCount(int count)
        {
            gridCounts.y = Mathf.Max(1, count);
        }

        public override int GetTailIndex()
        {
            var value = orientation switch
            {
                GridScrollOrientation.Horizontal => (Mathf.CeilToInt(viewPort.rect.width / CellWidth) + 1) * gridCounts.y - 1,
                GridScrollOrientation.Vertical => (Mathf.CeilToInt(viewPort.rect.height / CellHeight) + 1) * gridCounts.x - 1,
                GridScrollOrientation.Both => (Mathf.CeilToInt(viewPort.rect.height / CellHeight) + 1) * (Mathf.CeilToInt(viewPort.rect.width / CellWidth) + 1) - 1,
                _ => throw new System.Exception($"{orientation} is not supported"),
            };
            return Mathf.Max(0, value);
        }

        public override void SetItemSize(FlyweightScrollViewItemBase item)
        {
            var rect = item.RectTransform;
            var size = rect.sizeDelta;

            if (orientation != GridScrollOrientation.Horizontal || orientation == GridScrollOrientation.Both)
            {
                var availableWidth = Mathf.Max(0f, container.rect.width - padding.horizontal - spacing * (gridCounts.x - 1));
                var width = availableWidth / Mathf.Max(1, gridCounts.x);
                size.x = width;
                itemSize.x = width;
            }

            if (orientation != GridScrollOrientation.Vertical || orientation == GridScrollOrientation.Both)
            {
                var availableHeight = Mathf.Max(0f, container.rect.height - padding.vertical - spacing * (gridCounts.y - 1));
                var height = availableHeight / Mathf.Max(1, gridCounts.y);
                size.y = height;
                itemSize.y = height;
            }

            rect.sizeDelta = size;
        }

        public override void UpdateContainerSize(int itemCount)
        {
            lastItemCount = itemCount;
            var size = container.sizeDelta;
            switch (orientation)
            {
                case GridScrollOrientation.Horizontal:
                    size.x = GetContentSize(itemCount);
                    size.y = Mathf.Max(size.y, GetTotalHeight(gridCounts.y) + padding.vertical);
                    break;
                case GridScrollOrientation.Vertical:
                    size.y = GetContentSize(itemCount);
                    size.x = Mathf.Max(size.x, GetTotalWidth(gridCounts.x) + padding.horizontal);
                    break;
                case GridScrollOrientation.Both:
                    size.x = Mathf.Max(size.x, GetTotalWidth(Mathf.CeilToInt(itemCount / (float)gridCounts.y)) + padding.horizontal);
                    size.y = Mathf.Max(size.y, GetTotalHeight(Mathf.CeilToInt(itemCount / (float)gridCounts.x)) + padding.vertical);
                    break;
                default:
                    throw new System.Exception($"{orientation} is not supported.");
            }
            container.sizeDelta = size;
        }

        public override int CalculateIndex(int itemCount, float scrollPosition)
        {
            if (orientation == GridScrollOrientation.Horizontal)
            {
                var totalWidth = Mathf.Max(0f, Mathf.CeilToInt(itemCount / (float)gridCounts.y) * CellWidth + padding.horizontal);
                var viewWidth = viewPort.rect.width;
                var maxScrollWidth = Mathf.Max(0f, totalWidth - viewWidth);
                var horizontalOffset = Mathf.Clamp01(scrollPosition) * maxScrollWidth;
                var headColumn = Mathf.FloorToInt(horizontalOffset / CellWidth);
                return headColumn * gridCounts.y;
            }

            var totalHeight = Mathf.Max(0f, Mathf.CeilToInt(itemCount / (float)gridCounts.x) * CellHeight + padding.vertical);
            var viewHeight = viewPort.rect.height;
            var maxScrollHeight = Mathf.Max(0f, totalHeight - viewHeight);
            var verticalOffset = (orientation == GridScrollOrientation.Vertical ? 1f - Mathf.Clamp01(scrollPosition) : Mathf.Clamp01(scrollPosition)) * maxScrollHeight;
            var headRow = Mathf.FloorToInt(verticalOffset / CellHeight);
            return headRow * gridCounts.x;
        }

        public override Vector3 GetPosition(int index)
        {
            return orientation switch
            {
                GridScrollOrientation.Horizontal => GetHorizontalPosition(index),
                GridScrollOrientation.Vertical => GetVerticalPosition(index),
                GridScrollOrientation.Both => GetBothPosition(index),
                _ => throw new System.Exception($"{orientation} is not supported"),
            };
        }

        private Vector3 GetVerticalPosition(int index)
        {
            var row = Mathf.FloorToInt(index / (float)gridCounts.x);
            var column = index % gridCounts.x;
            var x = GetHorizontalOffset(column, gridCounts.x);
            var totalRows = Mathf.Max(1, Mathf.CeilToInt(lastItemCount / (float)gridCounts.x));
            var y = GetVerticalOffset(row, totalRows);
            return new Vector3(x, y, 0f);
        }

        private Vector3 GetHorizontalPosition(int index)
        {
            var column = Mathf.FloorToInt(index / (float)gridCounts.y);
            var row = index % gridCounts.y;
            var totalColumns = Mathf.Max(1, Mathf.CeilToInt(lastItemCount / (float)gridCounts.y));
            var x = GetHorizontalOffset(column, totalColumns);
            var y = GetVerticalOffset(row, gridCounts.y);
            return new Vector3(x, y, 0f);
        }

        private Vector3 GetBothPosition(int index)
        {
            if (gridCounts.x <= 0)
                gridCounts.x = 1;
            var row = Mathf.FloorToInt(index / (float)gridCounts.x);
            var column = index % gridCounts.x;
            var x = GetHorizontalOffset(column, gridCounts.x);
            var totalRows = Mathf.Max(1, Mathf.CeilToInt(lastItemCount / (float)gridCounts.x));
            var y = GetVerticalOffset(row, totalRows);
            return new Vector3(x, y, 0f);
        }

        private float GetHorizontalOffset(int column, int currentColumnCount)
        {
            var totalWidth = GetTotalWidth(currentColumnCount);
            return horizontalAlignment switch
            {
                HorizontalAlignment.Left => padding.left + column * CellWidth,
                HorizontalAlignment.Center => column * CellWidth - (totalWidth - itemSize.x) * 0.5f,
                HorizontalAlignment.Right => -padding.right - (currentColumnCount - 1 - column) * CellWidth,
                _ => padding.left + column * CellWidth,
            };
        }

        private float GetVerticalOffset(int row, int currentRowCount)
        {
            var totalHeight = GetTotalHeight(currentRowCount);
            return verticalAlignment switch
            {
                VerticalAlignment.Top => -padding.top - row * CellHeight,
                VerticalAlignment.Middle => -row * CellHeight + (totalHeight - itemSize.y) * 0.5f,
                VerticalAlignment.Bottom => padding.bottom + (currentRowCount - 1 - row) * CellHeight,
                _ => -padding.top - row * CellHeight,
            };
        }

        public override float GetContentSize(int itemCount)
        {
            return orientation switch
            {
                GridScrollOrientation.Horizontal => GetTotalWidth(Mathf.CeilToInt(itemCount / (float)gridCounts.y)) + padding.horizontal,
                GridScrollOrientation.Both => GetTotalHeight(Mathf.CeilToInt(itemCount / (float)gridCounts.x)) + padding.vertical,
                _ => GetTotalHeight(Mathf.CeilToInt(itemCount / (float)gridCounts.x)) + padding.vertical,
            };
        }

        public override float GetContentOffset(int itemCount, float scrollPosition)
        {
            if (orientation == GridScrollOrientation.Horizontal)
            {
                var totalWidth = GetTotalWidth(Mathf.CeilToInt(itemCount / (float)gridCounts.y)) + padding.horizontal;
                var viewWidth = viewPort.rect.width;
                var maxScrollWidth = Mathf.Max(0f, totalWidth - viewWidth);
                return Mathf.Clamp01(scrollPosition) * maxScrollWidth;
            }
            var totalHeight = GetTotalHeight(Mathf.CeilToInt(itemCount / (float)gridCounts.x)) + padding.vertical;
            var viewHeight = viewPort.rect.height;
            var maxScrollHeight = Mathf.Max(0f, totalHeight - viewHeight);
            var t = orientation == GridScrollOrientation.Vertical ? 1f - Mathf.Clamp01(scrollPosition) : Mathf.Clamp01(scrollPosition);
            return t * maxScrollHeight;
        }

        public override float GetScrollPositionFromOffset(int itemCount, float contentOffset)
        {
            if (orientation == GridScrollOrientation.Horizontal)
            {
                var totalWidth = GetTotalWidth(Mathf.CeilToInt(itemCount / (float)gridCounts.y)) + padding.horizontal;
                var viewWidth = viewPort.rect.width;
                var maxScrollWidth = Mathf.Max(0f, totalWidth - viewWidth);
                if (maxScrollWidth <= 0f)
                    return 0f;
                return Mathf.Clamp01(contentOffset / maxScrollWidth);
            }

            var totalHeight = GetTotalHeight(Mathf.CeilToInt(itemCount / (float)gridCounts.x)) + padding.vertical;
            var viewHeight = viewPort.rect.height;
            var maxScrollHeight = Mathf.Max(0f, totalHeight - viewHeight);
            if (maxScrollHeight <= 0f)
                return orientation == GridScrollOrientation.Vertical ? 1f : 0f;
            var normalized = Mathf.Clamp01(contentOffset / maxScrollHeight);
            return orientation == GridScrollOrientation.Vertical ? 1f - normalized : normalized;
        }
    }
}
