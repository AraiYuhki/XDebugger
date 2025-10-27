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

    public class FlyweightGridScrollViewController<TData, TItem> : FlyweightScrollViewControllerBase
        where TItem : MonoBehaviour, IBindable<TData>
    {
        private readonly TItem prefab;
        private IObservableCollection<TData> dataList;
        private event Action<TItem> onItemCreated;

        private GridScrollOrientation orientation = GridScrollOrientation.Vertical;
        private Vector2Int gridSize = Vector2Int.one;

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
        public Vector2Int GridSize => gridSize;

        public FlyweightGridScrollViewController(
            TItem prefab,
            ObservableCollection<TData> dataList,
            int columnCount,
            int rowCount,
            GridScrollOrientation orientation = GridScrollOrientation.Vertical,
            Action<TItem> onItemCreated = null)
            : this(prefab, new FlyweightScrollViewDataAdapter<TData>(dataList), columnCount, rowCount, orientation, onItemCreated)
        {
        }

        public FlyweightGridScrollViewController(
            TItem prefab,
            IObservableCollection<TData> dataList,
            int columnCount,
            int rowCount,
            GridScrollOrientation orientation = GridScrollOrientation.Vertical,
            Action<TItem> onItemCreated = null)
        {
            this.prefab = prefab;
            this.itemSize = prefab.GetComponent<RectTransform>().rect.size;
            this.dataList = dataList;
            this.dataList.CollectionChanged += OnChangedItemCount;
            this.onItemCreated += onItemCreated;

            SetGridSizeInternal(new Vector2Int(columnCount, rowCount));
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

        public override void ConfigureForVertical(HorizontalAlignment alignment)
        {
            orientation = GridScrollOrientation.Vertical;
            horizontalAlignment = alignment;
            EnsureLayouter();
        }

        public override void ConfigureForHorizontal(VerticalAlignment alignment)
        {
            orientation = GridScrollOrientation.Horizontal;
            verticalAlignment = alignment;
            EnsureLayouter();
        }

        public override void ConfigureForBoth(HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
        {
            orientation = GridScrollOrientation.Both;
            this.horizontalAlignment = horizontalAlignment;
            this.verticalAlignment = verticalAlignment;
            EnsureLayouter();
        }

        public override void SetGridSize(Vector2Int gridSize)
        {
            SetGridSizeInternal(gridSize);
            if (layouter is GridLayouter gridLayouter)
            {
                gridLayouter.SetGridSize(this.gridSize);
                UpdateContainerSize();
                UpdateViewportSize();
            }
            else
            {
                EnsureLayouter();
            }
        }

        private void SetGridSizeInternal(Vector2Int newSize)
        {
            gridSize = new Vector2Int(Mathf.Max(1, newSize.x), Mathf.Max(1, newSize.y));
        }

        private void EnsureLayouter()
        {
            if (container == null || param == null)
                return;

            if (layouter is GridLayouter gridLayouter)
            {
                gridLayouter.SetOrientation(orientation);
                gridLayouter.SetGridSize(gridSize);
                gridLayouter.SetHorizontalAlignment(horizontalAlignment);
                gridLayouter.SetVerticalAlignment(verticalAlignment);
            }
            else
            {
                layouter = new GridLayouter(container, param, itemSize, orientation, gridSize, horizontalAlignment, verticalAlignment);
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
