# XDebugger

Unity向け汎用デバッグ機能パッケージ

## 概要

XDebuggerは、Unityアプリケーションにランタイムデバッグ機能を提供する包括的なデバッグツールです。コンソールログ、プロファイリング、システム情報表示などの機能を統合したデバッグUIシステムを提供します。

## プロジェクト構造

```
Assets/XDebugger/
├── Runtime/
│   ├── Scripts/
│   │   ├── XDebugger.cs           # メインのデバッグUIマネージャー（シングルトン）
│   │   ├── Console/               # コンソールログ機能
│   │   ├── Profiler/              # プロファイリング機能
│   │   ├── Common/                # 共通ユーティリティ
│   │   ├── Controls/              # UIコントロール
│   │   ├── Groups/                # グループ管理
│   │   ├── Models/                # データモデル
│   │   ├── System/                # システム情報
│   │   ├── UI/                    # UIコンポーネント
│   │   ├── Setting/               # 設定管理
│   │   ├── Style/                 # UIスタイル
│   │   ├── Scope/                 # スコープ管理
│   │   └── Interface/             # インターフェース定義
│   ├── Prefabs/                   # UIプレハブ
│   ├── Textures/                  # テクスチャアセット
│   ├── Fonts/                     # フォントアセット
│   ├── Animations/                # アニメーション
│   └── Settings/                  # 設定ファイル
├── Editor/
│   └── Scripts/
│       ├── XDebuggerWindow.cs     # Unityエディタウィンドウ
│       ├── EncodeHelper.cs        # エンコーディングヘルパー
│       └── ScriptImportPostProcessor.cs
├── FlyweightScrollView/           # 軽量スクロールビュー実装
│   └── Runtime/Scripts/
│       ├── FlyweightScrollView.cs
│       ├── FlyweightScrollViewController.cs
│       ├── Layouter/              # レイアウト管理
│       └── Model/                 # データモデル
└── XGraphs/                       # グラフ描画機能
```

## 主要機能

### 1. デバッグUIシステム (XDebugger.cs)

- **シングルトンパターン**: `XDebugger.Instance`でグローバルアクセス
- **Addressablesベース**: プレハブをAddressablesから動的にロード
- **DontDestroyOnLoad**: シーン遷移でも永続化

### 2. トリガーシステム

デバッグメニューの表示方法を複数サポート：

- **DoubleTap/TripleTap**: 画面上のトリガーボタンを2回/3回タップ
- **DoubleFingerHold/TripleFingerHold**: 2本/3本指で長押し（モバイル）
- **PC**: 右クリック長押し
- **コンソール**: Fire2ボタン長押し

プラットフォーム検出：
- `IsMobilePlatform()`: iOS/Android
- `IsConsolePlatform()`: PlayStation, Xbox, Switch

### 3. ページ遷移システム

- **TabController**: タブベースのナビゲーション
- **PageModel**: ページのデータモデル
- **OpenPage()/ClosePage()**: ページの開閉管理
- **履歴管理**: ページスタックによる階層的なナビゲーション

### 4. Console機能

場所: `Assets/XDebugger/Runtime/Scripts/Console/`

- **ConsoleController**: コンソールログの管理
- **LogItemData**: ログエントリのデータモデル
- **IConsolePage**: コンソールページインターフェース
- **ILogDataBuffer**: ログバッファインターフェース

### 5. Profiler機能

場所: `Assets/XDebugger/Runtime/Scripts/Profiler/`

- **ProfilerPage**: プロファイラーページ実装
- **IProfilerPage**: プロファイラーインターフェース

### 6. FlyweightScrollView

場所: `Assets/XDebugger/FlyweightScrollView/`

効率的な仮想スクロールビュー実装（フライウェイトパターン）：

- **FlyweightScrollView**: ベースクラス
- **FlyweightVerticalScrollView**: 垂直スクロール
- **FlyweightHorizontalScrollView**: 水平スクロール
- **CircularBuffer**: 循環バッファによる効率的なデータ管理
- **Layouter**: レイアウト計算（Vertical/Horizontal）

### 7. Editorウィンドウ

場所: `Assets/XDebugger/Editor/Scripts/XDebuggerWindow.cs`

- **メニュー**: `Window > XDebugger`
- **再生モード専用**: プレイモード中のみ動作
- **タブ表示**: Console、Profiler、SystemInfo以外のタブを表示

## 設定 (XDebuggerSetting)

- **TriggerMode**: トリガー方式の選択
- **TriggerPosition**: トリガーボタンの位置（8方向）
- **InputGraceTime**: タップ入力の猶予時間
- **HoldTimeForShow**: ホールド表示までの時間
- **UIFactory**: UIファクトリー（依存性注入）
- **TabButtonPrefab**: タブボタンプレハブ
- **TopPageTabList**: トップページのタブリスト

## 技術スタック

- **Unity**: 2021.3以上推奨
- **TextMeshPro**: テキスト表示
- **Addressables**: アセット管理
- **Input System**: 新しい入力システム対応
- **Legacy Input Manager**: 従来の入力システムも対応
- **UI Toolkit**: UIフレームワーク

## アニメーション

- **Open/Close**: メニューの開閉アニメーション
- **Animator**: ステートマシンベースのアニメーション制御

## 名前空間

- `Xeon.XDebugger`: メイン名前空間
- `Xeon.XDebugger.Console`: コンソール機能
- `Xeon.XDebugger.Profiler`: プロファイラー機能
- `Xeon.XDebugger.Common`: 共通機能
- `Xeon.XDebugger.Model`: データモデル
- `Xeon.XDebugger.UI`: UIコンポーネント
- `Xeon.XDebugger.Control`: コントロール
- `Xeon.XDebugger.Editor`: エディタ拡張

## 使用方法

### 基本的な使用

```csharp
// デバッグメニューを表示
XDebugger.Instance.Show();

// デバッグメニューを非表示
XDebugger.Instance.Hide();

// ページを開く
XDebugger.Instance.OpenPage<MyCustomPage>();

// ページを閉じる
XDebugger.Instance.ClosePage(pageModel);
```

### 初期ページの設定

```csharp
// 初期ページを設定
XDebugger.SetInitialPage(new MyInitialPageModel());
```

### UIファクトリーの設定

```csharp
// UIファクトリーを注入
XDebugger.Instance.SetUIFactory(myUIFactory);
```

## 開発ガイドライン

### ページの追加

1. `PageModel`を継承したクラスを作成
2. `GetOrCreateInitialPage()`または`OpenPage<T>()`で表示
3. 必要に応じて`IPageModel`インターフェースを実装

### カスタムコントロールの追加

1. `Assets/XDebugger/Runtime/Scripts/Controls/`に配置
2. 既存のコントロールを参考に実装
3. UIFactoryに登録

## 注意事項

- **Addressables設定**: XDebuggerプレハブを"XDebugger"というアドレスでAddressablesに登録する必要があります
- **シングルトン**: XDebuggerは自動的にシングルトン化されます
- **DontDestroyOnLoad**: シーン遷移で破棄されないため、手動で管理する場合は注意が必要
- **入力システム**: Input SystemとLegacy Input Managerの両方に対応していますが、プロジェクトの設定に応じて適切な方を選択してください

## ビルド設定

- アセンブリ定義: `xeon.jp.x_debugger.runtime.asmdef`
- プラットフォーム: iOS, Android, PC, PlayStation, Xbox, Switch

## 今後の拡張ポイント

- カスタムページの追加
- 新しいプロファイリング指標
- コンソールのフィルタリング機能強化
- グラフ描画機能の拡張（XGraphs）
- ネットワークデバッグ機能
