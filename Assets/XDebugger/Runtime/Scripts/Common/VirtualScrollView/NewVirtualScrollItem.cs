using System;
using UnityEngine;

namespace Xeon.Common
{
    public class NewVirtualScrollItem<T> : VirtualScrollViewItemBase
        where T : MonoBehaviour
    {
        public T Value { get; }

        public NewVirtualScrollItem(T value, RectTransform viewPort, int index)
            : base(viewPort, index)
        {
            Value = value;

            RectTransform = value.GetComponent<RectTransform>();
            gameObject = value.gameObject;

            InitializeRectTransform();
            UpdateIsInside();
        }
    }
}
