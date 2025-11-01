using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Xeon.XGraph.Manager;
using Xeon.XGraph.Model;

namespace Xeon.XGraph.View
{
    /// <summary>
    /// 積み上げ棒グラフビュー
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class StackedBarChart : ChartBase
    {
        [SerializeField] private float min = 0f;
        [SerializeField] private float max = 100f;
        [SerializeField] private MarkerManager markerManager;
        [SerializeField] private MultiValueSeries buffer = new();

        private bool rangeDirty = false;

        private float scale = 1f;
        private float baseY = 0f;

        public override Color Color
        {
            set
            {
                buffer?.SetColor(value);
                base.Color = value;
            }
        }

        public MarkerManager MarkerManager => markerManager;

        public void Initialize(MultiValueSeries buffer)
        {
            rangeDirty = true;
            markerManager.Initialize(ValueToHeight);
            SetSeries(buffer);
        }

        public void SetMax(float max)
        {
            this.max = max;
            rangeDirty = true;
            geometryDirty = true;
            SetVerticesDirty();
            markerManager.UpdateMarkers();
        }

        public void SetMin(float min)
        {
            this.min = min;
            rangeDirty = true;
            geometryDirty = true;
            SetVerticesDirty();
            markerManager.UpdateMarkers();
        }

        public void SetSeries(MultiValueSeries newData)
        {
            FinalizeSeries(buffer);
            buffer = newData;
            InitializeSeries(buffer);
            buffer.SetColor(color);

            geometryDirty = true;
            rangeDirty = true;
            SetVerticesDirty();
        }

        private void RecalculateRange()
        {
            if (Mathf.Approximately(max, min))
            {
                scale = 0f;
                baseY = rectTransform.rect.yMin + padding.bottom;
            }
            else
            {
                var usableHeight = rectTransform.rect.height - padding.vertical;
                scale = usableHeight / (max - min);
                baseY = rectTransform.rect.yMin + padding.bottom - min * scale;
            }

            rangeDirty = false;
        }

        private float ValueToHeight(float value, out bool isInRange)
        {
            if (rangeDirty)
                RecalculateRange();
            if (Mathf.Approximately(max, min))
                isInRange = false;
            else
                isInRange = value >= min && value <= max;
            return FastValueToHeight(value);
        }

        private float FastValueToHeight(float value)
        {
            if (scale == 0f)
                return rectTransform.rect.yMin + padding.bottom;
            return baseY + value * scale;
        }

        protected override void OnRectTransformDimensionsChange()
        {
            rangeDirty = true;
            base.OnRectTransformDimensionsChange();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (buffer == null || buffer.Count == 0)
                return;

            if (rangeDirty)
                RecalculateRange();

            if (!geometryDirty)
                return;

            var legendCount = buffer.Count;
            var dataCount = buffer.Series.First().Count;
            if (dataCount == 0)
                return;

            var gaps = dataCount - 1;
            var totalGaps = gaps * Mathf.Max(0f, spacing);
            var stepX = (width - totalGaps) / dataCount;
            var offsetX = rectTransform.rect.xMin + padding.left;

            for (var dataIndex = 0; dataIndex < dataCount; dataIndex++)
            {
                var cumulative = 0f;
                var left = offsetX + dataIndex * (stepX + spacing);
                var right = left + stepX;
                for (var legendIndex = 0; legendIndex < legendCount; legendIndex++)
                {
                    var value = buffer.Series[legendIndex][dataIndex];
                    if (value == 0f)
                    {
                        cumulative += value;
                        continue;
                    }

                    var bottom = FastValueToHeight(cumulative);
                    var top = FastValueToHeight(cumulative + value);
                    AddQuad(vh, left, right, bottom, top, buffer.GetSegmentColor(legendIndex, color));
                    cumulative += value;
                }
            }

            geometryDirty = false;
        }

        protected override void OnEnable()
        {
            rangeDirty = true;
            buffer?.SetColor(color);
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            buffer?.SetColor(color);
            base.OnDisable();
        }

        private static void AddQuad(VertexHelper vh, float left, float right, float top, float bottom, Color color)
        {
            var index = vh.currentVertCount;
            var vert = UIVertex.simpleVert;
            vert.color = color;
            vert.position = new Vector2(left, bottom);
            vh.AddVert(vert);
            vert.position = new Vector2(right, bottom);
            vh.AddVert(vert);
            vert.position = new Vector2(left, top);
            vh.AddVert(vert);
            vert.position = new Vector2(right, top);
            vh.AddVert(vert);
            vh.AddTriangle(index + 0, index + 2, index + 1);
            vh.AddTriangle(index + 2, index + 3, index + 1);
        }
        
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (Application.isPlaying)
                return;
            markerManager.Initialize(ValueToHeight);
            buffer?.SetColor(color);
            rangeDirty = true;
        }
#endif
    }
}

