using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
namespace Xeon.Common.Debug
{
    public class DebugVirtualTestItem : MonoBehaviour, ISetupable<int>
    {
        [SerializeField]
        private Image background;
        [SerializeField]
        private TMP_Text text;

        public void Setup(int value)
        {
            text.text = value.ToString();
        }

        [UnityEditor.MenuItem("GameObject/Xeon/Debug Virtual Item")]
        public static void Create()
        {
            var parent = UnityEditor.Selection.activeTransform;
            Create(parent);
        }
        public static DebugVirtualTestItem Create(Transform parent)
        {
            var item = CreateBackground();
            item.transform.SetParent(parent);
            item.transform.localPosition = Vector3.zero;
            item.background = item.GetComponent<Image>();
            item.text = CreateLabel(item.GetComponent<RectTransform>());
            return item;
        }

        private static DebugVirtualTestItem CreateBackground()
        {
            var gameObject = new GameObject("Debug Item", typeof(DebugVirtualTestItem), typeof(Image));
            return gameObject.GetComponent<DebugVirtualTestItem>();
        }

        private static TMP_Text CreateLabel(RectTransform parent)
        {
            var gameObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            gameObject.transform.SetParent(parent);

            var rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            var text = gameObject.GetComponent<TMP_Text>();
            text.verticalAlignment = VerticalAlignmentOptions.Middle;
            text.horizontalAlignment = HorizontalAlignmentOptions.Center;
            text.enableAutoSizing = true;
            text.fontSizeMin = 18;
            text.fontSizeMax = 24;
            text.text = "Debug Item";
            text.color = Color.black;
            return text;
        }
    }
}
#endif
