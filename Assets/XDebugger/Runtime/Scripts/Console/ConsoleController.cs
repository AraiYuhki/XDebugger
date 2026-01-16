using System;
using System.Collections;
using System.Collections.Specialized;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Xeon.Common.FlyweightScrollView;

namespace Xeon.XDebugger.Console
{
    public class ConsoleController : MonoBehaviour
    {
        [SerializeField]
        private LogItem logItemPrefab;
        [SerializeField]
        private FlyweightScrollView scrollView;
        [SerializeField]
        private Transform content;
        [SerializeField]
        private TMP_Text infoCountLabel;
        [SerializeField]
        private TMP_Text warningCountLabel;
        [SerializeField]
        private TMP_Text errorCountLabel;
        [SerializeField]
        private ToggleGroup toggleGroup;
        [SerializeField]
        private TMP_Text detailLabel;
        [SerializeField]
        private Button clearButton;

        [Header("FilterToggles")]
        [SerializeField]
        private Toggle infoToggle;
        [SerializeField]
        private Toggle warningToggle;
        [SerializeField]
        private Toggle errorToggle;

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField]
        private bool enableDebugLog = false;
        [SerializeField, Range(0.1f, 5f)]
        private float debugLogInterval = 1f;

        private float debugLogElapsed = 0f;
        private int debugLogCount = 0;
#endif

        ILogDataBuffer logDataBuffer;
        private FlyweightScrollViewController<LogItemData, LogItem> controller;
        private bool isInitialized = false;

        private void Awake()
        {
            infoCountLabel.text = "0";
            warningCountLabel.text = "0";
            errorCountLabel.text = "0";
        }

        private void OnDestroy()
        {
            Cleanup();
        }

        /// <summary>
        /// 既存のリソースをクリーンアップします。
        /// </summary>
        private void Cleanup()
        {
            if (logDataBuffer != null)
                logDataBuffer.CollectionChanged -= OnCollectionChanged;

            controller?.Dispose();
            controller = null;

            // Toggleのリスナーを解除
            infoToggle.onValueChanged.RemoveAllListeners();
            warningToggle.onValueChanged.RemoveAllListeners();
            errorToggle.onValueChanged.RemoveAllListeners();
            clearButton.onClick.RemoveAllListeners();
        }

        public void Initialize(ILogDataBuffer logDataList)
        {
            // 既に初期化済みの場合は既存のリソースをクリーンアップ
            if (isInitialized)
                Cleanup();

            logDataBuffer = logDataList;
            logDataBuffer.CollectionChanged += OnCollectionChanged;
            controller = new (logItemPrefab, logDataBuffer, OnCreatedItem);
            scrollView.Setup(controller);
            scrollView.normalizedPosition = Vector3.zero;

            infoToggle.isOn = logDataList.VisibleInfo;
            warningToggle.isOn = logDataList.VisibleWarn;
            errorToggle.isOn = logDataList.VisibleError;

            infoToggle.onValueChanged.AddListener(isOn => logDataBuffer.VisibleInfo = isOn);
            warningToggle.onValueChanged.AddListener(isOn => logDataBuffer.VisibleWarn = isOn);
            errorToggle.onValueChanged.AddListener(isOn => logDataBuffer.VisibleError = isOn);
            clearButton.onClick.AddListener(ClearLog);

            OnAddInfoLog(logDataBuffer.InfoCount);
            OnAddWarningLog(logDataBuffer.WarnCount);
            OnAddErrorLog(logDataBuffer.ErrorCount);

            isInitialized = true;
        }

        public void OnAddInfoLog(int count)
        {
            infoCountLabel.text = count > 999 ? "999+" : count.ToString();
        }

        public void OnAddWarningLog(int count)
        {
            warningCountLabel.text = count > 999 ? "999+" : count.ToString();
        }

        public void OnAddErrorLog(int count)
        {
            errorCountLabel.text = count > 999 ? "999+" : count.ToString();
        }

        private void OnCreatedItem(LogItem item)
        {
            item.SetToggleGroup(toggleGroup);
            item.OnSelect += OnChangedSelectItem;
        }

        private void OnChangedSelectItem(LogItemData data)
        {
            detailLabel.text = data.ToString();
        }

        protected void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add || scrollView.normalizedPosition.y >float.Epsilon)
            {
                return;
            }
            scrollView.normalizedPosition = Vector2.zero;
            controller.FixToLast();
        }

        private void ClearLog()
        {
            logDataBuffer.Clear();
            OnAddInfoLog(0);
            OnAddWarningLog(0);
            OnAddErrorLog(0);
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (!enableDebugLog)
                return;

            debugLogElapsed += Time.deltaTime;
            if (debugLogElapsed < debugLogInterval)
                return;

            debugLogElapsed = 0f;
            debugLogCount++;

            switch (debugLogCount % 3)
            {
                case 0:
                    Debug.Log($"[Debug] Test info log #{debugLogCount}");
                    break;
                case 1:
                    Debug.LogWarning($"[Debug] Test warning log #{debugLogCount}");
                    break;
                case 2:
                    Debug.LogError($"[Debug] Test error log #{debugLogCount}");
                    break;
            }
        }
#endif

    }
}
