# Changelog

このプロジェクトに対するすべての重要な変更はこのファイルに記録されます。

フォーマットは [Keep a Changelog](https://keepachangelog.com/ja/1.1.0/) に基づいています。

## [Unreleased]

### Fixed (修正)
- `NumberModel.Value` / `FloatSliderModel.Value`: プロパティセッターでバッキングフィールドとC#キーワードの名前衝突を修正（`_value`にリネーム）
- `FloatSliderModel`: コンストラクタで`parent`パラメータが基底クラスに渡されていない問題を修正
- `BoolModel`: コンストラクタで`parent`パラメータが基底クラスに渡されていない問題を修正
- `GlobalMenuTabPage.SetUIFactory`: `if`と`else if`の条件が同一でデッドコードになっていた問題を修正
- `FlowHorizontalLayoutGroup`: `SetLayoutHorizontal`に残留していた`Debug.Log`を削除
- `FlowHorizontalLayoutGroup.GetContentHeightWithoutPadding`: 行間スペースの計算が不正確だった問題を修正
- `XDebugger.cs`: `TouchPhase`エイリアスが`#if ENABLE_INPUT_SYSTEM`の外にあり、Input System無効時にコンパイルエラーになる問題を修正
- `PageModel.Close`: `Close()`内で`XDebugger.Instance.ClosePage(this)`を呼ぶ循環的な呼び出しを削除
- `ProfilerPage.GetValueWithSISuffix`: `suffixList`の配列範囲外アクセスの可能性を修正
- `ConsoleController.Cleanup`: `RemoveAllListeners()`がInspector設定のPersistentリスナーも削除する問題を、個別の`RemoveListener`に変更
- `IntSliderControl.Refresh`: `min`/`max`の設定が欠落していた問題を修正（`FloatSliderControl`との一貫性）
- `StringControl.Refresh`: リスナーの登録/解除を毎回繰り返していた問題を修正（`Setup`に移動）

### Changed (変更)
- `IGetPageModel`: 名前空間を`Xeon.XDebugger.Editor.Model`から`Xeon.XDebugger.Common`に変更（ランタイムインターフェースに適切な配置）
- `ControlModelBase`: 未使用の`isActive`フィールドを削除

### Removed (削除)
- `XDebuggerWindow`: 未使用の`using`ディレクティブを削除（`System.Threading.Tasks`, `UnityEditor.IMGUI.Controls`, `NUnit.Framework`, `UnityEditor.TerrainTools`）
