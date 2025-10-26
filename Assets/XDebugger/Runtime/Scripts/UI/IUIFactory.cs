using System;
using UnityEngine;
using Xeon.XDebugger.Control;

namespace Xeon.XDebugger.UI
{
    public interface IUIFactory
    {
        T CreateControl<T>(Transform parent = null) where T : ControlBase, new();
        ContentGroup CreateVerticalGroup(Transform parent = null);
        ContentGroup CreateHorizontalGroup(Transform parent = null);
        T CreatePage<T>(Transform parent = null) where T : PageControl, new();
    }
}
