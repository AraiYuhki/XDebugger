using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Xeon.XDebugger.Common;
using Xeon.XDebugger.Model;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Pin
{
    /// <summary>
    /// ピン止めされたコントロールを、画面上の指定位置を起点に並べて表示するオーバーレイ。
    /// デバッグメニューとは独立した専用Canvasを使うため、メニューを閉じていても表示される。
    /// </summary>
    public sealed class PinnedPanel : MonoBehaviour
    {
        private readonly Dictionary<ControlModelBase, GameObject> items = new();

        private IUIFactory uiFactory;
        private CanvasGroup canvasGroup;
        private RectTransform container;
        private RectTransform itemsRoot;
        private PinnedToggleButton toggleButton;
        private bool showToggleButton;

        public static PinnedPanel Create(Transform parent, XDebuggerSetting setting)
        {
            var root = new GameObject("PinnedCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),
                typeof(GraphicRaycaster), typeof(CanvasGroup), typeof(PinnedPanel));
            root.transform.SetParent(parent, false);
            root.layer = LayerMask.NameToLayer("UI");

            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            var panel = root.GetComponent<PinnedPanel>();
            panel.canvasGroup = root.GetComponent<CanvasGroup>();
            panel.canvasGroup.alpha = setting.PinnedAlpha;
            panel.showToggleButton = setting.ShowPinnedToggleButton;
            panel.uiFactory = setting.UIFactory;
            panel.Build(setting);
            return panel;
        }

        public void SetUIFactory(IUIFactory factory) => uiFactory = factory;

        private void Build(XDebuggerSetting setting)
        {
            var anchor = AnchorOf(setting.PinPosition);
            var isBottom = anchor.y < 0.5f;

            var containerGo = new GameObject("Container", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            containerGo.layer = gameObject.layer;
            container = (RectTransform)containerGo.transform;
            container.SetParent(transform, false);
            container.anchorMin = container.anchorMax = container.pivot = anchor;
            container.sizeDelta = new Vector2(Mathf.Max(100f, setting.PinnedWidth), 0f);
            container.anchoredPosition = Vector2.zero;

            ConfigureLayout(containerGo.GetComponent<VerticalLayoutGroup>(), 4f, 8);
            containerGo.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var itemsGo = new GameObject("Items", typeof(RectTransform), typeof(VerticalLayoutGroup));
            itemsGo.layer = gameObject.layer;
            itemsRoot = (RectTransform)itemsGo.transform;
            itemsRoot.SetParent(container, false);
            ConfigureLayout(itemsGo.GetComponent<VerticalLayoutGroup>(), 4f, 0);

            toggleButton = PinnedToggleButton.Create(container);
            // 下端起点の場合はトグルボタンを画面端側（最下部）に置く
            toggleButton.transform.SetSiblingIndex(isBottom ? 1 : 0);

            PinManager.PinChanged += OnPinChanged;
            PinManager.PinModeChanged += OnPinModeChanged;
            PinManager.PinnedVisibleChanged += RefreshVisibility;
            RefreshVisibility();
        }

        private static void ConfigureLayout(VerticalLayoutGroup layout, float spacing, int padding)
        {
            layout.spacing = spacing;
            layout.padding = new RectOffset(padding, padding, padding, padding);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
        }

        private static Vector2 AnchorOf(TriggerPosition position) => position switch
        {
            TriggerPosition.TopLeft => new Vector2(0, 1),
            TriggerPosition.TopCenter => new Vector2(0.5f, 1),
            TriggerPosition.TopRight => new Vector2(1, 1),
            TriggerPosition.MiddleLeft => new Vector2(0, 0.5f),
            TriggerPosition.MiddleRight => new Vector2(1, 0.5f),
            TriggerPosition.BottomLeft => new Vector2(0, 0),
            TriggerPosition.BottomCenter => new Vector2(0.5f, 0),
            _ => new Vector2(1, 0),
        };

        private void OnPinChanged(ControlModelBase model, bool isPinned)
        {
            if (isPinned)
                AddItem(model);
            else
                RemoveItem(model);
            RefreshVisibility();
        }

        private void AddItem(ControlModelBase model)
        {
            if (items.ContainsKey(model) || uiFactory == null)
                return;

            var itemGo = new GameObject("PinnedItem", typeof(RectTransform), typeof(VerticalLayoutGroup));
            itemGo.layer = gameObject.layer;
            itemGo.transform.SetParent(itemsRoot, false);
            ConfigureLayout(itemGo.GetComponent<VerticalLayoutGroup>(), 0f, 0);

            // ピン用コントロールにはメニュー用のピンボタンを付けない
            PinManager.SuppressAttach = true;
            try
            {
                model.CreateControl(itemGo.transform, uiFactory);
            }
            finally
            {
                PinManager.SuppressAttach = false;
            }
            // 解除用ボタン（ピン止めモード中のみ表示）
            PinButton.Create(itemGo.transform, model, "×");
            items.Add(model, itemGo);
        }

        private void RemoveItem(ControlModelBase model)
        {
            if (!items.Remove(model, out var go))
                return;
            go.transform.SetParent(null, false); // レイアウトを即時更新するため先に切り離す
            Destroy(go);
        }

        private void OnPinModeChanged() => RefreshVisibility();

        private void RefreshVisibility()
        {
            var hasPins = items.Count > 0;
            // 全体の表示非表示。トグルボタンは非表示中も残し、復帰できるようにする
            container.gameObject.SetActive(hasPins);
            itemsRoot.gameObject.SetActive(PinManager.PinnedVisible);
            toggleButton.gameObject.SetActive(showToggleButton);
            toggleButton.Refresh();
            LayoutRebuilder.MarkLayoutForRebuild(container);
        }

        private void OnDestroy()
        {
            PinManager.PinChanged -= OnPinChanged;
            PinManager.PinModeChanged -= OnPinModeChanged;
            PinManager.PinnedVisibleChanged -= RefreshVisibility;
        }
    }

    /// <summary>ピン止めUI全体の表示/非表示を切り替える小ボタン</summary>
    internal sealed class PinnedToggleButton : MonoBehaviour
    {
        private UnityEngine.UI.Image background;
        private TMPro.TMP_Text label;
        private Button button;

        public static PinnedToggleButton Create(Transform parent)
        {
            var (rect, image, button, label) = PinUI.CreateButton(
                parent, "PinnedToggle", "PIN", new Vector2(0.5f, 0.5f), new Vector2(0, 26), Vector2.zero);
            // コンテナ内ではレイアウトに従わせる
            rect.GetComponent<LayoutElement>().ignoreLayout = false;
            rect.GetComponent<LayoutElement>().preferredHeight = 26;

            var self = rect.gameObject.AddComponent<PinnedToggleButton>();
            self.background = image;
            self.label = label;
            self.button = button;
            button.onClick.RemoveListener(self.OnClick);
            button.onClick.AddListener(self.OnClick);
            return self;
        }

        private void OnClick() => PinManager.SetPinnedVisible(!PinManager.PinnedVisible);

        public void Refresh()
        {
            label.text = PinManager.PinnedVisible ? "PINNED: ON" : "PINNED: OFF";
            background.color = PinManager.PinnedVisible ? PinUI.NormalColor : PinUI.ActiveColor;
        }

        private void OnDestroy()
        {
            if (button != null)
                button.onClick.RemoveListener(OnClick);
        }
    }
}
