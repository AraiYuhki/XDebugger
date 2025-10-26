using System;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Xeon.Common.FlyweightScrollView
{
    public enum GridScrollOrientation
    {
        Vertical,
        Horizontal,
        Both,
    }

    public interface IFlyweightGridScrollViewController
    {
        GridScrollOrientation Orientation { get; }
        int ColumnCount { get; }
        int RowCount { get; }

        void ConfigureForVertical(HorizontalAlignment alignment);
        void ConfigureForHorizontal(VerticalAlignment alignment);
        void ConfigureForBoth(HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment);
        void SetColumnCount(int columnCount);
        void SetRowCount(int rowCount);
        void SetGridSize(int columnCount, int rowCount);
    }

    public class FlyweightGridScrollViewController<TData, TItem> : FlyweightScrollViewControllerBase, IFlyweightGridScrollViewController
        where TItem : MonoBehaviour, IBindable<TData>
    {
        private readonly TItem prefab;
        private IObservableCollection<TData> dataList;
        private event Action<TItem> onItemCreated;

        private GridScrollOrientation orientation = GridScrollOrientation.Vertical;
        private int columnCount = 1;
        private int rowCount = 1;

        public event Action<TItem> OnItemCreated
        {
            add
            {
                onItemCreated -= value;
                onItemCreated += value;
            }
            remove => onItemCreated -= value;
        }

        public override int ItemCount => dataList == null ? 0 : dataList.Count;

        public GridScrollOrientation Orientation => orientation;
        public int ColumnCount => columnCount;
        public int RowCount => rowCount;

        public FlyweightGridScrollViewController(TItem prefab, ObservableCollection<TData> dataList, int columnCount, int rowCount, GridScrollOrientation orientation = GridScrollOrientation.Vertical, Action<TItem> onItemCreated = null)
            : this(prefab, new FlyweightScrollViewDataAdapter<TData>(dataList), columnCount, rowCount, orientation, onItemCreated)
        {
        }

        public FlyweightGridScrollViewController(TItem prefab, IObservableCollection<TData> dataList, int columnCount, int rowCount, GridScrollOrientation orientation = GridScrollOrientation.Vertical, Action<TItem> onItemCreated = null)
        {
            this.prefab = prefab;
            this.itemSize = prefab.GetComponent<RectTransform>().rect.size;
            this.dataList = dataList;
            this.dataList.CollectionChanged += OnChangedItemCount;
            this.onItemCreated += onItemCreated;

            SetGridSize(columnCount, rowCount);
            this.orientation = orientation;
        }

        public void SetDataList(IObservableCollection<TData> newDataList)
        {
            if (dataList != null)
                dataList.CollectionChanged -= OnChangedItemCount;

            dataList = newDataList;
            dataList.CollectionChanged += OnChangedItemCount;
            OnChangedItemCount(null, null);
        }

        public override void Dispose()
        {
            dataList?.Clear();
            UpdateContainerSize();
            dataList = null;
            base.Dispose();
        }

        public void SetColumnCount(int columnCount)
        {
            this.columnCount = Mathf.Max(1, columnCount);
            if (layouter is GridLayouter gridLayouter)
            {
                gridLayouter.SetColumnCount(this.columnCount);
                UpdateContainerSize();
                UpdateViewportSize();
            }
        }

        public void SetRowCount(int rowCount)
        {
            this.rowCount = Mathf.Max(1, rowCount);
            if (layouter is GridLayouter gridLayouter)
            {
                gridLayouter.SetRowCount(this.rowCount);
                UpdateContainerSize();
                UpdateViewportSize();
            }
        }

        public void SetGridSize(int columnCount, int rowCount)
        {
            this.columnCount = Mathf.Max(1, columnCount);
            this.rowCount = Mathf.Max(1, rowCount);
            if (layouter is GridLayouter gridLayouter)
            {
                gridLayouter.SetColumnCount(this.columnCount);
                gridLayouter.SetRowCount(this.rowCount);
                UpdateContainerSize();
                UpdateViewportSize();
            }
        }

        public void ConfigureForVertical(HorizontalAlignment alignment)
        {
            orientation = GridScrollOrientation.Vertical;
            horizontalAlignment = alignment;
            EnsureLayouter();
        }

        public void ConfigureForHorizontal(VerticalAlignment alignment)
        {
            orientation = GridScrollOrientation.Horizontal;
            verticalAlignment = alignment;
            EnsureLayouter();
        }

        public void ConfigureForBoth(HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
        {
            orientation = GridScrollOrientation.Both;
            this.horizontalAlignment = horizontalAlignment;
            this.verticalAlignment = verticalAlignment;
            EnsureLayouter();
        }

        private void EnsureLayouter()
        {
            if (container == null || param == null)
                return;
            if (layouter is GridLayouter gridLayouter)
            {
                gridLayouter.SetOrientation(orientation);
                gridLayouter.SetColumnCount(columnCount);
                gridLayouter.SetRowCount(rowCount);
                gridLayouter.SetHorizontalAlignment(horizontalAlignment);
                gridLayouter.SetVerticalAlignment(verticalAlignment);
            }
            else
            {
                layouter = new GridLayouter(container, param, itemSize, orientation, columnCount, rowCount, horizontalAlignment, verticalAlignment);
            }
            UpdateContainerSize();
            UpdateViewportSize();
        }

        protected override FlyweightScrollViewItemBase CreateItem(int index)
        {
            var instance = GameObject.Instantiate(prefab, container);
            instance.name = $"Item({index})";

            var scrollItem = new FlyweightScrollItem<TItem>(instance, viewPort, horizontalAlignment);
            scrollItem.SetVerticalAlignment(verticalAlignment);
            scrollItem.SetPosition(CreatePosition(index));
            onItemCreated?.Invoke(instance);
            return scrollItem;
        }

        protected override void OnChangedItemIndex(int index, FlyweightScrollViewItemBase target)
        {
            if (target is not FlyweightScrollItem<TItem> item)
                return;
            if (index < 0 || index >= ItemCount)
                return;
            if (isReverse)
                index = ItemCount - index - 1;

            var data = dataList[index];
            item.Value.Bind(data);
        }
    }
}
