using UnityEngine;

public class NewVirtualScrollItem<T> where T : MonoBehaviour
{
    public T Value { get; }
    public RectTransform RectTransform { get; }
    public int Index { get; set; }
    public bool IsInside { get; private set; }
    private readonly Rect viewPortRect;

    public NewVirtualScrollItem(T value, RectTransform viewPort, int index)
    {
        Value = value;
        viewPortRect = GetWorldRect(viewPort);

        RectTransform = value.GetComponent<RectTransform>();
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
