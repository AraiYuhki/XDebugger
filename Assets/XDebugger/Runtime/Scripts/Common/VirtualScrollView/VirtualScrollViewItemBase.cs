using UnityEngine;

namespace Xeon.Common
{
    public abstract class VirtualScrollViewItemBase
    {
        public GameObject gameObject { get; protected set; }
        public RectTransform RectTransform { get; protected set; }
        public bool IsInside { get; private set; }

        private readonly RectTransform viewPort;

        public VirtualScrollViewItemBase(RectTransform viewPort)
        {
            this.viewPort = viewPort;
        }

        protected virtual void InitializeRectTransform()
        {
            RectTransform.anchorMin = Vector2.up;
            RectTransform.anchorMax = Vector2.up;
            RectTransform.pivot = Vector2.up;
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
