using System.Collections.ObjectModel;
using UnityEngine;

namespace Xeon.Common
{
    /// <summary>
    /// VirtualScrollViewControllerBaseのジェネリックな実装クラス。
    /// </summary>
    /// <typeparam name="TData">リストに表示するデータの型</typeparam>
    /// <typeparam name="TItem">表示に使用するUIアイテムのコンポーネントの型</typeparam>
    public class VirtualScrollViewController<TData, TItem> : VirtualScrollViewControllerBase
        where TItem : MonoBehaviour, ISetupable<TData>
    {
        // ====================================================================================================
        // Fields & Properties
        // ====================================================================================================

        private ObservableCollection<TData> dataList;
        private readonly TItem prefab;

        public override int ItemCount => dataList.Count;


        // ====================================================================================================
        // Constructor
        // ====================================================================================================

        public VirtualScrollViewController(TItem prefab, ObservableCollection<TData> dataList)
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
        public void SetDataList(ObservableCollection<TData> newDataList)
        {
            if (dataList != null)
            {
                dataList.CollectionChanged -= OnChangedItemCount;
            }

            dataList = newDataList;
            dataList.CollectionChanged += OnChangedItemCount;

            OnChangedItemCount(null, null);
        }


        // ====================================================================================================
        // Protected Overrides (Base Class Implementation)
        // ====================================================================================================

        /// <summary>
        /// プレハブからアイテムのインスタンスを生成します。
        /// </summary>
        protected override VirtualScrollViewItemBase CreateItem(int index)
        {
            var instance = GameObject.Instantiate(prefab, container);
            instance.name = $"Item({index})";
            instance.gameObject.SetActive(false);

            var scrollItem = new VirtualScrollItem<TItem>(instance, viewPort, horizontalAlignment);
            scrollItem.SetPosition(CreatePosition(index));

            return scrollItem;
        }

        /// <summary>
        /// アイテムの表示を、指定したインデックスのデータで更新します。
        /// </summary>
        protected override void OnChangedItemIndex(int index, VirtualScrollViewItemBase target)
        {
            if (target is not VirtualScrollItem<TItem> item) return;
            if (index < 0 || index >= dataList.Count) return;

            var data = dataList[index];
            item.Value.Setup(data);
        }
    }
}