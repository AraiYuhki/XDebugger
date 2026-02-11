# FlyweightScrollView

## 概要

FlyweightScrollViewは、Unity UIの`ScrollRect`をベースにした仮想スクロールシステムです。フライウェイトパターンを採用し、ビューポートに表示される分だけのUIアイテムをインスタンス化して使い回すことで、数千〜数万件のデータを持つリストでも高いパフォーマンスを維持します。

通常のScrollViewでは全データ分のGameObjectを生成するためメモリと描画コストが増大しますが、FlyweightScrollViewではビューポートを埋めるのに必要な最小限のアイテムのみを生成し、スクロール時にアイテムの位置とデータバインディングを切り替えることで効率的な表示を実現します。

**主な特徴:**
- ビューポートに収まる数のアイテムのみを生成（メモリ効率が高い）
- 垂直スクロール・水平スクロールの両方に対応
- `IObservableCollection<T>`による変更通知でデータ追加・削除時に自動更新
- 逆順表示モード、末尾固定（スティッキー）モードのサポート
- `CircularBuffer<T>`による固定容量の循環バッファでログ等の用途に最適
- パディング、スペーシング、アライメントなどのレイアウトカスタマイズ

**名前空間:** `Xeon.Common.FlyweightScrollView`

---

## アーキテクチャ

### クラス階層図

```
FlyweightScrollView (MonoBehaviour, 抽象基底クラス)
├── FlyweightVerticalScrollView   (垂直スクロール実装)
└── FlyweightHorizontalScrollView (水平スクロール実装)

FlyweightScrollViewControllerBase (抽象基底クラス, IDisposable)
└── FlyweightScrollViewController<TData, TItem> (ジェネリック実装)

Layouter (抽象基底クラス)
├── VerticalLayouter   (垂直レイアウト計算)
└── HorizontalLayouter (水平レイアウト計算)

FlyweightScrollViewItemBase (抽象基底クラス)
└── FlyweightScrollItem<T> (ジェネリックアイテムラッパー)

IObservableCollection<T> (インターフェース)
├── CircularBuffer<T>                    (固定容量循環バッファ)
└── FlyweightScrollViewDataAdapter<T>    (ObservableCollectionアダプター)

IBindable<TData> (インターフェース)
└── ユーザー定義のアイテムコンポーネント
```

### コンポーネント間の関係

```
┌─────────────────────────────────────────────────────────┐
│  FlyweightVerticalScrollView / HorizontalScrollView     │
│  (MonoBehaviour - Inspectorで設定)                       │
│  ┌──────────────────────────────────────────────┐       │
│  │  FlyweightScrollViewParam                    │       │
│  │  - ViewPort, Padding, Spacing                │       │
│  │  - IsControlChildSize, IsReverse             │       │
│  │  - IsAtLastSticky                            │       │
│  └──────────────────────────────────────────────┘       │
│  ┌──────────────────┐  ┌──────────────────────┐        │
│  │  ScrollRect       │  │  FlyweightScrollViewport │    │
│  │  (Unity標準)      │  │  (ビューポートサイズ監視)│    │
│  └──────────────────┘  └──────────────────────┘        │
└─────────────────┬───────────────────────────────────────┘
                  │ Setup(controller)
                  ▼
┌─────────────────────────────────────────────────────────┐
│  FlyweightScrollViewController<TData, TItem>            │
│  - prefab: TItem                                        │
│  - dataList: IObservableCollection<TData>               │
│  - Layouter (Vertical / Horizontal)                     │
│  - LinkedList<FlyweightScrollViewItemBase> (表示中アイテム)│
└───────────┬─────────────────────┬───────────────────────┘
            │ Bind(data)          │ CollectionChanged
            ▼                     ▼
┌──────────────────┐  ┌──────────────────────────────┐
│  TItem            │  │  IObservableCollection<TData> │
│  (IBindable<TData>)│  │  - CircularBuffer<T>          │
│  (MonoBehaviour)  │  │  - FlyweightScrollViewDataAdapter<T>│
└──────────────────┘  └──────────────────────────────┘
```

### 処理フロー

