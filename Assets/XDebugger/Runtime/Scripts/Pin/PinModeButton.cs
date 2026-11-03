using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Xeon.XDebugger.Pin
{
    /// <summary>メニューのヘッダーに表示する、ピン止めモード切り替えボタン</summary>
    public sealed class PinModeButton : MonoBehaviour
    {
        private Image background;
        private Button button;
        private TMP_Text label;

        public static PinModeButton Create(Transform header, Vector2 position)
        {
            var (rect, image, button, label) = PinUI.CreateButton(
                header, "PinModeButton", "PIN: OFF", new Vector2(1, 0.5f), new Vector2(80, 36), position);
            var self = rect.gameObject.AddComponent<PinModeButton>();
            self.background = image;
            self.button = button;
            self.label = label;

            button.onClick.RemoveListener(self.OnClick);
            button.onClick.AddListener(self.OnClick);
            PinManager.PinModeChanged += self.Refresh;
            self.Refresh();
            return self;
        }

        private void OnClick() => PinManager.SetPinMode(!PinManager.PinMode);

        private void Refresh()
        {
            label.text = PinManager.PinMode ? "PIN: ON" : "PIN: OFF";
            background.color = PinManager.PinMode ? PinUI.ActiveColor : PinUI.NormalColor;
        }

        private void OnDestroy()
        {
            PinManager.PinModeChanged -= Refresh;
            if (button != null)
                button.onClick.RemoveListener(OnClick);
        }
    }
}
