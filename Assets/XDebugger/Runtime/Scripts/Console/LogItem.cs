using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
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
        [SerializeField]
        private GameObject extendGroup;
        [SerializeField]
        private TMP_Text stackTraceLabel;

        [Header("Icon sprites")]
        [SerializeField]
        private Sprite infoIcon;
        [SerializeField]
        private Sprite warningIcon;
        [SerializeField]
        private Sprite errorIcon;

        public bool IsReleased { get; set; }

        public LogItemData Data { get; private set; }

        private void Awake()
        {
        }

        public void SetToggleGroup(ToggleGroup group)
        {
            toggle.group = group;
        }

        public void SetOnChangedIsOn(Action onChanged)
        {
            toggle.onValueChanged.AddListener(_ => onChanged?.Invoke());
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

        public void Setup(string condition, string stackTrace, LogType logType)
        {
            icon.sprite = logType switch
            {
                LogType.Log => infoIcon,
                LogType.Warning => warningIcon,
                _ => errorIcon
            };
            message.text = condition;
            stackTraceLabel.text = stackTrace;
        }

        private void OnChangedToggle(bool isOn)
        {
            extendGroup.SetActive(isOn);
        }
    }
}
