using System;
using System.Collections.ObjectModel;
using UnityEngine;
using Xeon.Common;
using Xeon.XDebugger.Console;

public class VirtualScrollViewTest : MonoBehaviour
{
    [SerializeField]
    private VirtualVerticalScrollView scrollView;
    [SerializeField]
    private LogItem prefab;

    private VirtualScrollViewController<LogItemData, LogItem> controller;
    private ObservableCollection<LogItemData> dataList = new();

    private int index = 0;

    private void Awake()
    {
        controller = new VirtualScrollViewController<LogItemData, LogItem>(prefab, dataList);
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
