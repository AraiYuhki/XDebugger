using UnityEngine;

namespace Xeon.Common.FlyweightScrollView
{
    public class GridLayouter : Layouter
    {
        private GridScrollOrientation orientation;
        private Vector2Int gridSize;
        private HorizontalAlignment horizontalAlignment;
        private VerticalAlignment verticalAlignment;
        private int lastItemCount;

        private float CellWidth => itemSize.x + spacing;
        private float CellHeight => itemSize.y + spacing;
        private float GetTotalWidth(int columns) => Mathf.Max(0, columns) * itemSize.x + Mathf.Max(0, columns - 1) * spacing;
        private float GetTotalHeight(int rows) => Mathf.Max(0, rows) * itemSize.y + Mathf.Max(0, rows - 1) * spacing;

        private int ColumnCount => Mathf.Max(1, gridSize.x);
        private int RowCount => Mathf.Max(1, gridSize.y);

        public override float ItemSize => orientation switch
        {
            GridScrollOrientation.Horizontal => CellWidth,
            _ => CellHeight,
        };

        public GridLayouter(
            RectTransform container,
            FlyweightScrollViewParam param,
            Vector2 itemSize,
            GridScrollOrientation orientation,
            Vector2Int gridSize,
            HorizontalAlignment horizontalAlignment,
            VerticalAlignment verticalAlignment) : base(container, param, itemSize)
        {
            this.orientation = orientation;
            this.gridSize = new Vector2Int(Mathf.Max(1, gridSize.x), Mathf.Max(1, gridSize.y));
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

        public void SetGridSize(Vector2Int size)
        {
            gridSize = new Vector2Int(Mathf.Max(1, size.x), Mathf.Max(1, size.y));
        }

        public override int GetTailIndex()
        {
            return orientation switch
            {
                GridScrollOrientation.Horizontal => Mathf.Max(0, (Mathf.CeilToInt(viewPort.rect.width / CellWidth) + 1) * RowCount - 1),
                GridScrollOrientation.Both => Mathf.Max(0, (Mathf.CeilToInt(viewPort.rect.height / CellHeight) + 1) * (Mathf.CeilToInt(viewPort.rect.width / CellWidth) + 1) - 1),
                _ => Mathf.Max(0, (Mathf.CeilToInt(viewPort.rect.height / CellHeight) + 1) * ColumnCount - 1),
            };
        }

        public override void SetItemSize(FlyweightScrollViewItemBase item)
        {
            var rect = item.RectTransform;
            var size = rect.sizeDelta;

            if (orientation != GridScrollOrientation.Horizontal || orientation == GridScrollOrientation.Both)
            {
                var availableWidth = Mathf.Max(0f, container.rect.width - padding.horizontal - spacing * (ColumnCount - 1));
                var width = availableWidth / Mathf.Max(1, ColumnCount);
                size.x = width;
                itemSize.x = width;
            }

            if (orientation != GridScrollOrientation.Vertical || orientation == GridScrollOrientation.Both)
            {
                var availableHeight = Mathf.Max(0f, container.rect.height - padding.vertical - spacing * (RowCount - 1));
                var height = availableHeight / Mathf.Max(1, RowCount);
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
                    size.y = Mathf.Max(size.y, GetTotalHeight(RowCount) + padding.vertical);
                    break;
                case GridScrollOrientation.Both:
                    size.x = Mathf.Max(size.x, GetTotalWidth(Mathf.CeilToInt(itemCount / (float)RowCount)) + padding.horizontal);
                    size.y = Mathf.Max(size.y, GetTotalHeight(Mathf.CeilToInt(itemCount / (float)ColumnCount)) + padding.vertical);
                    break;
                default:
                    size.y = GetContentSize(itemCount);
                    size.x = Mathf.Max(size.x, GetTotalWidth(ColumnCount) + padding.horizontal);
                    break;
            }
            container.sizeDelta = size;
        }

        public override int CalculateIndex(int itemCount, float scrollPosition)
        {
            switch (orientation)
            {
                case GridScrollOrientation.Horizontal:
                    var totalWidth = Mathf.Max(0f, Mathf.CeilToInt(itemCount / (float)RowCount) * CellWidth + padding.horizontal);
                    var viewWidth = viewPort.rect.width;
                    var maxScrollWidth = Mathf.Max(0f, totalWidth - viewWidth);
                    var horizontalOffset = Mathf.Clamp01(scrollPosition) * maxScrollWidth;
                    var headColumn = Mathf.FloorToInt(horizontalOffset / CellWidth);
                    return headColumn * RowCount;
                default:
                    var totalHeight = Mathf.Max(0f, Mathf.CeilToInt(itemCount / (float)ColumnCount) * CellHeight + padding.vertical);
                    var viewHeight = viewPort.rect.height;
                    var maxScrollHeight = Mathf.Max(0f, totalHeight - viewHeight);
                    var verticalOffset = (orientation == GridScrollOrientation.Vertical ? 1f - Mathf.Clamp01(scrollPosition) : Mathf.Clamp01(scrollPosition)) * maxScrollHeight;
                    var headRow = Mathf.FloorToInt(verticalOffset / CellHeight);
                    return headRow * ColumnCount;
            }
        }

        public override Vector3 GetPosition(int index)
        {
            return orientation switch
            {
                GridScrollOrientation.Horizontal => GetHorizontalPosition(index),
                GridScrollOrientation.Both => GetBothPosition(index),
                _ => GetVerticalPosition(index),
            };
        }

        private Vector3 GetVerticalPosition(int index)
        {
            var row = Mathf.FloorToInt(index / (float)ColumnCount);
            var column = index % ColumnCount;
            var x = GetHorizontalOffset(column, ColumnCount);
            var totalRows = Mathf.Max(1, Mathf.CeilToInt(lastItemCount / (float)ColumnCount));
            var y = GetVerticalOffset(row, totalRows);
            return new Vector3(x, y, 0f);
        }

        private Vector3 GetHorizontalPosition(int index)
        {
            var column = Mathf.FloorToInt(index / (float)RowCount);
            var row = index % RowCount;
            var totalColumns = Mathf.Max(1, Mathf.CeilToInt(lastItemCount / (float)RowCount));
            var x = GetHorizontalOffset(column, totalColumns);
            var y = GetVerticalOffset(row, RowCount);
            return new Vector3(x, y, 0f);
        }

        private Vector3 GetBothPosition(int index)
        {
            if (ColumnCount <= 0)
                gridSize.x = 1;
            var row = Mathf.FloorToInt(index / (float)ColumnCount);
            var column = index % ColumnCount;
            var x = GetHorizontalOffset(column, ColumnCount);
            var totalRows = Mathf.Max(1, Mathf.CeilToInt(lastItemCount / (float)ColumnCount));
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
                GridScrollOrientation.Horizontal => GetTotalWidth(Mathf.CeilToInt(itemCount / (float)RowCount)) + padding.horizontal,
                GridScrollOrientation.Both => GetTotalHeight(Mathf.CeilToInt(itemCount / (float)ColumnCount)) + padding.vertical,
                _ => GetTotalHeight(Mathf.CeilToInt(itemCount / (float)ColumnCount)) + padding.vertical,
            };
        }

