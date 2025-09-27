using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;
using Xeon.XDebugger.Console;

namespace Xeon.Common
{
    public class NewVirtualVerticalScrollView : MonoBehaviour
    {
        [SerializeField]
        private LogItem prefab;
        [SerializeField]
        private ScrollRect scrollView;
        [SerializeField]
        private RectTransform viewPort;
        [SerializeField]
        private RectTransform content;

        private VirtualScrollViewControllerBase controller;
        private Vector2 prevScrollPosition = Vector2.zero;

        private ObservableCollection<LogItemData> logData = new();

        private void Awake()
        {
            scrollView.onValueChanged.AddListener(OnChangedScrollPosition);
            prevScrollPosition = scrollView.normalizedPosition;

            var controller = new VirtualScrollViewController<LogItemData, LogItem>(logData);
            controller.Setup(prefab, viewPort, content);
            this.controller = controller;
        }

        private int index = 0;

        public void Update()
        {
            if (index > 100)
                return;
            var data = new LogItemData(LogType.Log, $"Test{index}", $"StackTrace{index}", index);
            index++;
            logData.Add(data);
        }

        private void OnChangedScrollPosition(Vector2 position)
        {
            var isDown = position.y - prevScrollPosition.y < 0;
            prevScrollPosition = position;

            controller.Update();
            if (isDown)
                controller.RepositionForDown(position.y);
            else
                controller.RepositionForUp(position.y);
        }
    }
}