1. **初期化**: `scrollView.Setup(controller)` を呼び出すと、コントローラーがLayouterを生成し、ビューポートサイズに基づいて必要なアイテム数を計算、プレハブからアイテムを生成します。
2. **スクロール**: `ScrollRect.onValueChanged`イベントを検知し、スクロール方向に応じてアイテムをLinkedListの先頭/末尾間で移動させ、新しいデータをバインドします。
3. **データ変更**: `IObservableCollection<T>.CollectionChanged`イベントにより、アイテムの追加・削除・リセットを検知し、コンテナサイズの更新と再描画を行います。
4. **再描画**: `IsDirty`フラグが立っている場合、`Update()`ループで`UpdateView()`を呼び出し、全表示アイテムの位置とデータバインディングを更新します。

---

## 使用方法

### 基本的なセットアップ

#### 1. アイテムコンポーネントの作成

`IBindable<TData>`を実装したMonoBehaviourクラスを作成します。これがスクロールビューの各行（または各列）に表示されるUIアイテムとなります。

```csharp
using System;
using TMPro;
using UnityEngine;
using Xeon.Common;

public class MyListItem : MonoBehaviour, IBindable<string>
{
    [SerializeField] private TMP_Text label;

    public event Action<string> OnSelect;

    public void Bind(string data)
    {
        label.text = data;
    }
}
```

`IBindable<TData>`インターフェースには以下の2つのメンバーがあります:
- `void Bind(TData data)` - データをUIに反映する処理を実装します
- `event Action<TData> OnSelect` - アイテムが選択されたときのイベント（任意で利用）

#### 2. データコレクションの準備

データソースとして`IObservableCollection<T>`を実装したコレクションを用意します。以下の選択肢があります:

**方法A: ObservableCollectionを使用（FlyweightScrollViewDataAdapterで自動ラップ）**

```csharp
using System.Collections.ObjectModel;

var dataList = new ObservableCollection<string>();
dataList.Add("Item 1");
dataList.Add("Item 2");
dataList.Add("Item 3");
```

**方法B: CircularBufferを使用（固定容量、ログ表示向け）**

```csharp
using Xeon.Common.FlyweightScrollView.Model;

// 最大1000件を保持する循環バッファ
var buffer = new CircularBuffer<string>(1000);
buffer.Add("Log entry 1");
buffer.Add("Log entry 2");
// 容量を超えると最も古いエントリが自動的に上書きされる
```

**方法C: FlyweightScrollViewDataAdapterで明示的にラップ**

```csharp
using System.Collections.ObjectModel;
using Xeon.Common.FlyweightScrollView;

var source = new ObservableCollection<string>();
var adapter = new FlyweightScrollViewDataAdapter<string>(source);
```

#### 3. コントローラーの作成とセットアップ

```csharp
using Xeon.Common.FlyweightScrollView;

// コントローラーを作成
// 引数: プレハブ, データコレクション, アイテム生成時コールバック(省略可)
var controller = new FlyweightScrollViewController<string, MyListItem>(
    itemPrefab,     // MyListItemコンポーネントがアタッチされたプレハブ
    dataList,       // IObservableCollection<string> または ObservableCollection<string>
    null            // アイテム生成時のコールバック（不要なら null）
);

// スクロールビューにコントローラーを設定
// scrollView は FlyweightVerticalScrollView または FlyweightHorizontalScrollView
scrollView.Setup(controller);
```

### コード例

#### 垂直スクロールリストの基本的な実装

```csharp
using System;
using System.Collections.ObjectModel;
using TMPro;
using UnityEngine;
using Xeon.Common;
using Xeon.Common.FlyweightScrollView;

/// <summary>
/// リストアイテムのUIコンポーネント
/// </summary>
public class UserListItem : MonoBehaviour, IBindable<UserData>
{
    [SerializeField] private TMP_Text nameLabel;
    [SerializeField] private TMP_Text scoreLabel;

    public event Action<UserData> OnSelect;

    public void Bind(UserData data)
    {
        nameLabel.text = data.Name;
        scoreLabel.text = data.Score.ToString();
    }
}

[Serializable]
public class UserData
{
    public string Name;
    public int Score;
}

/// <summary>
/// スクロールビューの管理クラス
/// </summary>
public class UserListView : MonoBehaviour
{
    [SerializeField] private FlyweightVerticalScrollView scrollView;
    [SerializeField] private UserListItem itemPrefab;

    private ObservableCollection<UserData> dataList;
    private FlyweightScrollViewController<UserData, UserListItem> controller;

    private void Start()
    {
        // データの準備
        dataList = new ObservableCollection<UserData>();
        for (var i = 0; i < 10000; i++)
        {
            dataList.Add(new UserData
            {
                Name = $"User {i}",
                Score = UnityEngine.Random.Range(0, 100)
            });
        }

        // コントローラーの作成
        controller = new FlyweightScrollViewController<UserData, UserListItem>(
            itemPrefab,
            dataList,
            OnItemCreated
        );

        // セットアップ
        scrollView.Setup(controller);
    }

    private void OnItemCreated(UserListItem item)
    {
        // アイテムが生成されたときの初期化処理（ボタンイベントの登録など）
        item.OnSelect += OnItemSelected;
    }

    private void OnItemSelected(UserData data)
    {
        Debug.Log($"Selected: {data.Name}");
    }

    /// <summary>
    /// 動的にデータを追加する例
    /// </summary>
    public void AddUser(string name, int score)
    {
        dataList.Add(new UserData { Name = name, Score = score });
        // CollectionChangedイベントにより自動的にスクロールビューが更新される
    }
}
```

