using UnityEngine.UI;

namespace Xeon.Style
{
    public class PreferredWidth : IStyle
    {
        private float width = -1f;

        public PreferredWidth(float width) => this.width = width;
        public void Apply(LayoutElement layoutElement, Image background)
        {
            if (layoutElement == null) return;
            layoutElement.preferredWidth = width;
        }
    }

    public class MinWidth : IStyle
    {
        private float width = -1f;
        public MinWidth(float width) => this.width = width;

        public void Apply(LayoutElement layoutElement, Image background)
        {
            if (layoutElement == null) return;
            layoutElement.minWidth = width;
        }
    }

    public class FlexibleWidth : IStyle
    {
        private float width = -1f;

        public FlexibleWidth(float width)
        {
            this.width = width;
        }

        public void Apply(LayoutElement layoutElement, Image background)
        {
            if (layoutElement == null) return;
            layoutElement.flexibleWidth = width;
        }
    }
}
