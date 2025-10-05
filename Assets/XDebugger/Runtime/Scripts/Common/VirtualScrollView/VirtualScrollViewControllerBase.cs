using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Xeon.Common
{
    public abstract class VirtualScrollViewControllerBase : IDisposable
    {
        // ====================================================================================================
        // Fields & Properties
        // ====================================================================================================

        // Fields
        private LinkedList<VirtualScrollViewItemBase> itemList = new();
        private Action<int> onChangedItemCount;

        protected float scrollPosition = 0f;
        protected int headIndex = 0;
        protected int tailIndex = 0;

        // Setupで初期化される変数
        protected Vector2 itemSize;
        protected RectTransform viewPort;
        protected RectTransform container;
        protected RectOffset padding;
        protected float spacing = 0f;
        protected HorizontalAlignment horizontalAlignment;
        protected VerticalAlignment verticalAlignment;
        protected bool isControlChildSize = false;
        protected bool isReverse = false;
        protected bool isVertical = true;

        // Properties
        public abstract int ItemCount { get; }
        protected float itemHeight => itemSize.y + spacing;
        protected float itemWidth => itemSize.x + spacing;


        // ====================================================================================================
        // Constructor
        // ====================================================================================================

        public VirtualScrollViewControllerBase() { }

        public VirtualScrollViewControllerBase(Action<int> onChangedItemCount = null)
        {
            this.onChangedItemCount = onChangedItemCount;
        }


        // ====================================================================================================
        // Public Methods (API)
        // ====================================================================================================

        /// <summary>
        /// スクロールビューを初期化します。
        /// </summary>
        public void Setup(RectTransform viewPort, RectTransform container, RectOffset padding, float spacing, HorizontalAlignment alignment, bool isControlChildSize, bool isReverse)
        {
            this.viewPort = viewPort;
            this.container = container;
            this.padding = padding;
            this.spacing = spacing;
            horizontalAlignment = alignment;
            this.isControlChildSize = isControlChildSize;
            this.isReverse = isReverse;
            isVertical = true;

            headIndex = 0;
            tailIndex = Mathf.CeilToInt(viewPort.rect.height / itemHeight);
            CreateItems();
        }

        /// <summary>
        /// スクロールビューを初期化します。
        /// </summary>
        public void Setup(RectTransform viewPort, RectTransform container, RectOffset padding, float spacing, VerticalAlignment alignment, bool isControlChildSize, bool isReverse)
        {
            this.viewPort = viewPort;
            this.container = container;
            this.padding = padding;
            this.spacing = spacing;
            verticalAlignment = alignment;
            this.isControlChildSize = isControlChildSize;
            this.isReverse = isReverse;
            isVertical = false;

            headIndex = 0;
            tailIndex = Mathf.CeilToInt(viewPort.rect.width / itemWidth);
            CreateItems();
        }

        /// <summary>
        /// スクロール位置に応じてビューを更新します。
        /// </summary>
        public void Update(bool isNext, float normalizedPosition)
        {
            scrollPosition = normalizedPosition;
            UpdateInternal();

            if (isNext)
                RepositionForNext();
            else
                RepositionForPrev();
        }

        /// <summary>
        /// 現在のインデックスに基づいてビュー全体を再描画します。
        /// </summary>
        public void UpdateView()
        {
            var index = headIndex;
            foreach (var item in itemList)
            {
                item.SetPosition(CreatePosition(index));
                OnChangedItemIndex(index, item);
                item.UpdateIsInside();
                item.gameObject.SetActive(true);
                index++;
            }
        }

        /// <summary>
        /// ItemCountが変更された際のコールバックを設定します。
        /// </summary>
        public void SetOnChangedItemCount(Action<int> onChangedItemCount)
            => this.onChangedItemCount = onChangedItemCount;

        /// <summary>
        /// アイテム間のスペースを設定し、ビューを更新します。
        /// </summary>
        public void SetSpacing(float spacing)
        {
            this.spacing = spacing;
            UpdateContainerSize();
            (headIndex, tailIndex) = CalculateIndex();
            CreateItems();
            UpdateView();
        }

        public void SetHorizontalAlignment(HorizontalAlignment horizontalAlignment)
        {
            this.horizontalAlignment = horizontalAlignment;
            foreach (var item in itemList)
                item.SetHorizontalAlignment(horizontalAlignment);
        }

        public void SetVerticalAlignment(VerticalAlignment verticalAlignment)
        {
            this.verticalAlignment = verticalAlignment;
            foreach(var item in itemList)
                item.SetVerticalAlignment(verticalAlignment);
        }

        public void SetIsReverse(bool isReverse)
        {
            this.isReverse = isReverse;
            UpdateView();

        }

        public virtual void Dispose()
        {
            if (itemList == null) return;
            foreach (var item in itemList)
            {
                if (Application.isPlaying)
                    GameObject.Destroy(item.gameObject);
                else
                    GameObject.DestroyImmediate(item.gameObject);
            }
            itemList.Clear();
        }


        // ====================================================================================================
        // Protected Methods (For Derived Classes & Core Logic)
        // ====================================================================================================

        /// <summary>
        /// 下方向にスクロールした際のアイテム再配置処理。
        /// </summary>
        protected void RepositionForNext()
        {
            var item = itemList.First;
            var (newHeadIndex, newTailIndex) = CalculateIndex();
            if (Mathf.Abs(newHeadIndex - headIndex) >= itemList.Count)
            {
                headIndex = newHeadIndex;
                tailIndex = newTailIndex;
                UpdateView();
                return;
            }

            while (!item.Value.IsInside)
            {
                if (tailIndex >= ItemCount - 1)
                    break;
                headIndex++;
                tailIndex++;
                OnChangedItemIndex(tailIndex, item.Value);
                item.Value.SetPosition(CreatePosition(tailIndex));
                var tmp = item.Value;
                item = item.Next;
                itemList.RemoveFirst();
                itemList.AddLast(tmp);
            }
        }

        /// <summary>
        /// 上方向にスクロールした際のアイテム再配置処理。
        /// </summary>
        protected void RepositionForPrev()
        {
            var item = itemList.Last;
            var (newHeadIndex, newTailIndex) = CalculateIndex();
            if (Mathf.Abs(newHeadIndex - headIndex) >= itemList.Count)
            {
                headIndex = newHeadIndex;
                tailIndex = newTailIndex;
                UpdateView();
                return;
            }

            while (!item.Value.IsInside)
            {
                if (headIndex <= 0)
                    break;
                headIndex--;
                tailIndex--;
                item.Value.SetPosition(CreatePosition(headIndex));
                OnChangedItemIndex(headIndex, item.Value);
                var tmp = item.Value;
                item = item.Previous;
                itemList.RemoveLast();
                itemList.AddFirst(tmp);
            }
        }

        /// <summary>
        /// 総アイテム数が表示可能数に満たない場合の表示更新処理。
        /// </summary>
        protected void UpdateNotEnoughData()
        {
            if (ItemCount > itemList.Count)
                return;

            foreach (var (item, index) in itemList.Select((item, index) => (item, index)))
            {
                if (index >= ItemCount)
                {
                    item.gameObject.SetActive(false);
                    continue;
                }
                var dataIndex = headIndex + index;
                item.gameObject.SetActive(true);
                item.SetPosition(CreatePosition(dataIndex));
                OnChangedItemIndex(dataIndex, item);
            }
        }

        /// <summary>
        /// データソースのアイテム数が変更されたときに呼び出されるイベントハンドラ。
        /// </summary>
        protected void OnChangedItemCount(object sender, EventArgs e)
        {
            UpdateContainerSize();
            UpdateInternal();
            if (isReverse)
            {
                (headIndex, tailIndex) = CalculateIndex();
                UpdateView();
            }
            onChangedItemCount?.Invoke(ItemCount);
        }

        /// <summary>
        /// スクロールコンテンツ全体のサイズを更新します。
        /// </summary>
        public void UpdateContainerSize()
        {
            var size = container.sizeDelta;
            if (isVertical)
                size.y = ItemCount * itemHeight + padding.vertical;
            else
                size.x = ItemCount * itemWidth + padding.horizontal;
            container.sizeDelta = size;
        }

        /// <summary>
        /// スクロール位置から、表示すべきアイテムの先頭と末尾のインデックスを計算します。
        /// </summary>
        protected (int headIndex, int tailIndex) CalculateIndex()
        {
            var desiredHead = isVertical ? CalculateIndexForVertical() : CalculateIndexForHorizontal();

            var maxHead = Mathf.Max(0, ItemCount - itemList.Count);
            desiredHead = Mathf.Clamp(desiredHead, 0, maxHead);
            return (desiredHead, desiredHead + itemList.Count - 1);
        }

        private int CalculateIndexForVertical()
        {
            var totalContentHeight = ItemCount * itemHeight + padding.vertical;
            var viewPortHeight = viewPort.rect.height;
            var maxScroll = totalContentHeight - viewPortHeight;
            var contentOffset = (1f - Mathf.Clamp01(scrollPosition)) * maxScroll;
            return Mathf.FloorToInt(contentOffset / itemHeight);
        }

        private int CalculateIndexForHorizontal()
        {
            var totalContentWidth = ItemCount * itemWidth + padding.horizontal;
            var viewPortWidth = viewPort.rect.width;
            var maxScroll = totalContentWidth - viewPortWidth;
            var contentOffset = Mathf.Clamp01(scrollPosition) * maxScroll;
            return Mathf.FloorToInt(contentOffset / itemWidth);
        }

        /// <summary>
        /// 指定されたインデックスのアイテムが配置されるべきローカル座標を計算します。
        /// </summary>
        protected Vector3 CreatePosition(int index)
        {
            if (isVertical)
            {
                var x = horizontalAlignment switch
                {
                    HorizontalAlignment.Left => padding.left,
                    HorizontalAlignment.Center => 0f,
                    HorizontalAlignment.Right => -padding.right,
                    _ => 0f
                };
                return new Vector3(x, -index * itemHeight - padding.top, 0f);
            }
            else
            {
                var y = verticalAlignment switch
                {
                    VerticalAlignment.Top => -padding.top,
                    VerticalAlignment.Middle => 0,
                    VerticalAlignment.Bottom => padding.bottom,
                    _ => 0f
                };
                return new Vector3(index * itemWidth + padding.left, y, 0f);
            }
        }


        // ====================================================================================================
        // Private Methods (Implementation Details)
        // ====================================================================================================

        /// <summary>
        /// 内部的な更新処理。
        /// </summary>
        private void UpdateInternal()
        {
            UpdateNotEnoughData();
            foreach (var item in itemList)
                item.UpdateIsInside();
        }

        /// <summary>
        /// 表示に必要なアイテムオブジェクトを生成または破棄します。
        /// </summary>
        private void CreateItems()
        {
            var itemCount = 0;
            
            if (isVertical)
                itemCount = Mathf.CeilToInt(viewPort.rect.height / itemHeight) + 1;
            else
                itemCount = Mathf.CeilToInt(viewPort.rect.width / itemWidth) + 1;

            if (itemList.Count == itemCount)
                return;

            foreach (var item in itemList)
            {
                if (Application.isPlaying)
                    GameObject.Destroy(item.gameObject);
                else
                    GameObject.DestroyImmediate(item.gameObject);
            }
            itemList.Clear();

            for (var index = 0; index < itemCount; index++)
            {
                var item = CreateItem(headIndex + index);
                if (isControlChildSize)
                {
                    if (isVertical)
                        item.SetFittingItemWith(container.rect.width - padding.horizontal);
                    else
                        item.SetFittingItemHeight(container.rect.height - padding.vertical);
                }
                item.gameObject.SetActive(false);
                itemList.AddLast(item);
            }
        }


        // ====================================================================================================
        // Abstract Methods (Must be implemented by Derived Classes)
        // ====================================================================================================

        /// <summary>
        /// 指定したインデックスに対応するアイテムのインスタンスを生成します。
        /// </summary>
        protected abstract VirtualScrollViewItemBase CreateItem(int index);

        /// <summary>
        /// アイテムの表示内容を、指定したインデックスのデータで更新します。
        /// </summary>
        protected abstract void OnChangedItemIndex(int index, VirtualScrollViewItemBase target);
    }
}