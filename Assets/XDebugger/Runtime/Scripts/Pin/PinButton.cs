using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Xeon.XDebugger.Model;

namespace Xeon.XDebugger.Pin
{
    /// <summary>
    /// コントロール右上に重ねて表示するピン/解除ボタン。
    /// ピン止めモード中のみ表示される。
    /// </summary>
    public sealed class PinButton : MonoBehaviour
    {
        private ControlModelBase model;
        private Image background;
        private Button button;
        private TMP_Text label;
        private string pinnedLabel;

        internal static PinButton Create(Transform parent, ControlModelBase model, string pinnedLabel)
        {
            var (rect, image, button, label) = PinUI.CreateButton(
                parent, "PinButton", "PIN", new Vector2(1, 1), new Vector2(56, 22), new Vector2(-4, -4));
            var self = rect.gameObject.AddComponent<PinButton>();
            self.model = model;
            self.background = image;
            self.button = button;
            self.label = label;
            self.pinnedLabel = pinnedLabel;

            button.onClick.RemoveListener(self.OnClick);
            button.onClick.AddListener(self.OnClick);
            PinManager.PinModeChanged += self.Refresh;
            PinManager.PinChanged += self.OnPinChanged;
            self.Refresh();
            return self;
        }

        private void OnClick() => PinManager.TogglePin(model);

        private void OnPinChanged(ControlModelBase changed, bool _)
        {
            if (changed == model)
                Refresh();
        }

        private void Refresh()
        {
            var visible = PinManager.PinMode;
            var isPinned = PinManager.IsPinned(model);
            background.enabled = visible;
            label.enabled = visible;
            button.enabled = visible;
            label.text = isPinned ? pinnedLabel : "PIN";
            background.color = isPinned ? PinUI.ActiveColor : PinUI.NormalColor;
        }

        private void OnDestroy()
        {
            PinManager.PinModeChanged -= Refresh;
            PinManager.PinChanged -= OnPinChanged;
            if (button != null)
                button.onClick.RemoveListener(OnClick);
        }
    }
}
