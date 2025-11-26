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

        ILogDataBuffer logDataBuffer;
        private FlyweightScrollViewController<LogItemData, LogItem> controller;

        private void Awake()
        {
            infoCountLabel.text = "0";
            warningCountLabel.text = "0";
            errorCountLabel.text = "0";
        }

        private void OnDestroy()
        {
            logDataBuffer.CollectionChanged -= OnCollectionChanged;
            controller.Dispose();
        }

        public void Initialize(ILogDataBuffer logDataList)
        {
            logDataBuffer = logDataList;
            logDataBuffer.CollectionChanged += OnCollectionChanged;
            controller = new (logItemPrefab, logDataBuffer, OnCreatedItem);
            scrollView.Setup(controller);
            scrollView.normalizedPosition = Vector3.zero;

            infoToggle.onValueChanged.AddListener(isOn => logDataBuffer.VisibleInfo = isOn);
            warningToggle.onValueChanged.AddListener(isOn => logDataBuffer.VisibleWarn = isOn);
            errorToggle.onValueChanged.AddListener(isOn => logDataBuffer.VisibleError = isOn);
            clearButton.onClick.AddListener(ClearLog);

            OnAddInfoLog(logDataBuffer.InfoCount);
            OnAddWarningLog(logDataBuffer.WarnCount);
            OnAddErrorLog(logDataBuffer.ErrorCount);
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
            if (e.Action == NotifyCollectionChangedAction.Add && scrollView.normalizedPosition.y <= float.Epsilon)
            {
                scrollView.normalizedPosition = Vector2.zero;
                controller.FixToLast();
            }
        }

        private void ClearLog()
        {
            logDataBuffer.Clear();
            OnAddInfoLog(0);
            OnAddWarningLog(0);
            OnAddErrorLog(0);
        }

    }
}
