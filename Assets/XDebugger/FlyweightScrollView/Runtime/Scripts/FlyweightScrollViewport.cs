using System;
using UnityEngine;
using UnityEngine.UI;

namespace Xeon.Common
{
    [RequireComponent(typeof(RectTransform), typeof(RectMask2D))]
    public class FlyweightScrollViewport : MonoBehaviour
    {
        [SerializeField]
        private RectTransform rectTransform;

        private bool isDirty = false;

        public RectTransform RectTransform => rectTransform;

        public event Action OnRectTransformDimensionsChanged;

        private void OnRectTransformDimensionsChange()
        {
            isDirty = true;
        }

        private void Update()
        {
            if (!isDirty)
                return;
            OnRectTransformDimensionsChanged?.Invoke();
            isDirty = false;
        }
    }
}
