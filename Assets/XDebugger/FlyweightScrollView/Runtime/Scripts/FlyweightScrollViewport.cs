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

        public RectTransform RectTransform => rectTransform;

        public event Action OnRectTranformDimensionsChanged;

        private void OnRectTransformDimensionsChange()
        {
            OnRectTranformDimensionsChanged?.Invoke();
        }
    }
}
