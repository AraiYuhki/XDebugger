# XDebugger

Unity向け汎用デバッグ機能パッケージ。ランタイムデバッグUI（コンソール、プロファイラー、システム情報等）をAddressablesベースのシングルトンとして提供する。

## アーキテクチャ

**Model-View-Factory パターン**を採用。ModelがデータとChangedイベントを持ち、ControlBase派生のViewがUIを描画し、UIFactoryがプレハブからインスタンスを生成する。

- **ControlModelBase**: 全コントロールModelの基底。`Changed`イベントで状態変更を通知。`CreateControl()`でViewを生成
- **ControlBase**: 全コントロールViewの基底。`Refresh()`でModelの状態をUIへ反映。`ApplyStyle()`でスタイル適用
- **UIFactoryBase**: ScriptableObjectベースのファクトリー。型→プレハブのマッピングを保持し`CreateControl<T>()`で生成
- **PageModel**: ページ単位のコンテナ。ビルダーAPIで子コントロールを宣言的に構築
- **GroupModel**: ControlModelBaseを継承しIGroupModelを実装。子コントロールをレイアウトグループとしてまとめる

**ページライフサイクル**: `Initialize()` → `OpenPage()` → `CreateControl()` → `OpenedPage()` → ... → `Close()` → `ClosedPage()` → `Clear()`

**PageModel ビルダーAPI**: AddLabel/AddButton/AddToggle等のメソッドチェーンでコントロールを追加。`HorizontalScope()`等のusingスコープでグループをネストできる。

## 名前空間

| 名前空間 | 用途 |
|---|---|
| `Xeon.XDebugger` | メインクラス（XDebugger, TriggerMode等） |
| `Xeon.XDebugger.Model` | PageModel, ControlModelBase, GroupModel等のデータモデル |
| `Xeon.XDebugger.Control` | ControlBase, PageControl, 各種コントロールView |
| `Xeon.XDebugger.UI` | IUIFactory, UIFactoryBase, DefaultUIFactory |
| `Xeon.XDebugger.Common` | TabController, IGetPageModel, CommonDialog周辺 |
| `Xeon.XDebugger.Console` | ConsolePage, ConsoleController, ILogDataBuffer |
| `Xeon.XDebugger.Profiler` | ProfilerPage, IProfilerPage |
| `Xeon.XDebugger.Dialog` | CommonDialog（UniTaskベースの非同期ダイアログ） |
| `Xeon.Style` | IStyle, BgColor, PreferredHeight, PreferredWidth等 |
| `Xeon.Common.FlyweightScrollView` | 仮想スクロールビュー（フライウェイトパターン） |
| `Xeon.XDebugger.Editor` | XDebuggerWindow等のエディタ拡張 |

## 依存関係

- **UniTask** (`Cysharp.Threading.Tasks`): CommonDialog等の非同期処理
- **Addressables**: プレハブとXDebuggerシングルトンの動的ロード
- **TextMeshPro**: テキスト表示全般
- **Input System** (任意): `#if ENABLE_INPUT_SYSTEM` で有効化。Legacy Input Managerとの共存可
- **XGraphs** (gitサブモジュール): グラフ描画機能

## サブモジュール

`Assets/XDebugger/XGraphs/` はgitサブモジュール。

```
url = git@github.com:AraiYuhki/XGraphs.git
```

初回クローン後: `git submodule update --init --recursive`

## 使用方法

### 基本操作

```csharp
XDebugger.Instance.Show();    // デバッグメニュー表示
XDebugger.Instance.Hide();    // 非表示
XDebugger.Instance.OpenPage<MyPage>(); // ページ遷移
XDebugger.Instance.ClosePage(model);   // ページを閉じる
XDebugger.SetInitialPage(new MyPageModel()); // 初期ページ設定
```

### PageModel ビルダーパターン

```csharp
public class MyPageModel : PageModel
{
    public MyPageModel() : base("My Page") { }
    protected override void InitializeInternal()
    {
        AddLabel("Status");
        AddButton("Execute", () => Debug.Log("clicked"));
        AddToggle("Feature", false, v => Debug.Log(v));
        AddSlider("Volume", 0.5f, 0f, 1f);
        AddEnumDropdown("Mode", MyEnum.Default, v => Debug.Log(v));

        // グループスコープ（usingで自動クローズ）
        using (HorizontalScope("Row"))
        {
            AddButton("A", () => {});
            AddButton("B", () => {});
        }
        // FoldingScope, DisableScope も同様
    }
}
```

