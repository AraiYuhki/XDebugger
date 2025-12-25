using System;
using UnityEngine;
using Xeon.XDebugger.Control;

namespace Xeon.XDebugger.Common
{
    [Serializable]
    public class TabData
    {
        [SerializeField]
        private TabButton tabButton;
        [SerializeField]
        private StaticPageControl tabContent;

        public TabButton TabButton => tabButton;
        public StaticPageControl Content => tabContent;

        public void Initialize()
        {
            tabButton.OnValueChanged += tabContent.SetActive;
        }
    }
}
