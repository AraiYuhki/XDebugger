using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Xeon.XDebugger.Model;

namespace Xeon.XDebugger.Pin
{
    /// <summary>
    /// ピン止めされたグループ内の子に表示する、「この子だけピン止めから外す」ボタン。
    /// ピン止めモード中のみ表示される。
    /// </summary>
    public sealed class PinExcludeButton : MonoBehaviour
    {
        private ControlModelBase model;
        private PinContext context;
        private Image background;
        private Button button;
        private TMP_Text label;

        internal static PinExcludeButton Create(Transform parent, ControlModelBase model, PinContext context)
        {
            var (rect, image, button, label) = PinUI.CreateButton(
                parent, "PinExcludeButton", "×", new Vector2(1, 1), new Vector2(56, 22), new Vector2(-4, -4));
            image.color = PinUI.ActiveColor;
            var self = rect.gameObject.AddComponent<PinExcludeButton>();
            self.model = model;
            self.context = context;
            self.background = image;
            self.button = button;
            self.label = label;

            button.onClick.RemoveListener(self.OnClick);
            button.onClick.AddListener(self.OnClick);
            PinManager.PinModeChanged += self.Refresh;
            self.Refresh();
            return self;
        }

        private void OnClick() => context.Exclude(model);

        private void Refresh()
        {
            var visible = PinManager.PinMode;
            background.enabled = visible;
            label.enabled = visible;
            button.enabled = visible;
        }

        private void OnDestroy()
        {
            PinManager.PinModeChanged -= Refresh;
            if (button != null)
                button.onClick.RemoveListener(OnClick);
        }
    }
}
