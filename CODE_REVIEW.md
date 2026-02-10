# XDebugger コードレビュー

## 概要

XDebuggerプロジェクト全体（SRDebuggerを除く）のコードレビュー結果です。
約90ファイルのC#コードをレビューし、バグ・設計上の問題・改善提案を以下にまとめます。

---

## 1. バグ・不具合 (Critical / High)

### 1.1 `GlobalMenuTabPage.SetUIFactory` のデッドコード (到達不能分岐)

**ファイル**: `Runtime/Scripts/Common/GlobalMenuTabPage.cs:63-76`

```csharp
public override void SetUIFactory(UIFactoryBase uiFactory)
{
    base.SetUIFactory(uiFactory);
    if (pageModel != null && content != null)       // ← 条件A
    {
        CreateAndDisplayPage();
    }
    else if (pageModel != null && content != null)  // ← 条件Aと同一 → 到達不能
    {
        pageModel.Close();
        CreateAndDisplayPage();
    }
}
```

`if` と `else if` の条件が完全に同一のため、`else if` ブロックには絶対に到達しません。おそらく2つ目の条件は `pageModel.Content != null`（既にページが表示済み）等の異なる条件を意図していたと思われます。

### 1.2 `NumberModel.Value` セッターが自分自身を代入

**ファイル**: `Runtime/Scripts/Models/NumberModel.cs`

```csharp
public float Value
{
    get => value;
    set => SetValue(value, false);  // ← 引数の value ではなくフィールドの value を渡している
}
```

`SetValue(value, false)` の `value` はC#のプロパティセッターのキーワード（新しく設定された値）として解釈されるべきですが、同名のフィールド `private float value` が存在するため、フィールドの `value` が優先されます。結果として `Value` プロパティへの代入が常にno-opになります。

**同じ問題**: `FloatSliderModel.Value` のセッターも同様です。

### 1.3 `FlowHorizontalLayoutGroup.SetLayoutHorizontal` に `Debug.Log` が残留

**ファイル**: `Runtime/Scripts/Common/FlowHorizontalLayoutGroup.cs:76`

```csharp
public override void SetLayoutHorizontal()
{
    var availableWidth = Mathf.Max(0f, GetCanvasSpaceScreenWidth() - padding.horizontal);
    Debug.Log(availableWidth); // ← デバッグ用ログが残っている
```

レイアウト再計算のたびに毎フレーム呼ばれる可能性があり、ログが大量に出力されてパフォーマンスに影響します。

### 1.4 `FlowHorizontalLayoutGroup.GetContentHeightWithoutPadding` の余分なspacing加算

**ファイル**: `Runtime/Scripts/Common/FlowHorizontalLayoutGroup.cs:133-142`

```csharp
private float GetContentHeightWithoutPadding(List<Line> lines)
{
    var result = 0f;
    foreach (var line in lines)
    {
        result += line.Height;
    }
    result += spaceY;  // ← 行間ではなく固定で1回だけ spaceY を加算
    return result;
}
```

行間のスペースが正しく計算されていません。`SetLayoutHorizontal`では行間に`spaceY`を加算していますが、この関数では行数に関係なく1回だけ`spaceY`を足しています。正しくは `result += spaceY * Math.Max(0, lines.Count - 1)` のようにすべきです。

### 1.5 `ProfilerPage` のFPS計算が不正確

**ファイル**: `Runtime/Scripts/Profiler/ProfilerPage.cs:85`

```csharp
fps = 1f / Time.deltaTime;
```

`Time.deltaTime`は1フレームの値なので非常にノイズが多く、FPS表示がちらつきます。一般的には移動平均やスムージングを使用します。機能上の問題ではありますが、バグではなく精度の問題です。

### 1.6 `ProfilerPage` の `GetValueWithSISuffix` で配列範囲外アクセスの可能性

**ファイル**: `Runtime/Scripts/Profiler/ProfilerPage.cs:160-169`

```csharp
private string GetValueWithSISuffix(double value)
{
    var index = 0;
    while (value > 1024)
    {
        value /= 1024d;
        index++;
    }
    value = Math.Round(value, 2);
    return $"{value}{suffixList[index]}";
}
```

`suffixList`は5要素（B, KB, MB, GB, TB）しかないため、TB超の値が渡されると`IndexOutOfRangeException`が発生します。現実的にはUnityのメモリ情報で発生しにくいですが、防御的コーディングが望ましいです。`SystemPageModel`にも同様のメソッドがあり、そちらはループ条件に`index < suffix.Length`のガードがあります。

