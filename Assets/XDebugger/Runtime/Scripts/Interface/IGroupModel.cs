using System.Collections.Generic;
using UnityEngine;
using Xeon.XDebugger.Control;

namespace Xeon.XDebugger.Model
{
    public interface IGroupModel
    {
        IGroupModel Parent { get; }
        string Title { get; }
        int Priority { get; }
        IReadOnlyCollection<ControlModelBase> Children { get; }
        void AddChild(ControlModelBase model);
        ControlBase CreateControl(Transform parent);
    }
}