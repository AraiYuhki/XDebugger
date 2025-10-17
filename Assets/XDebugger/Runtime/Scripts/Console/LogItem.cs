using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Xeon.Common;

namespace Xeon.XDebugger.Console
{
    public class LogItem : MonoBehaviour, IBindable<LogItemData>
    {
        [SerializeField]
        private Image icon;
        [SerializeField]
        private TMP_Text message;
        [SerializeField]
        private Toggle toggle;

        [Header("Icon sprites")]
        [SerializeField]
        private Sprite infoIcon;
        [SerializeField]
        private Sprite warningIcon;
        [SerializeField]
        private Sprite errorIcon;

        public bool IsReleased { get; set; }

        public LogItemData Data { get; private set; }

        private event Action<LogItemData> onSelect;

        public event Action<LogItemData> OnSelect
        {
            add
            {
                onSelect -= value;
                onSelect += value;
            }

            remove => onSelect -= value;
        }

        private void Awake()
        {
            toggle.onValueChanged.AddListener(OnChangedToggle);
        }

        public void SetToggleGroup(ToggleGroup group)
        {
            toggle.group = group;
        }

        public void SetToggleOff()
        {
            toggle.isOn = false;
        }

        public void Bind(LogItemData data)
        {
            icon.sprite = data.Type switch
            {
                LogType.Log => infoIcon,
                LogType.Warning => warningIcon,
                _ => errorIcon
            };
            message.text = data.Contents;
            Data = data;
        }

        private void OnChangedToggle(bool isOn)
        {
            if (!isOn) return;
            onSelect?.Invoke(Data);
        }
    }
}
