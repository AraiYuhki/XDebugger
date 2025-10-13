using System.Collections.ObjectModel;
using UnityEngine;
using Xeon.Common;
using Xeon.XDebugger.Console;

public class FlyweightScrollViewTest : MonoBehaviour
{
    [SerializeField]
    private FlyweightVerticalScrollView scrollView;
    [SerializeField]
    private LogItem prefab;

    private FlyweightScrollViewController<LogItemData, LogItem> controller;
    private ObservableCollection<LogItemData> dataList = new();

    private int index = 0;

    private void Awake()
    {
        var adapter = new FlyweightScrollViewDataAdapter<LogItemData>(dataList);
        controller = new FlyweightScrollViewController<LogItemData, LogItem>(prefab, adapter);
        scrollView.Setup(controller);
    }

    private void Update()
    {
        if (index > 100)
            return;
        var data = new LogItemData(LogType.Log, $"Test{index}", $"StackTrace{index}", index);
        index++;
        dataList.Add(data);
    }
}
