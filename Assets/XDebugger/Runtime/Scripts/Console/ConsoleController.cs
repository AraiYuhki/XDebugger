using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Xeon.Common;

namespace Xeon.XDebugger.Console
{
    public class ConsoleController : MonoBehaviour
    {
        private const int LogBufferCapacity = 1000;

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

        [Header("FilterToggles")]
        [SerializeField]
        private Toggle infoToggle;
        [SerializeField]
        private Toggle warningToggle;
        [SerializeField]
        private Toggle errorToggle;

        private int infoCount = 0;
        private int warningCount = 0;
        private int errorCount = 0;

        private CircularBuffer<LogItemData> logDataList = new(LogBufferCapacity);
        private FlyweightScrollViewController<LogItemData, LogItem> controller;

        private void Awake()
        {
            controller = new FlyweightScrollViewController<LogItemData, LogItem>(logItemPrefab, logDataList);
            scrollView.Setup(controller);
            Application.logMessageReceived += OnReceivedLogMessage;
            infoCountLabel.text = "0";
            warningCountLabel.text = "0";
            errorCountLabel.text = "0";
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
            logDataList.PushBack(data);
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
