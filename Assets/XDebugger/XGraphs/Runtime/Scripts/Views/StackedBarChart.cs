using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Xeon.Common;
using Xeon.XGraph.Controller;
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
        [SerializeField] private BarGraphMarker markerPrefab;
        [SerializeField, HideInInspector] private List<BarGraphMarker> markers = new();
        [SerializeField] private List<BarGraphMarkerData> markerDataList = new();
        [SerializeField] private MultiValueSeries buffer = new();

        private bool rangeDirty = false;
        private readonly ColorManager colorManager = new();

        private float scale = 1f;
        private float baseY = 0f;

        public void Initialize(MultiValueSeries buffer)
        {
            rangeDirty = true;
            SetSeries(buffer);
        }

        public void SetMax(float max)
        {
            this.max = max;
            rangeDirty = true;
            geometryDirty = true;
            SetVerticesDirty();
            UpdateMarkers();
        }

        public void SetMin(float min)
        {
            this.min = min;
            rangeDirty = true;
            geometryDirty = true;
            SetVerticesDirty();
            UpdateMarkers();
        }

        public void SetSeries(MultiValueSeries newData)
        {
            FinalizeSeries(buffer);
            buffer = newData;
            InitializeSeries(buffer);
            colorManager.SetBuffer(buffer);

            rangeDirty = true;
            SetVerticesDirty();
        }

        public void SetMarkers(List<BarGraphMarkerData> newData)
        {
            markerDataList = newData;
            RefreshMarkers();
        }

        public void AddMarker(string label, float value)
        {
            markerDataList.Add(new BarGraphMarkerData(label, value));
            RefreshMarkers();
        }

        public void RemoveMarker(int index)
        {
            markerDataList.RemoveAt(index);
            RefreshMarkers();
        }

        private void ClearMarkers()
        {
            foreach (var marker in markers)
            {
                if (marker == null)
                    continue;
                if (Application.isPlaying)
                    Destroy(marker.gameObject);
                else
                    DestroyImmediate(marker.gameObject);
            }

            markers.Clear();
        }

        private void RefreshMarkers()
        {
            ClearMarkers();
            if (markerPrefab == null)
                return;
            foreach (var data in markerDataList)
            {
                var marker = Instantiate(markerPrefab, transform);
                marker.Data = data;
                var position = marker.transform.localPosition;
                position.y = ValueToHeight(data.Value, out var inRange);
                marker.transform.localPosition = position;
                marker.gameObject.SetActive(inRange);
                markers.Add(marker);
            }
        }

        private void UpdateMarkers()
        {
            foreach (var marker in markers)
            {
                if (marker == null || marker.Data == null)
                    continue;
                var position = marker.transform.localPosition;
                position.y = ValueToHeight(marker.Data.Value, out var inRange);
                marker.transform.localPosition = position;
                marker.gameObject.SetActive(inRange);
            }
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

            colorManager.RecalculateColors(color);

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
                    AddQuad(vh, left, right, bottom, top, colorManager.GetSegmentColor(legendIndex, color));
                    cumulative += value;
                }
            }

            geometryDirty = false;
        }

        protected override void OnEnable()
        {
            rangeDirty = true;
            colorManager.SetBuffer(buffer);
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            colorManager.SetBuffer(null);
            base.OnDisable();
        }

        protected override void OnChangedColor()
        {
            geometryDirty = true;
            base.OnChangedColor();
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
            colorManager.SetBuffer(buffer);
            rangeDirty = true;
        }

        [UnityEditor.CustomEditor(typeof(StackedBarChart))]
        private class StackedBarChartEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                if (GUILayout.Button("ApplyMarkers"))
                    (target as StackedBarChart)?.RefreshMarkers();
                if (GUILayout.Button("ClearMarkers"))
                    (target as StackedBarChart)?.ClearMarkers();
            }
        }
#endif
    }
}