#### CircularBufferを使ったログビューアの実装

```csharp
using System;
using TMPro;
using UnityEngine;
using Xeon.Common;
using Xeon.Common.FlyweightScrollView;
using Xeon.Common.FlyweightScrollView.Model;

public class LogItem : MonoBehaviour, IBindable<string>
{
    [SerializeField] private TMP_Text logText;

    public event Action<string> OnSelect;

    public void Bind(string data)
    {
        logText.text = data;
    }
}

public class LogViewer : MonoBehaviour
{
    [SerializeField] private FlyweightVerticalScrollView scrollView;
    [SerializeField] private LogItem logItemPrefab;

    // 最大500件のログを保持する循環バッファ
    private CircularBuffer<string> logBuffer;

    private void Start()
    {
        logBuffer = new CircularBuffer<string>(500);

        var controller = new FlyweightScrollViewController<string, LogItem>(
            logItemPrefab,
            logBuffer
        );

        // IsAtLastSticky が有効な場合、末尾に新しいログが追加されると
        // 自動的にスクロール位置が末尾に追従する
        scrollView.Setup(controller);
    }

    public void AddLog(string message)
    {
        logBuffer.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
        // 500件を超えると最も古いログが自動的に上書きされる
    }
}
```

#### データリストの差し替え

```csharp
// 新しいデータリストに差し替える
var newDataList = new FlyweightScrollViewDataAdapter<UserData>(newObservableCollection);
controller.SetDataList(newDataList);
// 自動的にリセット通知が発火し、ビューが更新される
```

### Hierarchy構成

Inspectorで以下のようなHierarchy構成を設定します:

```
Canvas
└── FlyweightVerticalScrollView (FlyweightVerticalScrollViewコンポーネント)
    └── ScrollRect (ScrollRectコンポーネント)
        ├── Viewport (FlyweightScrollViewportコンポーネント, RectMask2D)
        │   └── Content (RectTransform - アイテムのコンテナ)
        ├── Scrollbar Horizontal (任意)
        └── Scrollbar Vertical (任意)
```

`FlyweightScrollView`コンポーネントのInspectorフィールドに以下を設定します:
- **ScrollView**: ScrollRectコンポーネントへの参照
- **ViewPort**: FlyweightScrollViewportコンポーネントへの参照
- **Content**: コンテンツコンテナのRectTransform
- **Param**: 各種パラメータ（後述）
- **Alignment**: 垂直ビューでは`HorizontalAlignment`、水平ビューでは`VerticalAlignment`

---

## クラスリファレンス

### FlyweightScrollView

**名前空間:** `Xeon.Common.FlyweightScrollView`
**ファイル:** `FlyweightScrollView.cs`

スクロールビューのMonoBehaviour基底クラス。ScrollRectのイベントハンドリングとコントローラーのライフサイクル管理を行います。直接使用せず、サブクラスを使用してください。

| メンバー | 型 | 説明 |
|---|---|---|
| `Spacing` | `float` | アイテム間のスペース（get/set） |
| `normalizedPosition` | `Vector2` | 正規化されたスクロール位置（0.0〜1.0） |
| `Setup(controller)` | `void` | コントローラーを設定し、スクロールビューを初期化する |

---

### FlyweightVerticalScrollView

**名前空間:** `Xeon.Common.FlyweightScrollView`
**ファイル:** `FlyweightVerticalScrollView.cs`

垂直スクロールの実装クラス。`[ExecuteInEditMode]`属性が付与されています。

