# XDebugger 改善実装プラン

**作成日**: 2026-01-19
**対象バージョン**: v1.0
**目標バージョン**: v1.1

---

## 📋 目次

1. [実装スケジュール](#実装スケジュール)
2. [フェーズ1: Critical Issues（Week 1）](#フェーズ1-critical-issues)
3. [フェーズ2: Code Quality（Week 2-3）](#フェーズ2-code-quality)
4. [フェーズ3: Documentation（Week 4-5）](#フェーズ3-documentation)
5. [テスト計画](#テスト計画)
6. [リリース計画](#リリース計画)

---

## 🗓️ 実装スケジュール

### 全体タイムライン

```
Week 1: Critical Issues
├── Day 1-2: Addressables非同期化
├── Day 3-4: 静的イベント修正
└── Day 5: ProfilerPage修正 + テスト

Week 2-3: Code Quality
├── Day 1: Update()最適化
├── Day 2: 各種バグ修正
├── Day 3-4: GC最適化
└── Day 5: コードレビュー

Week 4-5: Documentation
├── Day 1-3: XMLドキュメント
├── Day 4: README更新
└── Day 5: リリース準備
```

---

## 🔴 フェーズ1: Critical Issues（Week 1）

### タスク1: Addressables非同期化

**目的**: メインスレッドのブロッキングを解消

**影響範囲**:
- `XDebugger.cs`
- `XDebugger` を使用する全てのコード

**実装詳細**:

#### Step 1: 非同期版APIの追加

**ファイル**: `Assets/XDebugger/Runtime/Scripts/XDebugger.cs`

```csharp
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Xeon.XDebugger
{
    public class XDebugger : MonoBehaviour
    {
        private static XDebugger instance;
        private static Task<XDebugger> instanceTask;
        private static AsyncOperationHandle<GameObject> loadHandle;

        /// <summary>
        /// 非同期でXDebuggerインスタンスを取得します（推奨）
        /// </summary>
        public static async Task<XDebugger> GetInstanceAsync()
        {
            if (instance != null)
                return instance;

            // 既に読み込み中の場合は同じTaskを返す
            if (instanceTask != null && !instanceTask.IsCompleted)
                return await instanceTask;

            instanceTask = LoadInstanceAsync();
            return await instanceTask;
        }

        /// <summary>
        /// Addressablesからプレハブを非同期で読み込み、インスタンス化します
        /// </summary>
        private static async Task<XDebugger> LoadInstanceAsync()
        {
            try
            {
                loadHandle = Addressables.LoadAssetAsync<GameObject>(nameof(XDebugger));
                var prefab = await loadHandle.Task;

                if (prefab == null)
                {
                    Debug.LogError("XDebugger prefab not found in Addressables.");
                    return null;
                }

                var go = Instantiate(prefab);
                if (go == null)
                {
                    Debug.LogError("Failed to instantiate XDebugger prefab.");
                    return null;
                }

                instance = go.GetComponent<XDebugger>();

                if (instance == null)
                {
                    Debug.LogError("XDebugger component not found on instantiated prefab.");
                    Destroy(go);
                    return null;
                }

                return instance;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load XDebugger: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 同期版（非推奨）- 既存コードとの互換性のために残す
        /// </summary>
        [Obsolete("Use GetInstanceAsync() instead to avoid blocking the main thread")]
        public static XDebugger Instance
        {
            get
            {
                if (instance != null)
                    return instance;

                Debug.LogWarning("XDebugger.Instance is deprecated. Use GetInstanceAsync() instead.");

                var prefab = Addressables.LoadAssetAsync<GameObject>(nameof(XDebugger))
                    .WaitForCompletion();

                if (prefab == null)
                {
                    Debug.LogError("XDebugger prefab not found in Addressables.");
                    return null;
                }

                var go = Instantiate(prefab);
                instance = go.GetComponent<XDebugger>();
                return instance;
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
                instanceTask = null;

                // Addressablesのハンドルを解放
                if (loadHandle.IsValid())
                {
                    Addressables.Release(loadHandle);
                }
            }
        }
    }
}
```

#### Step 2: 使用箇所の更新

**既存コード**:
```csharp
XDebugger.Instance.Show();
```

**新しいコード**:
```csharp
var debugger = await XDebugger.GetInstanceAsync();
debugger?.Show();
```

**移行期間**: 3ヶ月（その後、同期版を削除）

#### テスト計画

1. **単体テスト**:
   - 非同期読み込みの成功
   - プレハブが存在しない場合のエラーハンドリング
   - 複数回の同時呼び出し

2. **統合テスト**:
   - シーン読み込み時の動作
   - DontDestroyOnLoadの動作確認

3. **パフォーマンステスト**:
   - 読み込み時間の計測
   - メモリ使用量の確認

**推定工数**: 2-3時間
**リスク**: 低（既存APIは残す）

---

### タスク2: 静的イベントのメモリリーク修正

**目的**: シーン遷移時のメモリリーク解消

**影響範囲**:
- `TabController.cs`
- `ConsolePage.cs`

**実装詳細**:

#### Step 1: TabControllerの修正

**ファイル**: `Assets/XDebugger/Runtime/Scripts/Common/TabController.cs`

**変更前**:
```csharp
private static LogItemBuffer logDataList = new(LogBufferCapacity);
private static event Action<int> onAddInfoLog;
private static event Action<int> onAddWarningLog;
private static event Action<int> onAddErrorLog;

public static void PreInitialize()
{
    Application.logMessageReceived += OnReceivedLogMessage;
    Application.quitting += () =>
    {
        Application.logMessageReceived -= OnReceivedLogMessage;
    };
}
```

**変更後**:
```csharp
// staticを削除
private LogItemBuffer logDataList;
private event Action<int> onAddInfoLog;
private event Action<int> onAddWarningLog;
private event Action<int> onAddErrorLog;

private bool isLogReceiverRegistered = false;

// PreInitializeをインスタンスメソッドに変更
public void InitializeLogReceiver()
{
    if (isLogReceiverRegistered)
        return;

    logDataList = new LogItemBuffer(LogBufferCapacity);
    Application.logMessageReceived += OnReceivedLogMessage;
    isLogReceiverRegistered = true;
}

private void OnDestroy()
{
    if (isLogReceiverRegistered)
    {
        Application.logMessageReceived -= OnReceivedLogMessage;
        isLogReceiverRegistered = false;
    }

    // イベントハンドラをクリア
    onAddInfoLog = null;
    onAddWarningLog = null;
    onAddErrorLog = null;

    // バッファの破棄
    logDataList?.Dispose();
    logDataList = null;
}

// public static プロパティをインスタンスプロパティに変更
public LogItemBuffer LogBuffer => logDataList;

// RegisterLogHandlersもインスタンスメソッドに
public void RegisterLogHandlers(Action<int> onInfo, Action<int> onWarn, Action<int> onError)
{
    onAddInfoLog += onInfo;
    onAddWarningLog += onWarn;
    onAddErrorLog += onError;
}

public void UnregisterLogHandlers(Action<int> onInfo, Action<int> onWarn, Action<int> onError)
{
    onAddInfoLog -= onInfo;
    onAddWarningLog -= onWarn;
    onAddErrorLog -= onError;
}

private void OnReceivedLogMessage(string condition, string stackTrace, LogType type)
{
    if (logDataList == null)
        return;

    var data = new LogItemData(type, condition, stackTrace, logDataList.Count, true);
    logDataList.Add(data);

    switch (type)
    {
        case LogType.Log:
            onAddInfoLog?.Invoke(logDataList.InfoCount);
            break;
        case LogType.Warning:
            onAddWarningLog?.Invoke(logDataList.WarnCount);
            break;
        default:
            onAddErrorLog?.Invoke(logDataList.ErrorCount);
            break;
    }
}
```

#### Step 2: XDebugger.csの更新

**ファイル**: `Assets/XDebugger/Runtime/Scripts/XDebugger.cs`

**変更前**:
```csharp
private void Awake()
{
    // ...
    TabController.PreInitialize();
    DontDestroyOnLoad(gameObject);
}
```

**変更後**:
```csharp
private void Awake()
{
    // ...
    DontDestroyOnLoad(gameObject);
}

private void Start()
{
    if (tabController == null)
    {
        Debug.LogError("TabController is not assigned to XDebugger");
        return;
    }

    // ログレシーバーの初期化を追加
    tabController.InitializeLogReceiver();
    tabController.Setup(setting.UIFactory, setting.TabButtonPrefab, setting.TopPageTabList, title => titleLabel.text = title);
    InitializeTrigger();
}
```

#### Step 3: ConsolePage.csの更新

**ファイル**: `Assets/XDebugger/Runtime/Scripts/Console/ConsolePage.cs`

**変更前**:
```csharp
public override void Setup(TabController tabController, UIFactoryBase uiFactory)
{
    // ...
    controller.Initialize(TabController.LogBuffer);
    TabController.RegisterLogHandlers(controller.OnAddInfoLog, controller.OnAddWarningLog, controller.OnAddErrorLog);
}

private void OnDestroy()
{
    TabController.UnregisterLogHandlers(controller.OnAddInfoLog, controller.OnAddWarningLog, controller.OnAddErrorLog);
}
```

**変更後**:
```csharp
private TabController tabControllerInstance;

public override void Setup(TabController tabController, UIFactoryBase uiFactory)
{
    this.tabControllerInstance = tabController;

    if (controller == null)
        return;

    if (isSetup)
    {
        tabController.UnregisterLogHandlers(controller.OnAddInfoLog, controller.OnAddWarningLog, controller.OnAddErrorLog);
        tabController.RegisterLogHandlers(controller.OnAddInfoLog, controller.OnAddWarningLog, controller.OnAddErrorLog);
        return;
    }

    base.Setup(tabController, uiFactory);

    // インスタンスメソッドを使用
    controller.Initialize(tabController.LogBuffer);
    tabController.RegisterLogHandlers(controller.OnAddInfoLog, controller.OnAddWarningLog, controller.OnAddErrorLog);

    isSetup = true;
}

private void OnDestroy()
{
    if (controller == null || tabControllerInstance == null)
        return;

    tabControllerInstance.UnregisterLogHandlers(controller.OnAddInfoLog, controller.OnAddWarningLog, controller.OnAddErrorLog);
}
```

#### テスト計画

1. **メモリリークテスト**:
   - シーンを10回連続で読み込み、メモリ使用量を確認
   - Profilerでイベントハンドラの参照カウントを確認

2. **機能テスト**:
   - ログの正常な表示
   - フィルタリング機能
   - カウント更新

**推定工数**: 3-4時間
**リスク**: 中（既存の動作に影響する可能性）

---

### タスク3: ProfilerPage.OnDestroy実装

**目的**: リソースの適切な解放

**実装詳細**:

**ファイル**: `Assets/XDebugger/Runtime/Scripts/Profiler/ProfilerPage.cs`

```csharp
private void OnDestroy()
{
    // RenderPipelineイベントの解除
    RenderPipelineManager.beginContextRendering -= RenderPipelineOnBeginFrameRendering;

    // Coroutineの停止
    if (endOfFrameCoroutineHandler != null)
    {
        StopCoroutine(endOfFrameCoroutineHandler);
        endOfFrameCoroutineHandler = null;
    }

    // Stopwatchの停止
    if (stopwatch != null && stopwatch.IsRunning)
    {
        stopwatch.Stop();
    }

    // バッファのクリア
    timeBuffer = null;
    totalTimeBuffer?.Clear();
    totalTimeBuffer = null;
}
```

**推定工数**: 30分
**リスク**: 低

---

## 🟡 フェーズ2: Code Quality（Week 2-3）

### タスク4: Update()処理の最適化

**目的**: 毎フレームの条件分岐を削減

**実装詳細**:

**ファイル**: `Assets/XDebugger/Runtime/Scripts/XDebugger.cs`

```csharp
private Action updateHandler;

private void Start()
{
    if (setting == null)
    {
        Debug.LogError("XDebuggerSetting is not assigned");
        enabled = false;
        return;
    }

    tabController.InitializeLogReceiver();
    tabController.Setup(setting.UIFactory, setting.TabButtonPrefab, setting.TopPageTabList, title => titleLabel.text = title);
    InitializeTrigger();

    // Update用ハンドラを設定
    SetupUpdateHandler();
}

private void SetupUpdateHandler()
{
    updateHandler = setting.TriggerMode switch
    {
        TriggerMode.DoubleTap or TriggerMode.TripleTap => UpdateTapMode,
        TriggerMode.DoubleFingerHold or TriggerMode.TripleFingerHold => UpdateHoldMode,
        _ => null
    };
}

private void Update()
{
    updateHandler?.Invoke();
}

private void UpdateTapMode()
{
    if (tapElapsedTime > setting.InputGraceTime)
        clickedCount = 0;
    else
        tapElapsedTime += Time.deltaTime;
}

private void UpdateHoldMode()
{
    bool isHoldingInput = false;

    if (PlatformUtility.IsMobile)
        isHoldingInput = HandleMobileHoldMode(setting.TriggerMode);
    else if (PlatformUtility.IsConsole)
        isHoldingInput = HandleConsoleHoldMode(setting.TriggerMode);
    else
        isHoldingInput = HandlePCHoldMode(setting.TriggerMode);

    ProcessHoldInput(isHoldingInput);
}
```

**推定工数**: 1-2時間
**リスク**: 低

---

### タスク5: 各種バグ修正

#### 5-1: float.Epsilon修正

**ファイル**: `Assets/XDebugger/Runtime/Scripts/Console/ConsoleController.cs`

```csharp
private const float ScrollThreshold = 0.01f; // 1%

protected void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
{
    if (e.Action != NotifyCollectionChangedAction.Add ||
        scrollView.normalizedPosition.y > ScrollThreshold)
    {
        return;
    }

    scrollView.normalizedPosition = Vector2.zero;
    controller.FixToLast();
}
```

**推定工数**: 10分

---

#### 5-2: PageModel.Clear()修正

**ファイル**: `Assets/XDebugger/Runtime/Scripts/Models/PageModel.cs`

```csharp
protected virtual void Clear()
{
    // コピーを作成してから破棄
    var controls = controlList.ToArray();
    controlList.Clear();
    modelList.Clear();
    IsInitialized = false;

    // GameObjectの破棄
    foreach (var control in controls)
    {
        if (control != null && control.gameObject != null)
        {
            GameObject.Destroy(control.gameObject);
        }
    }
}
```

**推定工数**: 30分

---

### タスク6: GC最適化

#### 6-1: PageModel.ModelListのキャッシング

**ファイル**: `Assets/XDebugger/Runtime/Scripts/Models/PageModel.cs`

```csharp
private ReadOnlyCollection<ControlModelBase> cachedModelList;
private bool isModelListDirty = true;

public ReadOnlyCollection<ControlModelBase> ModelList
{
    get
    {
        if (isModelListDirty || cachedModelList == null)
        {
            var sortedList = modelList.OrderBy(model => model.Priority).ToList();
            cachedModelList = new ReadOnlyCollection<ControlModelBase>(sortedList);
            isModelListDirty = false;
        }
        return cachedModelList;
    }
}

private void AddChild(ControlModelBase model)
{
    if (group is null)
        modelList.Add(model);
    else
        group.AddChild(model);

    isModelListDirty = true;
}

protected virtual void Clear()
{
    // ...既存のコード...
    cachedModelList = null;
    isModelListDirty = true;
}
```

**推定工数**: 1時間

---

#### 6-2: StringBuilder使用

**ファイル**: `Assets/XDebugger/Runtime/Scripts/Console/ConsoleController.cs`

```csharp
using System.Text;

private string GetPlainTextFromLogItemData(LogItemData data)
{
    var prefix = data.Type switch
    {
        LogType.Log => "[info]",
        LogType.Warning => "[warning]",
        LogType.Error => "[error]",
        LogType.Exception => "[exception]",
        LogType.Assert => "[assert]",
        _ => "[unknown]"
    };

    // 初期容量を設定してアロケーションを削減
    var capacity = prefix.Length + data.Contents.Length + data.StackTrace.Length + 20;
    var sb = new StringBuilder(capacity);

    sb.Append(prefix);
    sb.Append(' ');
    sb.Append(data.Contents);
    sb.Append("\nStack trace: ");
    sb.Append(data.StackTrace);

    return sb.ToString();
}
```

**推定工数**: 30分

---

### タスク7: Editorコード分離

#### Step 1: 新しいEditorファイルを作成

**新規ファイル**: `Assets/XDebugger/Editor/Scripts/XDebuggerSettingEditor.cs`

```csharp
using UnityEditor;
using UnityEngine;
using Xeon.XDebugger.Common;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Editor
{
    [CustomEditor(typeof(XDebuggerSetting))]
    public class XDebuggerSettingEditor : UnityEditor.Editor
    {
        private SerializedProperty topPageTabListProperty;

        private void OnEnable()
        {
            topPageTabListProperty = serializedObject.FindProperty("topPageTabList");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var setting = target as XDebuggerSetting;

            EditorGUI.BeginChangeCheck();

            DrawTriggerSettings(setting);
            EditorGUILayout.Space();
            DrawUISettings(setting);

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawTriggerSettings(XDebuggerSetting setting)
        {
            EditorGUILayout.LabelField("Trigger Settings", EditorStyles.boldLabel);

            setting.triggerMode = (TriggerMode)EditorGUILayout.EnumPopup("Trigger Mode", setting.triggerMode);

            if (setting.triggerMode is TriggerMode.DoubleFingerHold or TriggerMode.TripleFingerHold)
            {
                setting.holdTimeForShow = EditorGUILayout.FloatField("Hold Time for show", setting.holdTimeForShow);
                setting.holdTimeForShow = Mathf.Max(0.01f, setting.holdTimeForShow);
            }
            else if (setting.triggerMode is TriggerMode.DoubleTap or TriggerMode.TripleTap)
            {
                setting.triggerPosition = (TriggerPosition)EditorGUILayout.EnumPopup("Trigger position", setting.triggerPosition);
                setting.inputGraceTime = EditorGUILayout.FloatField("Input grace time", setting.inputGraceTime);
                setting.inputGraceTime = Mathf.Max(0.01f, setting.inputGraceTime);
            }
        }

        private void DrawUISettings(XDebuggerSetting setting)
        {
            EditorGUILayout.LabelField("UI Settings", EditorStyles.boldLabel);

            setting.uiFactory = (UIFactoryBase)EditorGUILayout.ObjectField(
                "UI Factory",
                setting.uiFactory,
                typeof(UIFactoryBase),
                false);

            setting.tabButtonPrefab = (TabButton)EditorGUILayout.ObjectField(
                "Tab Button Prefab",
                setting.tabButtonPrefab,
                typeof(TabButton),
                false);

            EditorGUILayout.PropertyField(topPageTabListProperty, true);
        }
    }
}
```

#### Step 2: XDebuggerSetting.csからEditorコードを削除

**ファイル**: `Assets/XDebugger/Runtime/Scripts/Setting/XDebuggerSetting.cs`

```csharp
using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Common
{
    public enum TriggerPosition
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    public enum TriggerMode
    {
        DoubleTap,
        TripleTap,
        DoubleFingerHold,
        TripleFingerHold,
    }

    [CreateAssetMenu(fileName = "XDebuggerSetting", menuName = "XDebugger/Setting")]
    public class XDebuggerSetting : ScriptableObject
    {
        [SerializeField]
        internal TriggerPosition triggerPosition; // internalに変更

        [SerializeField]
        internal TriggerMode triggerMode;

        [SerializeField]
        internal float inputGraceTime;

        [SerializeField]
        internal float holdTimeForShow;

        [SerializeField]
        internal UIFactoryBase uiFactory;

        [SerializeField]
        internal TabButton tabButtonPrefab;

        [SerializeField]
        internal StaticPageControl[] topPageTabList;

        public TriggerPosition TriggerPosition => triggerPosition;
        public TriggerMode TriggerMode => triggerMode;
        public float InputGraceTime => inputGraceTime;
        public float HoldTimeForShow => holdTimeForShow;
        public UIFactoryBase UIFactory => uiFactory;
        public TabButton TabButtonPrefab => tabButtonPrefab;
        public StaticPageControl[] TopPageTabList => topPageTabList;

        // #if UNITY_EDITOR セクションを完全に削除
    }
}
```

**推定工数**: 1時間

---

### タスク8: コード重複削減

#### PlatformUtilityクラスの作成

**新規ファイル**: `Assets/XDebugger/Runtime/Scripts/Common/PlatformUtility.cs`

```csharp
using UnityEngine;

namespace Xeon.XDebugger.Common
{
    /// <summary>
    /// プラットフォーム判定のユーティリティクラス
    /// </summary>
    public static class PlatformUtility
    {
        /// <summary>
        /// モバイルプラットフォーム（iOS/Android）かどうか
        /// </summary>
        public static bool IsMobile =>
            Application.isMobilePlatform ||
            Application.platform == RuntimePlatform.Android ||
            Application.platform == RuntimePlatform.IPhonePlayer;

        /// <summary>
        /// コンソールプラットフォーム（PlayStation/Xbox/Switch）かどうか
        /// </summary>
        public static bool IsConsole =>
            Application.platform == RuntimePlatform.PS4 ||
            Application.platform == RuntimePlatform.PS5 ||
            Application.platform == RuntimePlatform.XboxOne ||
            Application.platform == RuntimePlatform.GameCoreXboxSeries ||
            Application.platform == RuntimePlatform.GameCoreXboxOne ||
            Application.platform == RuntimePlatform.Switch;

        /// <summary>
        /// PCプラットフォーム（Windows/Mac/Linux）かどうか
        /// </summary>
        public static bool IsPC => !IsMobile && !IsConsole;

        /// <summary>
        /// エディタ上での実行かどうか
        /// </summary>
        public static bool IsEditor =>
#if UNITY_EDITOR
            true;
#else
            false;
#endif
    }
}
```

#### XDebugger.csの更新

**ファイル**: `Assets/XDebugger/Runtime/Scripts/XDebugger.cs`

```csharp
// 既存のメソッドを削除し、PlatformUtilityを使用
private void UpdateHoldMode()
{
    bool isHoldingInput = false;

    if (PlatformUtility.IsMobile)
    {
        isHoldingInput = HandleMobileHoldMode(setting.TriggerMode);
    }
    else if (PlatformUtility.IsConsole)
    {
        isHoldingInput = HandleConsoleHoldMode(setting.TriggerMode);
    }
    else
    {
        isHoldingInput = HandlePCHoldMode(setting.TriggerMode);
    }

    ProcessHoldInput(isHoldingInput);
}

// IsMobilePlatform(), IsConsolePlatform()メソッドを削除
```

**推定工数**: 1時間

---

## 🔵 フェーズ3: Documentation（Week 4-5）

### タスク9: XMLドキュメントの追加

**対象ファイル**:
- `IUIFactory.cs`
- `IPageModel.cs`
- `ILogDataBuffer.cs`
- `XDebugger.cs`
- `TabController.cs`
- `PageModel.cs`

**サンプル** (`IUIFactory.cs`):

```csharp
namespace Xeon.XDebugger.UI
{
    /// <summary>
    /// UIコンポーネントを生成するファクトリーインターフェース。
    /// カスタムUIを実装する場合は、このインターフェースを実装したファクトリーを作成します。
    /// </summary>
    /// <example>
    /// <code>
    /// public class CustomUIFactory : UIFactoryBase
    /// {
    ///     public override T CreateControl&lt;T&gt;(Transform parent = null)
    ///     {
    ///         // カスタム実装
    ///     }
    /// }
    /// </code>
    /// </example>
    public interface IUIFactory
    {
        /// <summary>
        /// 指定した型のコントロールを生成します。
        /// </summary>
        /// <typeparam name="T">生成するコントロールの型。ControlBaseを継承している必要があります。</typeparam>
        /// <param name="parent">親となるTransform。nullの場合はルートに配置されます。</param>
        /// <returns>生成されたコントロールのインスタンス</returns>
        /// <exception cref="InvalidOperationException">サポートされていない型が指定された場合</exception>
        T CreateControl<T>(Transform parent = null) where T : ControlBase, new();

        /// <summary>
        /// 指定した型のページを生成します。
        /// </summary>
        /// <typeparam name="T">生成するページの型。MonoBehaviourを継承している必要があります。</typeparam>
        /// <param name="parent">親となるTransform。nullの場合はルートに配置されます。</param>
        /// <returns>生成されたページのインスタンス</returns>
        /// <exception cref="InvalidOperationException">サポートされていない型が指定された場合</exception>
        T CreatePage<T>(Transform parent = null) where T : MonoBehaviour, new();

        /// <summary>
        /// 垂直レイアウトグループを生成します。
        /// </summary>
        /// <param name="parent">親となるTransform</param>
        /// <returns>生成されたContentGroupのインスタンス</returns>
        ContentGroup CreateVerticalGroup(Transform parent = null);

        /// <summary>
        /// 水平レイアウトグループを生成します。
        /// </summary>
        /// <param name="parent">親となるTransform</param>
        /// <returns>生成されたContentGroupのインスタンス</returns>
        ContentGroup CreateHorizontalGroup(Transform parent = null);

        /// <summary>
        /// 無効化グループを生成します。
        /// </summary>
        /// <param name="parent">親となるTransform</param>
        /// <returns>生成されたDisableGroupのインスタンス</returns>
        DisableGroup CreateDisableGroup(Transform parent = null);

        /// <summary>
        /// 折りたたみ可能なグループを生成します。
        /// </summary>
        /// <param name="parent">親となるTransform</param>
        /// <returns>生成されたFoldingGroupのインスタンス</returns>
        FoldingGroup CreateFoldingGroup(Transform parent = null);
    }
}
```

**推定工数**: 4-6時間

---

### タスク10: README.mdの更新

**新規セクション追加**:

1. **インストール方法**
2. **クイックスタート**
3. **API リファレンス**
4. **カスタマイズガイド**
5. **トラブルシューティング**
6. **変更履歴**

**推定工数**: 2-3時間

---

## 🧪 テスト計画

### ユニットテスト

**対象**:
- `CircularBuffer<T>`
- `LogItemBuffer`
- `PlatformUtility`

**テストフレームワーク**: Unity Test Framework

**テストケース例** (`CircularBufferTests.cs`):

```csharp
using NUnit.Framework;
using Xeon.Common.FlyweightScrollView.Model;

namespace Xeon.XDebugger.Tests
{
    public class CircularBufferTests
    {
        [Test]
        public void PushBack_WhenNotFull_AddsItem()
        {
            var buffer = new CircularBuffer<int>(3);
            buffer.PushBack(1);

            Assert.AreEqual(1, buffer.Count);
            Assert.AreEqual(1, buffer[0]);
        }

        [Test]
        public void PushBack_WhenFull_OverwritesOldest()
        {
            var buffer = new CircularBuffer<int>(3);
            buffer.PushBack(1);
            buffer.PushBack(2);
            buffer.PushBack(3);
            buffer.PushBack(4); // Overwrites 1

            Assert.AreEqual(3, buffer.Count);
            Assert.AreEqual(2, buffer[0]);
            Assert.AreEqual(4, buffer[2]);
        }

        [Test]
        public void Clear_RemovesAllItems()
        {
            var buffer = new CircularBuffer<int>(3);
            buffer.PushBack(1);
            buffer.PushBack(2);
            buffer.Clear();

            Assert.AreEqual(0, buffer.Count);
            Assert.IsTrue(buffer.IsEmpty);
        }
    }
}
```

**推定工数**: 6-8時間

---

### 統合テスト

**シナリオ**:
1. デバッグメニューの表示/非表示
2. ページ遷移
3. ログの記録とフィルタリング
4. プロファイラーの計測
5. シーン遷移時の動作

**推定工数**: 4-6時間

---

### パフォーマンステスト

**測定項目**:
1. メモリ使用量
2. GCアロケーション
3. フレームタイム
4. 起動時間

**ベンチマーク**:
```csharp
[Test, Performance]
public void CircularBuffer_Performance_1000Inserts()
{
    var buffer = new CircularBuffer<int>(1000);

    Measure.Method(() =>
    {
        for (int i = 0; i < 1000; i++)
        {
            buffer.PushBack(i);
        }
    })
    .WarmupCount(10)
    .MeasurementCount(100)
    .Run();
}
```

**推定工数**: 3-4時間

---

## 📦 リリース計画

### v1.1 リリース内容

**新機能**:
- 非同期API (`GetInstanceAsync()`)
- プラットフォームユーティリティ

**改善**:
- メモリリーク修正
- パフォーマンス最適化
- コード品質向上

**破壊的変更**:
- なし（`Instance` プロパティは非推奨だが削除しない）

**移行ガイド**:
```csharp
// 旧バージョン
XDebugger.Instance.Show();

// 新バージョン（推奨）
var debugger = await XDebugger.GetInstanceAsync();
debugger?.Show();

// 新バージョン（互換性維持）
XDebugger.Instance.Show(); // 警告が出るが動作する
```

---

### リリースチェックリスト

- [ ] すべてのユニットテストが通過
- [ ] 統合テストが通過
- [ ] パフォーマンステストで劣化がない
- [ ] ドキュメントが更新されている
- [ ] CHANGELOGが更新されている
- [ ] バージョン番号が更新されている
- [ ] サンプルシーンが動作する
- [ ] 各プラットフォームでビルドが通る

---

## 📊 進捗追跡

### KPI

| 指標 | 現在 | 目標 |
|------|------|------|
| コードカバレッジ | 0% | 60% |
| 静的解析エラー | ? | 0 |
| メモリリーク | 2件 | 0件 |
| GCアロケーション/フレーム | ? | < 100B |
| XMLドキュメント率 | 10% | 80% |

---

## 🎯 成功基準

### Phase 1
- [ ] メモリリークが解消されている
- [ ] Addressables読み込みがブロックしない
- [ ] すべてのリソースが適切に解放される

### Phase 2
- [ ] GCアロケーションが50%削減
- [ ] 静的解析エラーがゼロ
- [ ] コードの重複が削減されている

### Phase 3
- [ ] 公開APIの80%にドキュメントがある
- [ ] ユニットテストのカバレッジが60%以上
- [ ] サンプルコードが充実している

---

## 📞 サポート

質問や問題がある場合は、プロジェクトのIssueトラッカーで報告してください。

**作成者**: Claude (AI Assistant)
**最終更新**: 2026-01-19
