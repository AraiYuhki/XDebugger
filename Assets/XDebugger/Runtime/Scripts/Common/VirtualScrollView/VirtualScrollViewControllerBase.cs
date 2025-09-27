
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Xeon.Common
{
    public abstract class VirtualScrollViewControllerBase
    {
        private LinkedList<VirtualScrollViewItemBase> itemList = new();

        protected int headIndex = 0;
        protected int tailIndex = 0;
        protected float itemHeight;
        protected RectTransform viewPort;
        protected RectTransform container;
        private Action<int> onChangedItemCount;

        public VirtualScrollViewControllerBase(Action<int> onChangedItemCount = null)
        {
            this.onChangedItemCount = onChangedItemCount;
        }

        public abstract int ItemCount { get; }

        public void Setup(float itemHeight, RectTransform viewPort, RectTransform container)
        {
            this.itemHeight = itemHeight;
            this.viewPort = viewPort;
            this.container = container;

            tailIndex = Mathf.CeilToInt(viewPort.rect.height / itemHeight);
            var itemCount = tailIndex + 1;

            for (var index = 0; index < itemCount; index++)
            {
                var item = CreateItem(index);
                item.gameObject.SetActive(false);
                itemList.AddLast(item);
            }
        }


        public void Update()
        {
            if (ItemCount <= itemList.Count)
            {
                foreach (var (item, index) in itemList.Select((item, index) => (item, index)))
                {
                    if (index >= ItemCount)
                    {
                        item.gameObject.SetActive(false);
                        continue;
                    }
                    var dataIndex = headIndex + index;
                    item.gameObject.SetActive(true);
                    item.SetIndex(dataIndex);
                    OnChangedItemIndex(dataIndex, item);
                    item.SetPosition(CreatePosition(dataIndex));
                }
            }
            foreach (var item in itemList)
                item.UpdateIsInside();
        }

        public void RepositionForDown(float normalizedPosition)
        {
            var item = itemList.First;
            var (newHeadIndex, newTailIndex) = CalculateIndex(normalizedPosition);
            if (Mathf.Abs(newHeadIndex - headIndex) >= itemList.Count)
            {
                headIndex = newHeadIndex;
                tailIndex = newTailIndex;
                UpdateForJump();
                return;
            }

            while (!item.Value.IsInside)
            {
                if (tailIndex >= ItemCount - 1)
                    break;
                headIndex++;
                tailIndex++;
                item.Value.SetIndex(tailIndex);
                OnChangedItemIndex(tailIndex, item.Value);
                item.Value.SetPosition(CreatePosition(tailIndex));
                var tmp = item.Value;
                item = item.Next;
                itemList.RemoveFirst();
                itemList.AddLast(tmp);
            }
        }

        public void RepositionForUp(float normalizedPosition)
        {
            var item = itemList.Last;
            var (newHeadIndex, newTailIndex) = CalculateIndex(normalizedPosition);
            if (Mathf.Abs(newHeadIndex - headIndex) >= itemList.Count)
            {
                headIndex = newHeadIndex;
                tailIndex = newTailIndex;
                UpdateForJump();
                return;
            }

            while (!item.Value.IsInside)
            {
                if (headIndex <= 0)
                    break;
                headIndex--;
                tailIndex--;
                item.Value.SetIndex(headIndex);
                item.Value.SetPosition(CreatePosition(headIndex));
                OnChangedItemIndex(headIndex, item.Value);
                var tmp = item.Value;
                item = item.Previous;
                itemList.RemoveLast();
                itemList.AddFirst(tmp);
            }
        }

        protected abstract VirtualScrollViewItemBase CreateItem(int index);
        protected abstract void OnChangedItemIndex(int index, VirtualScrollViewItemBase target);

        protected Vector3 CreatePosition(int index) => new Vector3(0f, -index * itemHeight, 0f);

        protected void OnChangedItemCount(object sender, EventArgs e)
        {
            var size = container.sizeDelta;
            size.y = ItemCount * itemHeight;
            container.sizeDelta = size;
            Update();
            onChangedItemCount?.Invoke(ItemCount);
        }

        private void UpdateForJump()
        {
            var index = headIndex;
            foreach (var item in itemList)
            {
                item.SetIndex(index);
                item.SetPosition(CreatePosition(index));
                OnChangedItemIndex(index, item);
                item.UpdateIsInside();
                index++;
            }
        }

        private (int headIndex, int tailIndex) CalculateIndex(float normalizedPosition)
        {
            var totalContentHeight = ItemCount * itemHeight;
            var viewPortHeight = viewPort.rect.height;
            var maxScroll = totalContentHeight - viewPortHeight;
            var contentOffset = (1f - Mathf.Clamp01(normalizedPosition)) * maxScroll;
            var desiredHead = Mathf.FloorToInt(contentOffset / itemHeight);
            var maxHead = Mathf.Max(0, ItemCount - itemList.Count);
            desiredHead = Mathf.Clamp(desiredHead, 0, maxHead);
            return (desiredHead, desiredHead + itemList.Count - 1);

        }
    }
}