| メンバー | 型 | 説明 |
|---|---|---|
| `alignment` | `HorizontalAlignment` | アイテムの水平方向の配置（Left / Center / Right） |

---

### FlyweightHorizontalScrollView

**名前空間:** `Xeon.Common.FlyweightScrollView`
**ファイル:** `FlyweightHorizontalScrollView.cs`

水平スクロールの実装クラス。`[ExecuteInEditMode]`属性が付与されています。

| メンバー | 型 | 説明 |
|---|---|---|
| `alignment` | `VerticalAlignment` | アイテムの垂直方向の配置（Top / Middle / Bottom） |

---

### FlyweightScrollViewControllerBase

**名前空間:** `Xeon.Common.FlyweightScrollView`
**ファイル:** `FlyweightScrollViewControllerBase.cs`

コントローラーの抽象基底クラス。アイテムの生成・再配置・スクロール位置管理のコアロジックを提供します。`IDisposable`を実装しています。

| メンバー | 型 | 説明 |
|---|---|---|
| `ItemCount` | `int` (abstract) | データソースの総アイテム数 |
| `IsDirty` | `bool` | 再描画が必要かどうかのフラグ |
| `Setup(scrollView, param, container, HorizontalAlignment)` | `void` | 垂直スクロール用に初期化 |
| `Setup(scrollView, param, container, VerticalAlignment)` | `void` | 水平スクロール用に初期化 |
| `Update(isNext, normalizedPosition, isPositionLast)` | `void` | スクロール位置に応じてビューを更新 |
| `UpdateView()` | `void` | 現在のインデックスに基づいてビュー全体を再描画 |
| `UpdateViewportSize()` | `void` | ビューポートサイズ変更時の再計算 |
| `UpdateContainerSize()` | `void` | コンテナサイズの更新 |
| `FixToHead()` | `void` | スクロール位置を先頭に固定 |
| `FixToLast()` | `void` | スクロール位置を末尾に固定 |
| `SetOnChangedItemCount(callback)` | `void` | アイテム数変更時のコールバックを設定 |
| `SetSpacing(spacing)` | `void` | アイテム間のスペースを設定 |
| `SetHorizontalAlignment(alignment)` | `void` | 水平方向の配置を設定 |
| `SetVerticalAlignment(alignment)` | `void` | 垂直方向の配置を設定 |
| `SetIsReverse(isReverse)` | `void` | 逆順表示モードを設定 |
| `SetIsPositionLast(flag)` | `void` | 末尾位置フラグを設定 |
| `GetFitItemSize()` | `Vector2` | コンテナサイズに合わせたアイテムサイズを取得 |
| `Dispose()` | `void` | リソースの解放（全アイテムオブジェクトの破棄） |

---

### FlyweightScrollViewController\<TData, TItem\>

**名前空間:** `Xeon.Common.FlyweightScrollView`
**ファイル:** `FlyweightScrollViewController.cs`

ジェネリックなコントローラー実装クラス。データリストとUIアイテムのバインディングを管理します。

**型制約:**
- `TData` - リストに表示するデータの型
- `TItem : MonoBehaviour, IBindable<TData>` - UIアイテムのコンポーネント型

| メンバー | 型 | 説明 |
|---|---|---|
| `OnItemCreated` | `event Action<TItem>` | アイテムが生成されたときに発火するイベント |
| `ItemCount` | `int` | データソースの総アイテム数 |

**コンストラクタ:**

```csharp
// ObservableCollectionを使用（内部でFlyweightScrollViewDataAdapterにラップ）
FlyweightScrollViewController(TItem prefab, ObservableCollection<TData> dataList, Action<TItem> onCreatedItem)

// IObservableCollectionを使用
FlyweightScrollViewController(TItem prefab, IObservableCollection<TData> dataList, Action<TItem> onItemCreated = null)
```

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `SetDataList(newDataList)` | `void` | 表示するデータリストを差し替える |
| `GetSample()` | `TItem` | サイズ計算用のサンプルアイテムを取得 |
| `Dispose()` | `void` | リソースの解放 |

---

### IBindable\<TData\>

**名前空間:** `Xeon.Common`
**ファイル:** `Interfaces/IBindable.cs`

データバインディング可能なUIアイテムのインターフェース。スクロールビューで表示する各アイテムのMonoBehaviourに実装します。

