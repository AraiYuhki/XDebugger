# XDebugger

Unity向け汎用デバッグ機能パッケージ。ランタイムデバッグUI（コンソール、プロファイラー、システム情報等）をAddressablesベースのシングルトンとして提供する。

## アーキテクチャ

**Model-View-Factory パターン**。ModelがデータとChangedイベントを持ち、ControlBase派生のViewがUIを描画し、UIFactoryがプレハブからインスタンスを生成する。

**ページライフサイクル**: `Initialize()` → `OpenPage()` → `CreateControl()` → `OpenedPage()` → ... → `Close()` → `ClosedPage()` → `Clear()`

## サブモジュール

`Assets/XDebugger/XGraphs/` はgitサブモジュール（`git@github.com:AraiYuhki/XGraphs.git`）。
初回クローン後: `git submodule update --init --recursive`

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

`GlobalMenuTabPage`プレハブにScriptablePageModelをInspectorでアサインすると常駐タブになる（UIFactory注入後に自動初期化・表示）。

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

`ENABLE_INPUT_SYSTEM` と `ENABLE_LEGACY_INPUT_MANAGER` は両方が同時に有効な場合があるため、`#elif`ではなく`#if`を個別に使用する。

### UIFactoryとAddressables

- コントロールプレハブの生成は`IUIFactory`経由で行う
- グループプレハブはAddressablesから`WaitForCompletion()`で同期ロード（パフォーマンス注意）

## ビルド設定

- Addressables: XDebuggerプレハブを`"XDebugger"`アドレスで登録必須
