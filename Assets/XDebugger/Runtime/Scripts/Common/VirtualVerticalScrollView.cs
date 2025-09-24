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
        private Scrollbar scrollBar;
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

        [SerializeField]
        private int headIndex = 0;
        [SerializeField]
        private int tailIndex = 0;

        private List<VirtualScrollItem> itemList = new();
        
        private void Awake()
        {
            itemHeight = itemPrefab.GetComponent<RectTransform>().rect.height;
            Debug.Log($"{viewPort.rect.height}::{itemHeight}");
            tailIndex = Mathf.CeilToInt(viewPort.rect.height / itemHeight);
            viewCount = tailIndex + 1;
            for (var count = 0; count < viewCount; count++)
            {
                var item = Instantiate(itemPrefab, content);
                var rectTransform = item.GetComponent<RectTransform>();
                rectTransform.anchorMax = new Vector2(0, 1);
                rectTransform.anchorMin = new Vector2(0, 1);
                rectTransform.pivot = new Vector2(0, 1);
                rectTransform.anchoredPosition3D = new Vector3(0f, -count * itemHeight, 0f);
                item.name = $"Item({count})";
                var scrollItem = item.AddComponent<VirtualScrollItem>();
                scrollItem.Initialize(viewPort);
                scrollItem.Index = count;
                itemList.Add(scrollItem);
            }
            var size = content.sizeDelta;
            size.y = viewCount * itemCount;
            content.sizeDelta = size;

            scrollView.onValueChanged.AddListener(OnChangedScrollPosition);
        }

        private void OnChangedScrollPosition(Vector2 position)
        {
            var viewPortRect = GetWorldRect(viewPort);
            foreach (var item in itemList)
            {
                var prevIsInside = item.IsInside;
                var currentIsInside = item.CheckIsInside();
                Debug.Log($"{item.name} {prevIsInside} => {currentIsInside}");
                // 初めて外に出た
                if (prevIsInside != currentIsInside && !currentIsInside)
                {
                    if (item.Index == headIndex)
                    {
                        headIndex++;
                        tailIndex++;
                        item.Index = tailIndex;
                        item.RectTransform.anchoredPosition3D = new Vector3(0f, -itemHeight * tailIndex, 0f);
                    }
                    else
                    {
                        headIndex--;
                        tailIndex--;
                        item.Index = headIndex;
                        item.RectTransform.anchoredPosition3D = new Vector3(0f, -itemHeight * headIndex, 0f);
                    }
                }
                item.IsInside = currentIsInside;
            }
            itemList = itemList.OrderBy(item => item.Index).ToList();
        }

        private Rect GetWorldRect(RectTransform rectTransform)
        {
            var corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            return new Rect(
                corners[0].x,
                corners[0].y,
                corners[2].x - corners[0].x,
                corners[2].y - corners[0].y
            );
        }
    }
}
