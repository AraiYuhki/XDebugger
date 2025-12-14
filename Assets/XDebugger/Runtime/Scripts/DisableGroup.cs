using UnityEngine;

namespace Xeon.XDebugger
{
    public class DisableGroup : ContentGroup
    {
        [SerializeField]
        private GameObject disablePanel;

        [SerializeField]
        private bool isDisable = false;

        public void SetDisable(bool disabled)
        {
            isDisable = disabled;
            disablePanel.SetActive(disabled);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (disablePanel == null)
                return;
            disablePanel.SetActive(isDisable);
        }
#endif
    }
}
