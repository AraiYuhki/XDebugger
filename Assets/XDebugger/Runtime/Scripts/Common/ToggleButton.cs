using UnityEngine;
using UnityEngine.UI;

namespace Xeon.XDebugger.Common
{
    public class ToggleButton : MonoBehaviour
    {
        [SerializeField]
        private Toggle toggle;
        [SerializeField]
        private Image target;

        [SerializeField]
        private Color onColor;
        [SerializeField]
        private Color offColor;

        public void OnChangeToggle()
        {
            target.color = toggle.isOn ? onColor : offColor;
        }
    }
}
