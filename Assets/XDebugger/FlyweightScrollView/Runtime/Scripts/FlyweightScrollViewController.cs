using System;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Xeon.Common
{
    /// <summary>
    /// VirtualScrollViewControllerBaseのジェネリックな実装クラス。
    /// </summary>
    /// <typeparam name="TData">リストに表示するデータの型</typeparam>
    /// <typeparam name="TItem">表示に使用するUIアイテムのコンポーネントの型</typeparam>
    public class FlyweightScrollViewController<TData, TItem> : FlyweightScrollViewControllerBase
        where TItem : MonoBehaviour, IBindable<TData>
    {
        // ====================================================================================================
        // Fields & Properties
        // ====================================================================================================

        private IObservableCollection<TData> dataList;
        private readonly TItem prefab;

        public override int ItemCount => dataList.Count;


        // ====================================================================================================
        // Constructor
        // ====================================================================================================

        public FlyweightScrollViewController(TItem prefab, ObservableCollection<TData> dataList) : this(prefab, new FlyweightScrollViewDataAdapter<TData>(dataList))
        {
        }

        public FlyweightScrollViewController(TItem prefab, IObservableCollection<TData> dataList)
        {
            this.prefab = prefab;
            this.itemSize = prefab.GetComponent<RectTransform>().rect.size;
            this.dataList = dataList;
            this.dataList.CollectionChanged += OnChangedItemCount;
        }


        // ====================================================================================================
        // Public Methods
        // ====================================================================================================

        /// <summary>
        /// 表示するデータリストを差し替えます。
        /// </summary>
        public void SetDataList(IObservableCollection<TData> newDataList)
        {
            if (dataList != null)
            {
                dataList.CollectionChanged -= OnChangedItemCount;
            }

            dataList = newDataList;
            dataList.CollectionChanged += OnChangedItemCount;

            OnChangedItemCount(null, null);
        }

        public override void Dispose()
        {
            dataList.Clear();
            UpdateContainerSize();
            dataList = null;
            base.Dispose();
        }


        // ====================================================================================================
        // Protected Overrides (Base Class Implementation)
        // ====================================================================================================

        /// <summary>
        /// プレハブからアイテムのインスタンスを生成します。
        /// </summary>
        protected override FlyweightScrollViewItemBase CreateItem(int index)
        {
            var instance = GameObject.Instantiate(prefab, container);
            instance.name = $"Item({index})";
            instance.gameObject.SetActive(false);

            var scrollItem = new FlyweightlScrollItem<TItem>(instance, viewPort, horizontalAlignment);
            scrollItem.SetPosition(CreatePosition(index));

            return scrollItem;
        }

        /// <summary>
        /// アイテムの表示を、指定したインデックスのデータで更新します。
        /// </summary>
        protected override void OnChangedItemIndex(int index, FlyweightScrollViewItemBase target)
        {
            if (target is not FlyweightlScrollItem<TItem> item) return;
            if (index < 0 || index >= dataList.Count) return;
            if (isReverse)
                index = dataList.Count - index - 1;

            var data = dataList[index];
            item.Value.Bind(data);
        }
    }
}