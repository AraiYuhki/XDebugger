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
        [SerializeField] private RangeValue range = new(0f, 100f);
        [SerializeField] private float spacing = 0f;
        [SerializeField] private MarkerManager markerManager = new();
        [SerializeField] private MultiValueSeries buffer = new();

        private bool rangeDirty = false;

        private float scale = 1f;
        private float baseY = 0f;

        public float Spacing
        {
            get => spacing;
            set
            {
                spacing = value;
                geometryDirty = true;
                SetVerticesDirty();
            }
        }

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
            range.Max = max;
            rangeDirty = true;
            geometryDirty = true;
            SetVerticesDirty();
            markerManager.UpdateMarkers();
        }

        public void SetMin(float min)
        {
            range.Min = min;
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
            if (range.IsApproximately)
            {
                scale = 0f;
                baseY = rectTransform.rect.yMin + padding.bottom;
            }
            else
            {
                var usableHeight = rectTransform.rect.height - padding.vertical;
                scale = usableHeight / range.Range;
                baseY = rectTransform.rect.yMin + padding.bottom - range.Min * scale;
            }

            rangeDirty = false;
        }

        private float ValueToHeight(float value, out bool isInRange)
        {
            if (rangeDirty)
                RecalculateRange();
            isInRange = range.InRange(value);
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
            var dataCount = buffer.First().Count;
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
                    var value = buffer[legendIndex][dataIndex];
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
            var size = Mathf.Min(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y);
            rectTransform.sizeDelta = Vector2.one * size;
            if (Application.isPlaying)
                return;
            markerManager.Initialize(ValueToHeight);
            buffer?.SetColor(color);
            rangeDirty = true;
        }
#endif
    }
}