```csharp
public interface IBindable<TData>
{
    /// <summary>
    /// データをUIにバインドします
    /// </summary>
    void Bind(TData data);

    /// <summary>
    /// アイテムが選択されたときに発火するイベント
    /// </summary>
    event Action<TData> OnSelect;
}
```

---

### IObservableCollection\<T\>

**名前空間:** `Xeon.Common.FlyweightScrollView`
**ファイル:** `Interfaces/IObservableCollection.cs`

変更通知機能を持つコレクションのインターフェース。`IEnumerable<T>`と`INotifyCollectionChanged`を継承しています。

```csharp
public interface IObservableCollection<T> : IEnumerable<T>, INotifyCollectionChanged
{
    T this[int index] { get; }
    void Clear(bool isNotify = true);
    int Count { get; }
}
```

---

### CircularBuffer\<T\>

**名前空間:** `Xeon.Common.FlyweightScrollView.Model`
**ファイル:** `Model/CircularBuffer.cs`

固定サイズの循環バッファ。容量を超えた場合、最も古い要素が自動的に上書きされます。`IObservableCollection<T>`と`IReadOnlyList<T>`を実装しています。ログの保存など、最新のN件だけを保持したい場合に適したデータ構造です。

**コンストラクタ:**

```csharp
// 初期データを指定して初期化
CircularBuffer(int capacity, T[] items)

// 容量のみ指定して初期化
CircularBuffer(int capacity, bool fill = false)
```

| メンバー | 型 | 説明 |
|---|---|---|
| `Capacity` | `int` | バッファの最大容量 |
| `Count` | `int` | 現在の要素数 |
| `IsFull` | `bool` | バッファが満杯かどうか |
| `IsEmpty` | `bool` | バッファが空かどうか |
| `this[int index]` | `T` | 指定インデックスの要素（get/set） |
| `CollectionChanged` | `event` | コレクション変更通知イベント |

| メソッド | 説明 |
|---|---|
| `Add(item, isNotify)` | 末尾に要素を追加（PushBackのエイリアス） |
| `PushBack(item, isNotify)` | 末尾に要素を追加。満杯時は先頭を上書き |
| `PushFront(item, isNotify)` | 先頭に要素を追加。満杯時は末尾を上書き |
| `PopBack(isNotify)` | 末尾の要素を削除 |
| `PopFront(isNotify)` | 先頭の要素を削除 |
| `Front()` | 先頭の要素を取得 |
| `Back()` | 末尾の要素を取得 |
| `Clear(isNotify)` | 全要素を削除 |

**isNotifyパラメータ:** 各操作メソッドには`isNotify`パラメータ（デフォルト`true`）があり、`false`を指定すると`CollectionChanged`イベントの発火を抑制できます。大量のデータを一括操作する際のパフォーマンス最適化に利用できます。

---

### FlyweightScrollViewDataAdapter\<T\>

**名前空間:** `Xeon.Common.FlyweightScrollView`
**ファイル:** `FlyweightScrollViewDataAdapter.cs`

`ObservableCollection<T>`を`IObservableCollection<T>`に変換するアダプタークラス。`FlyweightScrollViewController`のコンストラクタに`ObservableCollection<T>`を渡した場合、内部で自動的にこのアダプターが使用されます。

```csharp
var source = new ObservableCollection<string>();
var adapter = new FlyweightScrollViewDataAdapter<string>(source);
// adapter は IObservableCollection<string> として使用可能
```

---

### Layouter

**名前空間:** `Xeon.Common.FlyweightScrollView`
**ファイル:** `Layouter/Layouter.cs`

レイアウト計算の抽象基底クラス。アイテムの配置位置、コンテナサイズ、表示インデックスの計算を担当します。

| メソッド | 説明 |
|---|---|
| `GetItemCount()` | 表示に必要なアイテム数を取得 |
| `GetTailIndex()` | 表示範囲の末尾インデックスを取得 |
| `GetPosition(index)` | 指定インデックスのアイテム位置を取得 |
| `CalculateIndex(itemCount, scrollPosition)` | スクロール位置から表示開始インデックスを計算 |
| `UpdateContainerSize(itemCount)` | コンテナサイズを更新 |
| `SetItemSize(item)` | アイテムのサイズを設定 |
| `GetFitItemSize()` | コンテナサイズに合わせたアイテムサイズを取得 |
| `GetContentSize(itemCount)` | 総コンテンツサイズを取得 |
| `GetContentOffset(itemCount, scrollPosition)` | スクロール位置からピクセルオフセットを計算 |
| `GetScrollPositionFromOffset(itemCount, contentOffset)` | ピクセルオフセットから正規化スクロール位置を計算 |

