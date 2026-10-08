# XDebugger

Unity向け汎用デバッグ機能パッケージ。ランタイムデバッグUI（コンソール、プロファイラー、システム情報等）をAddressablesベースのシングルトンとして提供します。

## 特徴

- **宣言的ビルダーAPI** - `AddLabel` / `AddButton` / `AddToggle` 等のメソッドチェーンでデバッグページを簡単に構築
- **Model-View-Factory パターン** - データモデルとUIを分離した拡張性の高い設計
- **Addressablesベース** - プレハブの動的ロードによる柔軟なUI生成
- **コンソール** - ログの表示・フィルタリング機能（Info / Warning / Error）
- **プロファイラー** - ランタイムパフォーマンス監視
- **グラフ描画** - XGraphsサブモジュールによるライン・バー・パイ等のチャート表示
- **仮想スクロールビュー** - フライウェイトパターンによる大量データの高速スクロール
- **マルチ入力対応** - Input System / Legacy Input Manager の両方に対応

## 動作要件

- Unity 6 以降
- [UniTask](https://github.com/Cysharp/UniTask)
- Addressables
- TextMeshPro

## インストール

### UPM (Unity Package Manager) 経由

1. `Window > Package Manager` を開く
2. `+` ボタンから `Add package from git URL...` を選択
3. リポジトリURLを入力

### サブモジュールの初期化

XGraphs（グラフ描画機能）はgitサブモジュールとして管理されています。

```bash
git submodule update --init --recursive
```

### Addressablesの設定

XDebuggerプレハブを `"XDebugger"` アドレスでAddressablesに登録してください。

## 使い方

### 基本操作

```csharp
// デバッグメニューの表示/非表示
XDebugger.Instance.Show();
XDebugger.Instance.Hide();

// ページ遷移
XDebugger.Instance.OpenPage<MyPage>();
XDebugger.Instance.ClosePage(model);

// 初期ページの設定
XDebugger.SetInitialPage(new MyPageModel());
```

### カスタムページの作成

`PageModel` を継承し、`InitializeInternal()` でビルダーAPIを使ってUIを構築します。

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
    }
}
```

### ScriptablePageModel（エディタ設定可能）

`ScriptableObject` としてアセット化できるページです。

```csharp
[CreateAssetMenu(menuName = "XDebugger/My Page")]
public class MyScriptablePage : ScriptablePageModel
{
    protected override void InitializeInternal()
    {
        AddLabel("Hello");
        AddButton("Action", () => { });
    }
}
```

### スタイルシステム

```csharp
control.ApplyStyle(
    new BgColor(Color.red),
    new PreferredHeight(60f),
    new PreferredWidth(200f)
);
```

利用可能なスタイル: `BgColor`, `PreferredHeight`, `MinHeight`, `FlexibleHeight`, `PreferredWidth`, `MinWidth`, `FlexibleWidth`

## 設定 (XDebuggerSetting)

| 項目 | 説明 |
|---|---|
| TriggerMode | デバッグメニューの起動方法（DoubleTap / TripleTap / DoubleFingerHold / TripleFingerHold） |
| TriggerPosition | 起動判定の画面位置（TopLeft〜BottomRight、8方向） |
| InputGraceTime | タップ猶予時間 |
| HoldTimeForShow | ホールド表示までの時間 |
| UIFactory | UIFactoryBaseのScriptableObject参照 |

## 対応プラットフォーム

iOS, Android, PC (Windows/macOS/Linux), PlayStation, Xbox, Switch

## ライセンス

このリポジトリのライセンスについてはLICENSEファイルを参照してください。

## 対応Unityバージョン

- 動作確認済み最新バージョン: 6000.6.4f1
- 最低対応バージョン: 6000.0.0f1
