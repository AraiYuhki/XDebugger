using UnityEngine;

namespace Xeon.Common
{
    public class FlyweightlScrollItem<T> : FlyweightScrollViewItemBase
        where T : MonoBehaviour
    {
        public T Value { get; }

        public FlyweightlScrollItem(T value, RectTransform viewPort, HorizontalAlignment horizontalAlignment)
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