---

## 2. 設計上の問題 (Medium)

### 2.1 `PageModel` と `ScriptablePageModel` の大量のコード重複

**ファイル**: `Runtime/Scripts/Models/PageModel.cs` / `Runtime/Scripts/Models/ScriptablePageModel.cs`

両クラスはほぼ同一のロジックを持っています:
- `Initialize`, `OpenPage`, `Close`, `Show`, `Hide`, `Refresh`, `RefreshCurrentPage`, `Clear`
- `AddLabel`, `AddButton`, `AddText`, `AddNumber`, `AddSlider`, `AddIntSlider`, `AddToggle`, `AddDropdown`, `AddEnumDropdown`

`ScriptablePageModel`は`ScriptableObject`を継承するため多重継承はできませんが、共通ロジックをインターフェースのデフォルト実装やCompositionパターンで抽出すれば、重複を大幅に削減できます。現状では片方を修正した場合にもう片方に反映し忘れるリスクがあります。

### 2.2 `PageModel.Close` がシングルトン経由で自己削除を要求

**ファイル**: `Runtime/Scripts/Models/PageModel.cs:109-141`

```csharp
public virtual void Close(Action onClose = null)
{
    if (control is PageControl pageControl)
    {
        pageControl.Close(() =>
        {
            ClosedPage();
            Clear();
            XDebugger.Instance.ClosePage(this);  // ← Close内で再びClosePage呼び出し
            onClose?.Invoke();
            ...
```

`MainMenuTabPage.ClosePage(target)` から呼ばれた `target.Close()` の中で再び `XDebugger.Instance.ClosePage(this)` を呼ぶという循環構造になっています。`MainMenuTabPage.ClosePage` は `pageStack.Remove(target)` を最初に行っているため実害は出にくいですが、ページ遷移のフローが読みにくくなっています。`ScriptablePageModel.Close` はこの呼び出しが含まれておらず、挙動の一貫性がありません。

### 2.3 `ControlModelBase` の同期的 Addressables ロード

**ファイル**: `Runtime/Scripts/Models/ControlModelBase.cs:46-51`

```csharp
protected virtual T Instantiate<T>(Transform parent) where T : ControlBase, new()
{
    var prefab = Addressables.LoadAssetAsync<GameObject>(prefabAddress).WaitForCompletion();
    ...
```

`WaitForCompletion()` はメインスレッドをブロックするため、多数のコントロールを一度に生成するとフレームスパイクが発生します。コメントで注意書きがされていますが、実際の使用箇所（`GroupModel.CreateControl`）では `Instantiate<ContentGroup>(parent)` 経由でこのメソッドが呼ばれています。一方、他の多くのコントロール（`ActionModel`, `BoolModel`等）は `UIFactory` 経由で生成しており、`Instantiate<T>` を直接呼んでいません。

`GroupModel` 系のみAddressables直接ロードを使い、他のコントロールは `UIFactory` を使うという不統一があります。

### 2.4 `IGetPageModel` インターフェースの名前空間が不適切

**ファイル**: `Runtime/Scripts/Common/Interface/IGetPageModel.cs`

```csharp
namespace Xeon.XDebugger.Editor.Model
{
    public interface IGetPageModel { ... }
}
```

ランタイムコード（`GlobalMenuTabPage`, `MainMenuTabPage`）が実装するインターフェースが `Editor.Model` 名前空間に配置されています。この名前空間はエディタ専用と認識されやすく、ビルド時にEditor名前空間のコードが除外される可能性があります（実際にはasmdefの設定次第）。Runtimeの名前空間に移動すべきです。

### 2.5 `XDebugger.Instance` の同期的Addressablesロード

**ファイル**: `Runtime/Scripts/XDebugger.cs:50`

```csharp
var prefab = Addressables.LoadAssetAsync<GameObject>(nameof(XDebugger)).WaitForCompletion();
```

シングルトンアクセス時にメインスレッドをブロックする同期ロードが実行されます。初回アクセス時に顕著なスパイクが発生する可能性があります。非同期初期化パターンの提供を推奨します。

### 2.6 `TabController` の static フィールドによるメモリリーク

**ファイル**: `Runtime/Scripts/Common/TabController.cs:34-38`

