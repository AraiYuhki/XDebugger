using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Xeon.XDebugger.Common
{
    public class TabColumn : MonoBehaviour
    {
        [SerializeField]
        private Transform content;
        [SerializeField]
        private ToggleGroup toggleGroup;
        [SerializeField]
        private TabButton tabButtonPrefab;
        [SerializeField]
        private List<TabButton> tabButtons;

        private event Action<int> onTabSelected;

        public event Action<int> OnTabSelected
        {
            add
            {
                onTabSelected -= value;
                onTabSelected += value;
            }
            remove => onTabSelected -= value;
        }

        public void AddTab(string tabName, Action<bool> onChangedToggleValue, Sprite sprite = null)
        {
            var instance = Instantiate(tabButtonPrefab, content);
            instance.Setup(tabName, sprite);
            instance.Toggle.onValueChanged.AddListener(isOn => onChangedToggleValue?.Invoke(isOn));
            instance.Toggle.group = toggleGroup;
            tabButtons.Add(instance);
        }
    }
}
