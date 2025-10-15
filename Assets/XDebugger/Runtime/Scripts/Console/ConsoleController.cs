using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;
using Xeon.Common;

namespace Xeon.XDebugger.Console
{
    public class ConsoleController : MonoBehaviour
    {
        private const int LogBufferCapacity = 1000;
        private const int LogItemBufferCapacity = 100;

        [SerializeField]
        private LogItem logItemPrefab;
        [SerializeField]
        private ScrollRect scrollView;
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

        [Header("FilterToggles")]
        [SerializeField]
        private Toggle infoToggle;
        [SerializeField]
        private Toggle warningToggle;
        [SerializeField]
        private Toggle errorToggle;

        private Queue<LogItem> activeItemQueue = new();
        private int infoCount = 0;
        private int warningCount = 0;
        private int errorCount = 0;

        private CircularBuffer<LogItemData> logDataList = new(LogBufferCapacity);
        private ObjectPool<LogItem> logItemPool;

        private void Awake()
        {
            logItemPool = new ObjectPool<LogItem>(OnCreateItem, OnGetItem, OnReleaseItem, OnDestroyItem, defaultCapacity: LogItemBufferCapacity);
            Application.logMessageReceived += OnReceivedLogMessage;
            infoCountLabel.text = "0";
            warningCountLabel.text = "0";
            errorCountLabel.text = "0";
        }

        private LogItem OnCreateItem()
        {
            var item = Instantiate(logItemPrefab, content);
            item.gameObject.SetActive(false);
            item.SetToggleGroup(toggleGroup);
            item.SetOnChangedIsOn(() => OnChangedSelectItem(item));
            return item;
        }

        private void OnGetItem(LogItem logItem)
        {
            logItem.gameObject.SetActive(true);
            logItem.transform.SetAsLastSibling();
        }

        private void OnReleaseItem(LogItem logItem)
        {
            logItem.SetToggleOff();
            logItem.gameObject.SetActive(false);
        }

        private void OnDestroyItem(LogItem logItem)
        {
            Destroy(logItem.gameObject);
        }
        
        private float elapsed = 1f;
        private void Update()
        {
            elapsed -= Time.deltaTime;
            if (elapsed > 0f) return;
            switch(Random.Range(0, 3))
            {
                case 0:
                    Debug.Log($"Test log {infoCount}");
                    break;
                case 1:
                    Debug.LogWarning($"Test warning {warningCount}");
                    break;
                default:
                    Debug.LogError($"Test error {errorCount}");
                    break;
            };
            elapsed = 0.5f;
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= OnReceivedLogMessage;
        }

        private static int logIndex = 0;

        private void OnReceivedLogMessage(string condition, string stackTrace, LogType type)
        {
            var data = new LogItemData(type, condition, stackTrace, logIndex, true);
            logIndex++;
            logDataList.PushFront(data);

            if (activeItemQueue.Count >= LogItemBufferCapacity)
            {
                var releaseItem = activeItemQueue.Dequeue();
                logItemPool.Release(releaseItem);
            }
            var logItem = logItemPool.Get();
            logItem.Bind(data);
            activeItemQueue.Enqueue(logItem);
            scrollView.normalizedPosition = Vector2.zero;
            switch (type)
            {
                case LogType.Log:
                    infoCount++;
                    infoCountLabel.text = infoCount > 999 ? "999+" : infoCount.ToString();
                    break;
                case LogType.Warning:
                    warningCount++;
                    warningCountLabel.text = warningCount > 999 ? "999+" : warningCount.ToString();
                    break;
                default:
                    errorCount++;
                    errorCountLabel.text = errorCount > 999 ? "999+" : errorCount.ToString();
                    break;
            }
        }

        public void Clear()
        {
            while(activeItemQueue.TryDequeue(out var result))
                logItemPool.Release(result);
            logDataList.Clear();
        }

        private void OnChangedSelectItem(LogItem item)
        {
            if (!toggleGroup.AnyTogglesOn())
            {
                detailLabel.text = string.Empty;
                return;
            }
            detailLabel.text = item.Data.ToString();
        }

    }
}
