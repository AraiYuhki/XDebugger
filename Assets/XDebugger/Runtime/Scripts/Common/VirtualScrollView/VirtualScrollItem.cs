using UnityEngine;

namespace Xeon.Common
{
    public class VirtualScrollItem<T> : VirtualScrollViewItemBase
        where T : MonoBehaviour
    {
        public T Value { get; }

        public VirtualScrollItem(T value, RectTransform viewPort, HorizontalAlignment horizontalAlignment)
            : base(viewPort)
        {
            Value = value;

            RectTransform = value.GetComponent<RectTransform>();
            gameObject = value.gameObject;

            SetHorizontalAlignment(horizontalAlignment);
            UpdateIsInside();
        }
    }
}
