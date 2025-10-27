using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Xeon.Common.FlyweightScrollView
{
    public abstract class FlyweightScrollViewControllerBase : IDisposable
    {
        // ====================================================================================================
        // Fields & Properties
        // ====================================================================================================

        // Fields
        private LinkedList<FlyweightScrollViewItemBase> itemList = new();
        private Action<int> onChangedItemCount;

        protected float scrollPosition = 0f;
        protected int headIndex = 0;
        protected int tailIndex = 0;

        // Setupで初期化される変数
        protected Vector2 itemSize;
        protected ScrollRect scrollView;
        protected FlyweightScrollViewParam param;
        protected RectTransform viewPort => param.ViewPort;
        protected RectTransform container;
        protected RectOffset padding => param.Padding;
        protected float spacing => param.Spacing;
        protected bool isControlChildSize => param.IsControlChildSize;
        protected bool isReverse => param.IsReverse;
        protected bool isAtLastSticky => param.IsAtLastSticky;

        protected VerticalAlignment verticalAlignment;
        protected HorizontalAlignment horizontalAlignment;

        protected bool isPositionLast = false;
        protected bool isItemCountChanging = false;
        protected Layouter layouter;

        // Properties
        public abstract int ItemCount { get; }
        public bool IsDirty { get; set; }


        // ====================================================================================================
        // Constructor
        // ====================================================================================================

        public FlyweightScrollViewControllerBase() { }

        public FlyweightScrollViewControllerBase(Action<int> onChangedItemCount = null)
        {
            this.onChangedItemCount = onChangedItemCount;
        }


        // ====================================================================================================
        // Public Methods (API)
        // ====================================================================================================

        /// <summary>
        /// スクロールビューを初期化します。
        /// </summary>
        public void Setup(ScrollRect scrollView, FlyweightScrollViewParam param, RectTransform container, HorizontalAlignment alignment)
        {
            this.scrollView = scrollView;
            this.container = container;
            this.param = param;
            horizontalAlignment = alignment;
            layouter = new VerticalLayouter(container, param, itemSize, alignment);

            UpdateViewportSize();
        }

        /// <summary>
        /// スクロールビューを初期化します。
        /// </summary>
        public void Setup(ScrollRect scrollView, FlyweightScrollViewParam param, RectTransform container, VerticalAlignment alignment)
        {
            this.scrollView = scrollView;
            this.container = container;
            this.param = param;
            verticalAlignment = alignment;
            layouter = new HorizontalLayouter(container, param, itemSize, alignment);

            UpdateViewportSize();
        }

        public void UpdateViewportSize()
        {
            tailIndex = layouter.GetTailIndex();
            tailIndex += headIndex;
            CreateItems();
            IsDirty = true;
        }

        /// <summary>
        /// スクロール位置に応じてビューを更新します。
        /// </summary>
        public void Update(bool isNext, float normalizedPosition, bool isPositionLast)
        {
            scrollPosition = normalizedPosition;
            if (!isItemCountChanging)
            {
                this.isPositionLast = isPositionLast;
            }
            isItemCountChanging = false;
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
            if (ItemCount < itemList.Count)
            {
                UpdateNotEnoughData();
                return;
            }
            (headIndex, tailIndex) = CalculateIndex();
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

        public virtual void FixToHead()
        {
            headIndex = 0;
            tailIndex = itemList.Count - 1;
            IsDirty = true;
        }

        public virtual void FixToLast()
        {
            tailIndex = ItemCount;
            headIndex = Mathf.Max(0, tailIndex - itemList.Count);
            IsDirty = true;
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
            layouter.Spacing = spacing;
            UpdateContainerSize();
            CreateItems();
            IsDirty = true;
        }

        public void SetHorizontalAlignment(HorizontalAlignment horizontalAlignment)
        {
            this.horizontalAlignment = horizontalAlignment;
            if (layouter is VerticalLayouter verticalLayouter)
                verticalLayouter.SetAlignment(horizontalAlignment);
            else if (layouter is GridLayouter gridLayouter)
                gridLayouter.SetHorizontalAlignment(horizontalAlignment);
            foreach (var item in itemList)
                item.SetHorizontalAlignment(horizontalAlignment);
        }

        public void SetVerticalAlignment(VerticalAlignment verticalAlignment)
        {
            this.verticalAlignment = verticalAlignment;
            if (layouter is HorizontalLayouter horizontalLayouter)
                horizontalLayouter.SetAlignment(verticalAlignment);
            else if (layouter is GridLayouter gridLayouter)
                gridLayouter.SetVerticalAlignment(verticalAlignment);
            foreach (var item in itemList)
                item.SetVerticalAlignment(verticalAlignment);
        }

        public virtual void ConfigureForVertical(HorizontalAlignment alignment) { }

        public virtual void ConfigureForHorizontal(VerticalAlignment alignment) { }

        public virtual void ConfigureForBoth(HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment) { }

        public virtual void SetGridSize(Vector2Int gridSize) { }

        public void SetIsReverse(bool isReverse)
        {
            param.IsReverse = isReverse;
            IsDirty = true;
        }

        public void SetIsPositionLast(bool flag)
        {
            isPositionLast = flag;
            if (isPositionLast)
                FixToLast();
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
                IsDirty = true;
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
                IsDirty = true;
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

        private void UpdateForAddItem()
        {
            UpdateContainerSize();
            if (isAtLastSticky && isPositionLast)
            {
                scrollView.normalizedPosition = Vector2.zero;
                FixToLast();
                return;
            }
            if (tailIndex >= ItemCount - 1)
                IsDirty = true;
        }

        private void UpdateForRemove()
        {
            // アイテム削除時はサイズ更新
            UpdateContainerSize();
            // 表示範囲より後ろが消えた場合は再描画不要
            // 表示中の範囲に影響がある場合のみ再描画
            if (headIndex >= ItemCount)
            {
                headIndex = Mathf.Max(0, ItemCount - itemList.Count);
                tailIndex = headIndex + itemList.Count - 1;
            }

            IsDirty = true;
        }

        /// <summary>
        /// データソースのアイテム数が変更されたときに呼び出されるイベントハンドラ。
        /// </summary>
        /// <summary>
        /// データソースのアイテム数が変更されたときに呼び出されるイベントハンドラ。
        /// 不要な再計算を避け、差分のみを更新します。
        /// </summary>
        protected void OnChangedItemCount(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    isItemCountChanging = true;
                    UpdateForAddItem();
                    break;

                case NotifyCollectionChangedAction.Remove:
                    isItemCountChanging = true;
                    UpdateForRemove();
                    break;

                case NotifyCollectionChangedAction.Reset:
                    // 全リセット時は完全再構築
                    isItemCountChanging = true;
                    UpdateContainerSize();
                    IsDirty = true;
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                default:
                    // 要素数に変化がない場合は再描画だけ
                    IsDirty = true;
                    break;
            }

            // 逆順モードのときのみ再計算を強制
            if (isReverse)
                IsDirty = true;

            // ItemCount変更通知
            onChangedItemCount?.Invoke(ItemCount);
        }


        /// <summary>
        /// スクロールコンテンツ全体のサイズを更新します。
        /// </summary>
        public void UpdateContainerSize()
        {
            layouter.UpdateContainerSize(ItemCount);
        }

        /// <summary>
        /// スクロール位置から、表示すべきアイテムの先頭と末尾のインデックスを計算します。
        /// </summary>
        protected (int headIndex, int tailIndex) CalculateIndex()
        {
            var desiredHead = layouter.CalculateIndex(ItemCount, scrollPosition);

            var maxHead = Mathf.Max(0, ItemCount - itemList.Count);
            desiredHead = Mathf.Clamp(desiredHead, 0, maxHead);
            return (desiredHead, desiredHead + itemList.Count - 1);
        }

        /// <summary>
        /// 指定されたインデックスのアイテムが配置されるべきローカル座標を計算します。
        /// </summary>
        protected Vector3 CreatePosition(int index)
        {
            return layouter.GetPosition(index);
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
            var itemCount = layouter.GetItemCount();

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
                    layouter.SetItemSize(item);
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
        protected abstract FlyweightScrollViewItemBase CreateItem(int index);

        /// <summary>
        /// アイテムの表示内容を、指定したインデックスのデータで更新します。
        /// </summary>
        protected abstract void OnChangedItemIndex(int index, FlyweightScrollViewItemBase target);
    }
}