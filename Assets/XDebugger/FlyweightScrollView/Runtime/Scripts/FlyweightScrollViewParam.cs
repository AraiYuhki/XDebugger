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
        [SerializeField]
        private bool useGridLayout;
        [SerializeField]
        private Vector2Int gridConstraint = Vector2Int.one;
        [SerializeField]
        private Vector2 gridSpacing = Vector2.zero;

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
        public bool UseGridLayout => useGridLayout;
        public Vector2Int GridConstraint
        {
            get
            {
                var columns = Mathf.Max(1, gridConstraint.x);
                var rows = Mathf.Max(1, gridConstraint.y);
                return new Vector2Int(columns, rows);
            }
        }
        public int GridColumnCount => Mathf.Max(1, gridConstraint.x);
        public int GridRowCount => Mathf.Max(1, gridConstraint.y);
        public Vector2 GridSpacing => gridSpacing;
    }
}