### ScriptablePageModel（エディタ設定可能）

`ScriptableObject`としてアセット化できるページ。`InitializeInternal()`をoverrideしてビルダーAPIで構築する。

```csharp
[CreateAssetMenu(menuName = "XDebugger/My Page")]
public class MyScriptablePage : ScriptablePageModel
{
    protected override void InitializeInternal() { /* AddLabel, AddButton等 */ }
}
```

### GlobalMenuTabPage（静的タブページ）

プレハブに`GlobalMenuTabPage`を配置し、`ScriptablePageModel`をInspectorでアサインすることで、タブとして常駐するページを作成できる。UIFactoryの注入後に自動で初期化・表示される。

### スタイルシステム

`ControlBase.ApplyStyle()`にIStyle実装を渡してレイアウトを調整する。

```csharp
control.ApplyStyle(new BgColor(Color.red), new PreferredHeight(60f), new PreferredWidth(200f));
// 利用可能: BgColor, PreferredHeight, MinHeight, FlexibleHeight, PreferredWidth, MinWidth, FlexibleWidth
```

## 開発ガイドライン

### 新しいコントロール型の追加

1. **Model**: `ControlModelBase`を継承。`CreateControl()`と`prefabAddress`を実装
2. **Control(View)**: `ControlBase`を継承。`Refresh()`でModelの値をUIへ反映
3. **UIFactory**: `UIFactoryBase`の`CreateControl<T>()`に型→プレハブのマッピングを追加
4. Addressablesにプレハブを`XDebugger/{ControlName}`で登録

### 新しいグループ型の追加

1. **GroupModel**: `GroupModel`を継承。`CreateControl()`でグループUIを生成
2. **Group(View)**: `ContentGroup`等を継承しレイアウト動作を実装
3. **Scope**: `GroupLayoutScope`を継承。`CreateModel()`で対応GroupModelを返す
4. UIFactoryにファクトリーメソッドを追加。PageModelにスコープメソッドを追加

### ScriptablePageModelサブクラスの作成

1. `ScriptablePageModel`を継承
2. `[CreateAssetMenu]`属性を付与
3. `InitializeInternal()`をoverrideしビルダーAPIでUIを構築
4. Unityエディタで`Create > XDebugger > ...`からアセットを作成

## コーディング規約

### イベント登録パターン

AddListener前に必ずRemoveListenerを呼ぶ。OnDestroyで解除する。

```csharp
button.onClick.RemoveListener(OnClick);
button.onClick.AddListener(OnClick);
// OnDestroy() { button.onClick.RemoveListener(OnClick); }
```

### Model-View通信

ModelがChangedイベントを発火 → ViewがRefresh()を呼び出す。Viewからの入力はModelの`NotifyValueChangedFromView()`経由で反映。

### 命名規約

- バッキングフィールドは`_`プレフィックス（例: `_value`）を使い、C#キーワードとの衝突を避ける
- プレハブアドレスは`XDebugger/{ControlName}`の形式

### 条件付きコンパイル

```csharp
#if ENABLE_INPUT_SYSTEM
// Input System用コード
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
// Legacy Input Manager用コード
#endif
```

両方が同時に有効な場合があるため、`#elif`ではなく`#if`を個別に使用する。

### UIFactoryとAddressables

- コントロールプレハブの生成は`IUIFactory`経由で行う
- グループプレハブはAddressablesから`WaitForCompletion()`で同期ロード（パフォーマンス注意）

## 設定 (XDebuggerSetting)

- **TriggerMode**: DoubleTap / TripleTap / DoubleFingerHold / TripleFingerHold
- **TriggerPosition**: TopLeft〜BottomRight（8方向）
- **InputGraceTime**: タップ猶予時間
- **HoldTimeForShow**: ホールド表示までの時間
- **UIFactory**: UIFactoryBaseのScriptableObject参照
- **TabButtonPrefab / TopPageTabList**: タブUI設定

## ビルド設定

- アセンブリ定義: `xeon.jp.x_debugger.runtime.asmdef`
- プラットフォーム: iOS, Android, PC, PlayStation, Xbox, Switch
- Addressables: XDebuggerプレハブを`"XDebugger"`アドレスで登録必須