```csharp
private static LogItemBuffer logDataList = new(LogBufferCapacity);
private static int logIndex = 0;
private static event Action<int> onAddInfoLog;
private static event Action<int> onAddWarningLog;
private static event Action<int> onAddErrorLog;
```

`LogItemBuffer` とイベントハンドラが static で保持されています。`PreInitialize` で `Application.logMessageReceived` にハンドラを登録し、`Application.quitting` で解除していますが、`logDataList` 自体は Domain Reload が無効な環境（Enter Play Mode Options有効時）でリセットされません。

### 2.7 `ConsoleController.Cleanup` の `RemoveAllListeners` 使用

**ファイル**: `Runtime/Scripts/Console/ConsoleController.cs:80-83`

```csharp
infoToggle.onValueChanged.RemoveAllListeners();
warningToggle.onValueChanged.RemoveAllListeners();
errorToggle.onValueChanged.RemoveAllListeners();
clearButton.onClick.RemoveAllListeners();
```

`RemoveAllListeners` はInspectorで設定されたPersistentリスナーも含めて全て削除します。自分で追加したリスナーのみを削除すべきです。

### 2.8 `using` ディレクティブの順序問題

**ファイル**: `Runtime/Scripts/XDebugger.cs:10-13`

```csharp
using TouchPhase = UnityEngine.InputSystem.TouchPhase;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
```

`TouchPhase`のエイリアスが`#if ENABLE_INPUT_SYSTEM`の外にあるため、Input Systemが無効な環境ではコンパイルエラーになる可能性があります。

---

## 3. コード品質・保守性 (Low)

### 3.1 `XDebuggerSettingEditor` のSerializedPropertyとの不整合

**ファイル**: `Runtime/Scripts/Setting/XDebuggerSetting.cs:80-109`

カスタムエディタ内で `serializedObject.Update()` と `serializedObject.ApplyModifiedProperties()` を使用しつつ、フィールドへの直接代入（`setting.triggerMode = ...`）も混在しています。`EditorUtility.SetDirty(target)` で対応していますが、`SerializedProperty` 経由のアクセスと直接アクセスの混在はUndoの動作に問題を起こす場合があります。

### 3.2 `XDebuggerWindow` の未使用 using ディレクティブ

**ファイル**: `Editor/Scripts/XDebuggerWindow.cs`

```csharp
using System.Threading.Tasks;        // 未使用
using UnityEditor.IMGUI.Controls;    // 未使用
using NUnit.Framework;               // 未使用
using UnityEditor.TerrainTools;      // 未使用
```

不要な `using` が多数残っています。特に `NUnit.Framework` や `UnityEditor.TerrainTools` はこのクラスと無関係です。

### 3.3 `XDebuggerWindow` でページスタックの戻る操作が未実装

**ファイル**: `Editor/Scripts/XDebuggerWindow.cs`

`pageStack` フィールドが定義され、`DrawPageLinkButton` でスタックにpushしていますが、戻る操作のUIが存在しません。スタックにページを追加する一方で、戻る手段がないためメモリが増え続けます。

### 3.4 `IStyle` の名前空間が他と異なる

**ファイル**: `Runtime/Scripts/Interface/IStyle.cs` → `namespace Xeon.Style`
**ファイル**: `Runtime/Scripts/Style/*.cs` → `namespace Xeon.Style`

XDebuggerの他のコードは `Xeon.XDebugger.*` 名前空間を使用していますが、Style系のみ `Xeon.Style` です。パッケージの外部から参照する意図がなければ `Xeon.XDebugger.Style` に統一すべきです。

### 3.5 `FlyweightScrollViewport` のnamespace不一致

**ファイル**: `FlyweightScrollView/Runtime/Scripts/FlyweightScrollViewport.cs`

```csharp
namespace Xeon.Common
```

他のFlyweightScrollView系は `Xeon.Common.FlyweightScrollView` ですが、このクラスだけ `Xeon.Common` です。

### 3.6 `ControlModelBase` に未使用の `isActive` フィールドが存在

**ファイル**: `Runtime/Scripts/Models/ControlModelBase.cs:13`

```csharp
protected bool isActive = true;
```

このフィールドはどこからも参照されていません。

### 3.7 `ControlModelBase` に未使用の `prefabAddress` プロパティ

**ファイル**: `Runtime/Scripts/Models/ControlModelBase.cs:11`

