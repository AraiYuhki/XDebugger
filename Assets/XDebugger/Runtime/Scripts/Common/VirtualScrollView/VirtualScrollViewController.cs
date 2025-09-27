using System;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Xeon.Common
{
    public class VirtualScrollViewController<TData, TItem> : VirtualScrollViewControllerBase
        where TItem : MonoBehaviour, ISetupable<TData>
    {
        private ObservableCollection<TData> dataList;

        private TItem prefab;

        public override int ItemCount => dataList.Count;
        public float Size => dataList.Count * itemHeight;

        public VirtualScrollViewController(ObservableCollection<TData> dataList, Action<int> onChangedItemCount = null)
            :base(onChangedItemCount)
        {
            this.dataList = dataList;
            dataList.CollectionChanged += OnChangedItemCount;
        }

        public void Setup(TItem prefab, RectTransform viewPort, RectTransform container)
        {
            this.prefab = prefab;
            var itemHeight = prefab.GetComponent<RectTransform>().rect.height;
            Setup(itemHeight, viewPort, container);
        }

        protected override void OnChangedItemIndex(int index, VirtualScrollViewItemBase target)
        {
            if (target is NewVirtualScrollItem<TItem> item)
            {
                var data = dataList[index];
                item.Value.Setup(data);
            }
        }


        protected override VirtualScrollViewItemBase CreateItem(int index)
        {
            var instance = GameObject.Instantiate(prefab, container);
            instance.name = $"Item({index})";
            instance.gameObject.SetActive(false);
            var scrollItem = new NewVirtualScrollItem<TItem>(instance, viewPort, index);
            scrollItem.RectTransform.anchoredPosition3D = CreatePosition(index);
            return scrollItem;
        }
    }
}