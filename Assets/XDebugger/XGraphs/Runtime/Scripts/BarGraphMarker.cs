using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Xeon.Common
{
    [Serializable]
    public class BarGraphMarkerData
    {
        [SerializeField]
        private string label;
        [SerializeField]
        private float value = 0f;

        public string Label => label;
        public float Value => value;

        public BarGraphMarkerData(string label, float value)
        {
            this.label = label;
            this.value = value;
        }
    }

    public class BarGraphMarker : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text label;
        [SerializeField]
        private Image line;
        [SerializeField]
        private Image labelBackground;

        [Header("Color settings")]
        [SerializeField]
        private Color labelColor = Color.black;
        [SerializeField]
        private Color lineColor = Color.white;
        [SerializeField]
        private Color labelBgColor = new Color(1f, 1f, 1f, 0.5f);

        [SerializeField]
        private BarGraphMarkerData data;
        public BarGraphMarkerData Data 
        {
            get => data;
            set
            {
                data = value;
                label.text = data.Label;
            }
        }

        public string Label
        {
            get => label.text;
            set => label.text = value;
        }

        private void OnEnable()
        {
            if (label != null)
                label.color = labelColor;
            if (line != null)
                line.color = lineColor;
            if (labelBackground != null)
                labelBackground.color = labelBgColor;
        }

        public void SetLabelColor(Color color)
        {
            label.color = labelColor = color;
        }

        public void SetLineColor(Color color)
        {
            line.color = lineColor = color;
        }

        public void SetLabelBgColor(Color color)
        {
            labelBackground.color = labelBgColor = color;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying) return;
            OnEnable();
        }
#endif
    }
}
