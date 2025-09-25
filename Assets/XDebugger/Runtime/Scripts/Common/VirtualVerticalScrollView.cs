using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Xeon.Common
{
    public class VirtualVerticalScrollView : MonoBehaviour
    {
        [SerializeField]
        private ScrollRect scrollView;
        [SerializeField]
        private RectTransform viewPort;
        [SerializeField]
        private RectTransform content;
        [SerializeField]
        private GameObject itemPrefab;

        [SerializeField]
        private int itemCount = 100;

        private float itemHeight = 0f;
        private int viewCount = 0;
        
        private int headIndex = 0;
        private int tailIndex = 0;

        private readonly LinkedList<VirtualScrollItem> itemList = new();
        private Vector2 prevScrollPosition = Vector2.zero;
        
        private void Awake()
        {
            itemHeight = itemPrefab.GetComponent<RectTransform>().rect.height;
            tailIndex = Mathf.CeilToInt(viewPort.rect.height / itemHeight);
            viewCount = tailIndex + 1;
            for (var count = 0; count < viewCount; count++)
            {
                CreateItem(count);
            }
            var size = content.sizeDelta;
            size.y = itemCount * itemHeight;
            content.sizeDelta = size;

            scrollView.onValueChanged.AddListener(OnChangedScrollPosition);
            prevScrollPosition = scrollView.normalizedPosition;
        }

        private void CreateItem(int index)
        {
            var item = Instantiate(itemPrefab, content);
            item.name = $"Item({index})";
            var scrollItem = item.AddComponent<VirtualScrollItem>();
            scrollItem.Initialize(viewPort, index);
            scrollItem.RectTransform.anchoredPosition3D = CreatePosition(index);
            itemList.AddLast(scrollItem);
        }

        private void OnChangedScrollPosition(Vector2 position)
        {
            var isDown = position.y - prevScrollPosition.y < 0;
            prevScrollPosition = position;
            
            foreach (var item in itemList)
                item.UpdateIsInside();
            if (isDown)
                RepositionForDown();
            else
                RepositionForUp();
        }

        private void RepositionForDown()
        {
            var item = itemList.First;
            while (!item.Value.IsInside)
            {
                if (tailIndex >= itemCount - 1)
                    break;
                headIndex++;
                tailIndex++;
                item.Value.Index = tailIndex;
                item.Value.RectTransform.anchoredPosition3D = CreatePosition(tailIndex);
                item.Value.name = $"item({item.Value.Index})";
                var tmp = item.Value;
                item = item.Next;
                itemList.RemoveFirst();
                itemList.AddLast(tmp);
            }
        }

        private void RepositionForUp()
        {
            var item = itemList.Last;
            while (!item.Value.IsInside)
            {
                if (headIndex <= 0)
                    break;
                headIndex--;
                tailIndex--;
                item.Value.Index = headIndex;
                item.Value.RectTransform.anchoredPosition3D = CreatePosition(headIndex);
                item.Value.name = $"item({item.Value.Index})";
                var tmp = item.Value;
                item = item.Previous;
                itemList.RemoveLast();
                itemList.AddFirst(tmp);
            }
        }

        private Vector3 CreatePosition(int index)
        {
            return new Vector3(0f, -index * itemHeight, 0f);
        }
    }
}
