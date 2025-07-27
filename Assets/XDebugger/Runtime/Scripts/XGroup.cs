using TMPro;
using UnityEngine;

namespace Xeon
{

    public class XGroup : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text titleLabel;
        [SerializeField]
        private Transform content;

        public void SetTitle(string title)
            => titleLabel.text = title;

        public Transform GetContent() => content;


    }
}
