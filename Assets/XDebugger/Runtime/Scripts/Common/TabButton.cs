using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Xeon.XDebugger.Common
{

    public class TabButton : MonoBehaviour
    {
        [SerializeField]
        private Toggle toggle;
        [SerializeField]
        private Image icon;
        [SerializeField]
        private TMP_Text label;

        public Toggle Toggle => toggle;

        public void Setup(string tabName, Sprite sprite = null)
        {
            label.text = tabName;
            if (sprite != null)
            {
                icon.sprite = sprite;
                icon.gameObject.SetActive(true);
            }
            else
            {
                icon.gameObject.SetActive(false);
            }
        }
    }
}
