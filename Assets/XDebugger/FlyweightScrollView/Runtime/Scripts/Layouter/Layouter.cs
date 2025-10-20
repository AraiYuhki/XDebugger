using UnityEngine;

namespace Xeon.Common.FlyweightScrollView
{
    public abstract class Layouter
    {
        protected RectTransform container;
        protected RectTransform viewPort;
        protected Vector2 itemSize;
        protected float spacing;
        protected RectOffset padding;

        public Layouter(RectTransform container, FlyweightScrollViewParam param, Vector2 itemSize)
        {
            this.container = container;
            this.itemSize = itemSize;
            viewPort = param.ViewPort;
            padding = param.Padding;
            spacing = param.Spacing;
        }

        public float Spacing
        {
            set => spacing = value;
        }
        public abstract float ItemSize { get; }
        public int GetItemCount() => GetTailIndex() + 1;
        public abstract int GetTailIndex();
        public abstract void SetItemSize(FlyweightScrollViewItemBase item);
        public abstract void UpdateContainerSize(int itemCount);
        public abstract int CalculateIndex(int itemCount, float scrollPosition);
        public abstract Vector3 GetPosition(int index);
        public abstract float GetContentSize(int itemCount);

        /// <summary>
        /// 現在のnormalizedPositionからスクロールのピクセルオフセットを算出します。
        /// </summary>
        public abstract float GetContentOffset(int itemCount, float scrollPosition);

        /// <summary>
        /// ピクセルオフセットからnormalizedPositionへ復元します。
        /// </summary>
        public abstract float GetScrollPositionFromOffset(int itemCount, float contentOffset);

    }
}
