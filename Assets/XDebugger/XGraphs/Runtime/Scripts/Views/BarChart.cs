using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Xeon.Common;
using Xeon.XGraph.Model;

namespace Xeon.XGraph.View
{
    /// <summary>
    /// 棒グラフビュー
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class BarChart : MaskableGraphic
    {
        [SerializeField] private RectOffset padding = new();
        [SerializeField] private float min = 0f;
        [SerializeField] private float max = 100f;
        [SerializeField] private BarGraphMarker markerPrefab;
        [SerializeField, HideInInspector] private List<BarGraphMarker> markers = new();
        [SerializeField] private List<BarGraphMarkerData> markerDataList = new();
        
        private List<Series> buffers = new();

        private bool geometryDirty = false;
        private bool rangeDirty = false;
        private bool colorDirty = false;

        private float scale = 1f;
        private float baseY = 0f;

        private float width => rectTransform.rect.width - padding.horizontal;
        
        public void Initialize(List<Series> buffers)
        {
            this.buffers = buffers;
            foreach (var buffer in buffers)
                buffer.OnChangedCollection += OnChangedCollection;
            geometryDirty = true;
            rangeDirty = true;
            colorDirty = true;
            SetVerticesDirty();
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

        public void SetSeries(List<Series> newData)
        {
            foreach (var buffer in buffers)
                buffer.OnChangedCollection -= OnChangedCollection;
            this.buffers = newData;
            foreach (var buffer in buffers)
                buffer.OnChangedCollection += OnChangedCollection;
            geometryDirty = true;
            SetVerticesDirty();
        }

        public void SetMarkers(List<BarGraphMarkerData> newData)
        {
            markerDataList = newData;
            RefreshMarkers();
        }
        
        public void AddMarker(string label, float value)
            => markerDataList.Add(new BarGraphMarkerData(label, value));

        public void RemoveMarker(int index)
            => markerDataList.RemoveAt(index);
        
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
        
        private void UpdateMarkers()
        {
            foreach (var marker in markers)
            {
                if (marker == null || marker.Data == null)
                    continue;
                var position = marker.transform.localPosition;
                position.y = ValueToHeight(marker.Data.Value, out var inRange);
            }
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

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (buffers == null || buffers.Count == 0)
                return;

            if (rangeDirty)
                RecalculateRange();

            if (!geometryDirty)
                return;

            foreach (var series in buffers.OrderByDescending(series => series.MaxValue))
            {
                var barCount = series.Count;
                if (barCount <= 0)
                    return;
                var stepX = width / barCount;
                var offsetX = rectTransform.rect.xMin + padding.left;
                var color = series.Color * this.color;

                for (var barIndex = 0; barIndex < barCount; barIndex++)
                {
                    var data = series[barIndex];
                    var left = offsetX + stepX * barIndex;
                    var right = offsetX + stepX * (barIndex + 1);
                    var bottom = FastValueToHeight(0f);
                    var top = FastValueToHeight(data);
                    AddQuad(vh, left, right, bottom, top, color);
                }
            }

            geometryDirty = false;
        }

        private void OnChangedCollection()
        {
            geometryDirty = true;
            SetVerticesDirty();
        }

        private float ValueToHeight(float value, out bool isInRange)
        {
            if (rangeDirty)
                RecalculateRange();
            if (Mathf.Approximately(max, min))
            {
                isInRange = false;
                return FastValueToHeight(value);
            }

            isInRange = value >= min && value <= max;
            return FastValueToHeight(value);
        }

        private float FastValueToHeight(float value)
        {
            if (scale == 0f)
                return rectTransform.rect.yMin + padding.bottom;
            return baseY + value * scale;
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

        protected override void OnEnable()
        {
            base.OnEnable();
            geometryDirty = true;
            rangeDirty = true;
            colorDirty = true;
            SetVerticesDirty();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            rangeDirty = true;
            geometryDirty = true;
            SetVerticesDirty();
        }

        private static void AddQuad(VertexHelper vh, float left, float right, float bottom, float top, Color color)
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
        [SerializeField] private List<TestSeries> testData;
        protected override void OnValidate()
        {
            base.OnValidate();
        }

        [UnityEditor.CustomEditor(typeof(BarChart))]
        private class BarChartEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                if (GUILayout.Button("ApplyMarkers"))
                    (target as BarChart)?.RefreshMarkers();
                if (GUILayout.Button("ClearMarkers"))
                    (target as BarChart)?.ClearMarkers();
            }
        }
#endif
    }
}
