using System;
using UnityEngine;

namespace Xeon.XGraph.Model
{
    /// <summary>
    /// 判例構造体
    /// </summary>
    [Serializable]
    public struct Legend
    {
        [SerializeField] private string name;
        [SerializeField] private Color color;

        public string Name => name;
        public Color Color => color;
        
        public Legend(string name, Color color)
        {
            this.name = name;
            this.color = color;
        }

        public Legend(ISeries series)
        {
            name = series.Name;
            color = series.Color;
        }
    }
}
