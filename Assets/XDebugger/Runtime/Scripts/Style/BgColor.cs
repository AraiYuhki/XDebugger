using UnityEngine;
using UnityEngine.UI;

namespace Xeon.Style
{
    public class BgColor : IStyle
    {
        private Color color;

        public BgColor(Color color) => this.color = color;
        public void Apply(LayoutElement element, Image background)
        {
            if (background == null) return;
            background.color = color;
            background.gameObject.SetActive(color.a > 0f);
        }
    }
}
