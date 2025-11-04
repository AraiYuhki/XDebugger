using System;
using UnityEngine;

namespace Xeon.XGraph.Model
{
    /// <summary>
    /// 系列データのインターフェース
    /// </summary>
    public interface ISeries
    {
        string Name { get; }
        Color Color { get; }

        event Action OnChangedColor;
    }
}