        public override float GetContentOffset(int itemCount, float scrollPosition)
        {
            switch (orientation)
            {
                case GridScrollOrientation.Horizontal:
                    var totalWidth = GetTotalWidth(Mathf.CeilToInt(itemCount / (float)RowCount)) + padding.horizontal;
                    var viewWidth = viewPort.rect.width;
                    var maxScrollWidth = Mathf.Max(0f, totalWidth - viewWidth);
                    return Mathf.Clamp01(scrollPosition) * maxScrollWidth;
                default:
                    var totalHeight = GetTotalHeight(Mathf.CeilToInt(itemCount / (float)ColumnCount)) + padding.vertical;
                    var viewHeight = viewPort.rect.height;
                    var maxScrollHeight = Mathf.Max(0f, totalHeight - viewHeight);
                    var t = orientation == GridScrollOrientation.Vertical ? 1f - Mathf.Clamp01(scrollPosition) : Mathf.Clamp01(scrollPosition);
                    return t * maxScrollHeight;
            }
        }

        public override float GetScrollPositionFromOffset(int itemCount, float contentOffset)
        {
            switch (orientation)
            {
                case GridScrollOrientation.Horizontal:
                    var totalWidth = GetTotalWidth(Mathf.CeilToInt(itemCount / (float)RowCount)) + padding.horizontal;
                    var viewWidth = viewPort.rect.width;
                    var maxScrollWidth = Mathf.Max(0f, totalWidth - viewWidth);
                    if (maxScrollWidth <= 0f)
                        return 0f;
                    return Mathf.Clamp01(contentOffset / maxScrollWidth);
                default:
                    var totalHeight = GetTotalHeight(Mathf.CeilToInt(itemCount / (float)ColumnCount)) + padding.vertical;
                    var viewHeight = viewPort.rect.height;
                    var maxScrollHeight = Mathf.Max(0f, totalHeight - viewHeight);
                    if (maxScrollHeight <= 0f)
                        return orientation == GridScrollOrientation.Vertical ? 1f : 0f;
                    var normalized = Mathf.Clamp01(contentOffset / maxScrollHeight);
                    return orientation == GridScrollOrientation.Vertical ? 1f - normalized : normalized;
            }
        }
    }
}
