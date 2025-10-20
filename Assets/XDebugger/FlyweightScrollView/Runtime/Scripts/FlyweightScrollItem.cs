using UnityEngine;

namespace Xeon.Common.FlyweightScrollView
{
    public class FlyweightScrollItem<T> : FlyweightScrollViewItemBase
        where T : MonoBehaviour
    {
        public T Value { get; }

        public FlyweightScrollItem(T value, RectTransform viewPort, HorizontalAlignment horizontalAlignment)
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
