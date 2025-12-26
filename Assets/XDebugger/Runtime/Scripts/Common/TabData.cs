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
        public string Title => tabContent?.Title ?? string.Empty;

        public void Initialize(Action<TabData> onTabChanged = null)
        {
            tabButton.OnValueChanged += (isActive) =>
            {
                tabContent.SetActive(isActive);
                if (isActive)
                {
                    onTabChanged?.Invoke(this);
                }
            };
        }
    }
}
