using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Xeon.Style;

namespace Xeon
{
    public abstract class ControlBase : MonoBehaviour
    {
        [SerializeField]
        protected Image background;
        [SerializeField]
        protected TMP_Text title;
        [SerializeField]
        protected LayoutElement layoutElement;

        public virtual void Setup(string title)
            => this.title.text = title;

        public virtual void Refresh() { }

        public void ApplyStyle(params IStyle[] styles)
        {
            foreach (var style in styles)
                style.Apply(layoutElement, background);
        }
    }
}
