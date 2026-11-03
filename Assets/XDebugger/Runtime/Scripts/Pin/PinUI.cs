using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Xeon.XDebugger.Pin
{
    /// <summary>ピン関連UIをコードで生成するためのヘルパー</summary>
    internal static class PinUI
    {
        public static readonly Color NormalColor = new(0.10f, 0.45f, 0.85f, 0.9f);
        public static readonly Color ActiveColor = new(0.85f, 0.30f, 0.20f, 0.9f);

        public static (RectTransform rect, Image image, Button button, TMP_Text label) CreateButton(
            Transform parent, string name, string text, Vector2 anchor, Vector2 size, Vector2 position)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.layer = parent.gameObject.layer;
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            var image = go.GetComponent<Image>();
            image.color = NormalColor;
            go.GetComponent<Button>().targetGraphic = image;
            // 親のレイアウトグループの影響を受けない
            go.GetComponent<LayoutElement>().ignoreLayout = true;

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelGo.layer = go.layer;
            var labelRect = (RectTransform)labelGo.transform;
            labelRect.SetParent(rect, false);
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;

            var label = labelGo.GetComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = 14;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            label.raycastTarget = false;
            label.enableAutoSizing = true;
            label.fontSizeMin = 8;
            label.fontSizeMax = 14;

            return (rect, image, go.GetComponent<Button>(), label);
        }
    }
}
