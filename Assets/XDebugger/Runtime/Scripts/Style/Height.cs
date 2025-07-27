using UnityEngine.UI;

namespace Xeon.Style
{
    public class PreferredHeight : IStyle
    {
        private float height = -1;

        public PreferredHeight(float height) => this.height = height;

        public void Apply(LayoutElement layoutElement, Image background)
        {
            if (layoutElement == null) return;
            layoutElement.preferredHeight = height;
        }
    }

    public class MinHeight : IStyle
    {
        private float height = -1f;

        public MinHeight(float height) => this.height = height;

        public void Apply(LayoutElement layoutElement, Image background)
        {
            if (layoutElement == null) return;
            layoutElement.minHeight = height;
        }
    }

    public class FlexibleHeight : IStyle
    {
        private float height = -1f;

        public FlexibleHeight(float height) => this.height = height;

        public void Apply(LayoutElement layoutElement, Image background)
        {
            if (layoutElement == null) return;
            layoutElement.flexibleHeight = height;
        }
    }
}
