using UnityEngine.UI;

namespace Xeon.Style
{
    public interface IStyle
    {
        public void Apply(LayoutElement element, Image background);
    }
}