#### VerticalLayouter

垂直方向のレイアウトを計算します。`HorizontalAlignment`（Left / Center / Right）でアイテムの水平配置を制御します。

#### HorizontalLayouter

水平方向のレイアウトを計算します。`VerticalAlignment`（Top / Middle / Bottom）でアイテムの垂直配置を制御します。

---

### FlyweightScrollViewItemBase

**名前空間:** `Xeon.Common.FlyweightScrollView`
**ファイル:** `FlyweightScrollViewItemBase.cs`

スクロールアイテムの抽象基底クラス。GameObjectのラッパーとして、位置設定・ビューポート内判定・アライメント設定を行います。

| メンバー | 型 | 説明 |
|---|---|---|
| `gameObject` | `GameObject` | アイテムのGameObject |
| `RectTransform` | `RectTransform` | アイテムのRectTransform |
| `IsInside` | `bool` | ビューポート内に存在するかどうか |

| メソッド | 説明 |
|---|---|
| `SetPosition(position)` | アイテムの位置を設定 |
| `UpdateIsInside()` | ビューポート内判定を更新 |
| `SetHorizontalAlignment(alignment)` | 水平方向の配置を設定 |
| `SetVerticalAlignment(alignment)` | 垂直方向の配置を設定 |
| `SetFittingItemWidth(contentWidth)` | コンテナ幅に合わせてアイテム幅を設定 |
| `SetFittingItemHeight(contentHeight)` | コンテナ高さに合わせてアイテム高さを設定 |

---

### FlyweightScrollViewport

**名前空間:** `Xeon.Common`
**ファイル:** `FlyweightScrollViewport.cs`

ビューポートのRectTransformサイズ変更を監視するMonoBehaviourコンポーネント。`RectTransform`と`RectMask2D`を必須コンポーネントとしています。

| メンバー | 型 | 説明 |
|---|---|---|
| `RectTransform` | `RectTransform` | ビューポートのRectTransform |
| `OnRectTransformDimensionsChanged` | `event Action` | サイズ変更時に発火するイベント |

---

## パラメータ設定

### FlyweightScrollViewParam

`FlyweightScrollView`コンポーネントのInspectorで設定するパラメータクラスです。

| フィールド | 型 | デフォルト | 説明 |
|---|---|---|---|
| `ViewPort` | `RectTransform` | - | ビューポートのRectTransform。表示領域を定義します。 |
| `Padding` | `RectOffset` | - | コンテンツの上下左右の余白（ピクセル）。 |
| `Spacing` | `float` | `0` | アイテム間のスペース（ピクセル）。ランタイムでも変更可能です。 |
| `IsControlChildSize` | `bool` | `false` | `true`の場合、アイテムのサイズをコンテナに合わせて自動調整します。垂直スクロールではアイテムの幅を、水平スクロールではアイテムの高さをコンテナサイズに合わせます。 |
| `IsReverse` | `bool` | `false` | `true`の場合、データの表示順序を逆にします。垂直スクロールでは最新のデータが下に、水平スクロールでは最新のデータが右に表示されます。 |
| `IsAtLastSticky` | `bool` | `false` | `true`の場合、スクロール位置が末尾にあるとき、新しいアイテムが追加されると自動的に末尾までスクロールします。チャットやログビューアのように、常に最新のメッセージを表示したい場合に有効です。 |

### アライメント設定

#### HorizontalAlignment（垂直スクロール時のアイテム水平配置）

| 値 | 説明 |
|---|---|
| `Left` | アイテムを左寄せ |
| `Center` | アイテムを中央寄せ |
| `Right` | アイテムを右寄せ |

#### VerticalAlignment（水平スクロール時のアイテム垂直配置）

| 値 | 説明 |
|---|---|
| `Top` | アイテムを上寄せ |
| `Middle` | アイテムを中央寄せ |
| `Bottom` | アイテムを下寄せ |

---

## エディタ機能

`FlyweightScrollView`のInspectorには、以下のデバッグ用ボタンが表示されます:

- **Debug Simulate**: 0〜99の整数データを使って100件のテスト表示を行います。編集モードでのレイアウト確認に使用します。
- **Clear**: デバッグ表示をクリアします。

これらはエディタ専用（`UNITY_EDITOR`）の機能であり、ビルドには含まれません。
