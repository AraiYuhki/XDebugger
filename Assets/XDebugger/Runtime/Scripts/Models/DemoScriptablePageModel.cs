using System;
using UnityEngine;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Model
{
    /// <summary>
    /// ScriptablePageModelの動作確認用実装クラス。
    /// 一通りのコントロール機能を備えています。
    /// </summary>
    [CreateAssetMenu(fileName = "DemoScriptablePage", menuName = "XDebugger/Demo Scriptable Page")]
    public class DemoScriptablePageModel : ScriptablePageModel
    {
        [SerializeField]
        private bool autoInitialize = true;

        public override void Initialize(IUIFactory uiFactory)
        {
            base.Initialize(uiFactory);

            if (!autoInitialize)
                return;

            // デモ用のコンテンツを生成
            CreateDemoContent();
        }

        /// <summary>
        /// デモ用のコンテンツを生成
        /// </summary>
        private void CreateDemoContent()
        {
            // タイトルラベル
            AddLabel("===== Demo Scriptable Page =====", 0);

            // 説明用ラベル
            AddLabel("ScriptablePageModelの動作確認用ページです", 1);

            // セパレータ
            AddLabel(string.Empty, 2);

            // ボタンセクション
            AddLabel("--- Buttons ---", 3);
            AddButton("Button 1", () => Debug.Log("Button 1 clicked"), 4);
            AddButton("Button 2", () => Debug.Log("Button 2 clicked"), 5);

            // セパレータ
            AddLabel(string.Empty, 6);

            // テキスト入力セクション
            AddLabel("--- Text Input ---", 7);
            AddText("Username", "Player", value => Debug.Log($"Username: {value}"), 8);

            // セパレータ
            AddLabel(string.Empty, 9);

            // トグルセクション
            AddLabel("--- Toggles ---", 10);
            AddToggle("Enable Feature A", false, value => Debug.Log($"Feature A: {value}"), 11);
            AddToggle("Enable Feature B", true, value => Debug.Log($"Feature B: {value}"), 12);

            // セパレータ
            AddLabel(string.Empty, 13);

            // スライダーセクション
            AddLabel("--- Sliders ---", 14);
            AddIntSlider("Volume", 50, 0, 100, value => Debug.Log($"Volume: {value}"), 15);
            AddSlider("Brightness", 0.5f, 0f, 1f, 2, value => Debug.Log($"Brightness: {value:F2}"), 16);

            // セパレータ
            AddLabel(string.Empty, 17);

            // ドロップダウンセクション
            AddLabel("--- Dropdown ---", 18);
            AddDropdown(
                "Difficulty",
                0,
                new[] { "Easy", "Normal", "Hard" },
                new[] { 0, 1, 2 },
                value => Debug.Log($"Difficulty: {value}"),
                19
            );

            // セパレータ
            AddLabel(string.Empty, 20);

            // Enum ドロップダウンセクション
            AddLabel("--- Enum Dropdown ---", 21);
            AddEnumDropdown("Color", TestColor.Red, value => Debug.Log($"Color: {value}"), 22);

            // セパレータ
            AddLabel(string.Empty, 23);

            // グループセクション
            AddLabel("--- Group Demo ---", 24);

            // 水平グループはここでは省略（GroupLayoutScopeが使用できないため）
            AddLabel("(グループ機能はPageModelで実装されています)", 25);

            // セパレータ
            AddLabel(string.Empty, 26);

            // アクション
            AddButton("Log All Values", LogAllValues, 27);
            AddButton("Refresh", () => Refresh(true), 28);
        }

        /// <summary>
        /// すべての値をログに出力
        /// </summary>
        private void LogAllValues()
        {
            Debug.Log("=== Demo Page Values ===");
            Debug.Log($"Title: {Title}");
            Debug.Log($"Model Count: {Count}");
            Debug.Log("========================");
        }
    }

    // テスト用Enum
    public enum TestColor
    {
        Red,
        Green,
        Blue
    }
}
