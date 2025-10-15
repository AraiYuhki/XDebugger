using System.ComponentModel;
using UnityEngine;

namespace Xeon.Common
{
    public abstract class FlyweightScrollViewItemBase
    {
        public GameObject gameObject { get; protected set; }
        public RectTransform RectTransform { get; protected set; }
        public bool IsInside { get; private set; }

        private readonly RectTransform viewPort;

        public FlyweightScrollViewItemBase(RectTransform viewPort)
        {
            this.viewPort = viewPort;
        }

        public virtual void SetHorizontalAlignment(HorizontalAlignment alignment)
        {
            var vector = alignment switch
            {
                HorizontalAlignment.Left => Vector2.up,
                HorizontalAlignment.Center => new Vector2(0.5f, 1f),
                HorizontalAlignment.Right => Vector2.one,
                _ => throw new InvalidEnumArgumentException()
            };
            RectTransform.anchorMin = vector;
            RectTransform.anchorMax = vector;
            RectTransform.pivot = vector;
            var position = RectTransform.anchoredPosition3D;
            position.x = 0f;
            RectTransform.anchoredPosition3D = position;
        }

        public virtual void SetVerticalAlignment(VerticalAlignment alignment)
        {
            var vector = alignment switch
            {
                VerticalAlignment.Top => new Vector2(0f, 1f),
                VerticalAlignment.Middle => new Vector2(0f, 0.5f),
                VerticalAlignment.Bottom => new Vector2(0f, 0f),
                _ => throw new InvalidEnumArgumentException()
            };
            RectTransform.anchorMin = vector;
            RectTransform.anchorMax = vector;
            RectTransform.pivot = vector;
            var position = RectTransform.anchoredPosition3D;
            position.y = 0f;
            RectTransform.anchoredPosition3D = position;
        }

        public void SetFittingItemWith(float contentWidth)
        {
            var size = RectTransform.sizeDelta;
            size.x = contentWidth;
            RectTransform.sizeDelta = size;
        }

        public void SetFittingItemHeight(float contentHeight)
        {
            var size = RectTransform.sizeDelta;
            size.y = contentHeight;
            RectTransform.sizeDelta = size;
        }

        public void UpdateIsInside()
        {
            var worldRect = GetWorldRect(RectTransform);
            var viewPortRect = GetWorldRect(viewPort);
            IsInside = viewPortRect.Overlaps(worldRect);
        }

        public void SetPosition(Vector3 position)
        {
            RectTransform.anchoredPosition3D = position;
        }

        protected static Rect GetWorldRect(RectTransform rectTransform)
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
