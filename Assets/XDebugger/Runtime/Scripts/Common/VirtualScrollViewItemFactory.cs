using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Xeon.Common
{
    public class VirtualScrollViewItemFactory<TItem, TData>
        where TItem : MonoBehaviour, new()
    {
        protected TItem prefab;
        protected LinkedList<TItem> itemList = new();
        protected ObservableCollection<TData> dataList = new();
        protected float itemHeight = 0f;

        public VirtualScrollViewItemFactory(TItem original, int itemCount, Transform container, RectTransform viewPort)
        {
            prefab = original;
            itemHeight = prefab.GetComponent<RectTransform>().rect.height;

            for (var count = 0; count < itemCount; count++)
            {
                var item = GameObject.Instantiate(original, container);
                item.name = $"Item({count})";
                var scrollItem = item.GetComponent<VirtualScrollItem>() ?? item.gameObject.AddComponent<VirtualScrollItem>();
                scrollItem.Initialize(viewPort, count);
                scrollItem.RectTransform.anchoredPosition3D = CreatePosition(count);
                itemList.AddLast(item);
            }
        }

        private Vector3 CreatePosition(int index) => new Vector3(0f, -index * itemHeight, 0f);

    }
}
