using UnityEngine;

namespace Xeon.Common
{
    [RequireComponent(typeof(RectTransform))]
    public class VirtualScrollItem : MonoBehaviour
    {
        public RectTransform RectTransform { get; private set; }
        public int Index { get; set; }
        public bool IsInside { get; private set; }

        private Rect viewPortRect;

        private void Awake()
        {
            RectTransform ??= GetComponent<RectTransform>();
        }

        public void Initialize(RectTransform viewPort, int index)
        {
            RectTransform ??= GetComponent<RectTransform>();
            viewPortRect = GetWorldRect(viewPort);

            var worldRect = GetWorldRect(RectTransform);
            IsInside = viewPortRect.Overlaps(worldRect);

            Index = index;
            RectTransform.anchorMin = Vector2.up;
            RectTransform.anchorMax = Vector2.up;
            RectTransform.pivot = Vector2.up;
        }

        public void UpdateIsInside()
        {
            var worldRect = GetWorldRect(RectTransform);
            IsInside = viewPortRect.Overlaps(worldRect);
        }

        private static Rect GetWorldRect(RectTransform rectTransform)
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
