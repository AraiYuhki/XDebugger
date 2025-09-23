using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Xeon.XDebugger.Console
{
    public class LogItem : MonoBehaviour
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

        private void Awake()
        {
            toggle.onValueChanged.RemoveListener(OnChangedToggle);
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