各モデル（`ActionModel`, `BoolModel`等）で `prefabAddress` が定義されていますが、実際のコントロール生成は `UIFactory` 経由で行われるため、`Instantiate<T>` を経由しない大半のモデルでは `prefabAddress` が使われていません。`GroupModel` 系でのみ使用されます。

### 3.8 `BoolModel` のコンストラクタで `parent` パラメータが未使用

**ファイル**: `Runtime/Scripts/Models/BoolModel.cs`

```csharp
public BoolModel(string title, bool isOn, Action<bool> onChangedValue, IGroupModel parent, int priority = 0)
    : base(title, priority)  // ← parentがbase()に渡されていない
```

`IGroupModel parent` を受け取るコンストラクタで、基底クラスの `ControlModelBase(string title, IGroupModel parent, int priority)` を呼んでおらず、`parent` が設定されません。`FloatSliderModel` にも同様の問題があります。

### 3.9 `IntSliderControl.Refresh` で min/max が設定されていない

**ファイル**: `Runtime/Scripts/Controls/IntSliderControl.cs`

```csharp
public override void Refresh()
{
    slider.SetValueWithoutNotify(model.Value);
    input.SetTextWithoutNotify(model.Value.ToString());
}
```

`FloatSliderControl.Refresh` では `slider.minValue` / `slider.maxValue` を設定していますが、`IntSliderControl.Refresh` ではそれが抜けています。動的にmin/maxが変更された場合、スライダーの範囲が更新されません。

### 3.10 `StringControl.Refresh` でリスナーの登録/解除を繰り返す

**ファイル**: `Runtime/Scripts/Controls/StringControl.cs`

```csharp
public override void Refresh()
{
    inputField.SetTextWithoutNotify(model.Text);
    inputField.onEndEdit.RemoveListener(OnEndEdit);
    inputField.onEndEdit.AddListener(OnEndEdit);
}
```

`Refresh` が呼ばれるたびにリスナーの Remove/Add を行っています。リスナーの登録は `Setup` で一度行えば十分です。

---

## 4. サブモジュール・依存関係の問題

### 4.1 XGraphs サブモジュール未初期化

`Assets/XDebugger/XGraphs/` はgitサブモジュール（`git@github.com:AraiYuhki/XGraphs.git`）ですが、現在初期化されていません。`ProfilerPage.cs` が参照する以下の型が不在です:
- `StackedBarChart`
- `MultiValueSeries`
- `Series`
- `ScaleMarkerData`
- `DoubleCircularBuffer`
- `CircularBuffer<float>`（`Xeon.XGraph.Model`名前空間のもの）

サブモジュールが初期化されないとProfiler機能がコンパイルできません。

---

## 5. 全体的な評価

### 良い点

- **明確なアーキテクチャ**: Model-View分離、Factory/Builder/Flyweightパターンの適切な活用
- **拡張性**: `PageModel`のFluent API（`AddLabel`, `AddButton`, `HorizontalScope` 等）は直感的で使いやすい
- **クロスプラットフォーム対応**: Input System / Legacy Input Manager の両対応、モバイル/コンソール/PCの各プラットフォーム考慮
- **FlyweightScrollView**: 仮想スクロールの実装が効率的で、`CircularBuffer` / `LinkedList` を活用した再配置ロジックは適切
- **リスナー管理**: 多くのControlクラスで `OnDestroy` 時にリスナーを確実に解除している
- **UIFactory**: 依存性注入パターンによりカスタムUI実装が容易

### 改善推奨の優先順位

| 優先度 | 項目 | 影響 |
|--------|------|------|
| **High** | 1.1 GlobalMenuTabPage デッドコード | 意図した再生成処理が動作しない |
| **High** | 1.2 NumberModel/FloatSliderModel のValue setter | プロパティ経由の値設定が機能しない |
| **High** | 1.3 Debug.Log残留 | パフォーマンス劣化 |
| **Medium** | 2.1 PageModel/ScriptablePageModel 重複 | 保守性低下、修正漏れリスク |
| **Medium** | 2.2 Close()の循環呼び出し | ページ遷移フロー理解困難 |
| **Medium** | 2.4 IGetPageModel 名前空間 | ビルド時に問題発生の可能性 |
| **Medium** | 2.8 TouchPhaseの条件付きコンパイル不足 | Input System無効時コンパイルエラー |
| **Low** | 3.2 未使用using | コード整理 |
| **Low** | 3.4/3.5 名前空間の不統一 | 一貫性 |
| **Low** | 3.8 コンストラクタのparent未使用 | 潜在バグ |
