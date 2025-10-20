using System;
using UnityEngine;

namespace Xeon.Common.FlyweightScrollView
{
    [Serializable]
    public class FlyweightScrollViewParam
    {
        [SerializeField]
        private RectTransform viewPort;
        [SerializeField]
        private RectOffset padding;
        [SerializeField]
        private float spacing;
        [SerializeField]
        private bool isControlChildSize;
        [SerializeField]
        private bool isReverse;
        [SerializeField]
        private bool isAtLastSticky;

        public RectTransform ViewPort => viewPort;
        public RectOffset Padding => padding;
        public float Spacing
        {
            get => spacing;
            set => spacing = value;
        }
        public bool IsControlChildSize => isControlChildSize;
        public bool IsReverse
        {
            get => isReverse;
            set => isReverse = value;
        }
        public bool IsAtLastSticky => isAtLastSticky;
    }
}
