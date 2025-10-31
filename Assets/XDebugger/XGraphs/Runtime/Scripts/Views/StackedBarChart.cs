using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Xeon.Common;
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
        private Color[] premultipliedColors = Array.Empty<Color>();

        private float scale = 1f;
        private float baseY = 0f;

        public void Initialize(MultiValueSeries buffer)
        {
            rangeDirty = true;
            colorDirty = true;
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
            if (colorDirty)
                RecalculateColors();
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
                    AddQuad(vh, left, right, bottom, top, GetSegmentColor(legendIndex));
                    cumulative += value;
                }
            }

            geometryDirty = false;
        }
        
        protected override void RecalculateColors()
        {
            if (premultipliedColors == null || premultipliedColors.Length != buffer.Count)
                premultipliedColors = new Color[buffer.Count];
            for (var index = 0; index < buffer.Count; index++)
                premultipliedColors[index] = buffer.Series[index].Color * color;
            colorDirty = false;
        }

        private Color GetSegmentColor(int index)
        {
            if (premultipliedColors == null || premultipliedColors.Length == 0)
                return color; // フォールバック
            if (index < premultipliedColors.Length)
                return premultipliedColors[index];
            return premultipliedColors[premultipliedColors.Length - 1]; // 足りない場合は最後
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

