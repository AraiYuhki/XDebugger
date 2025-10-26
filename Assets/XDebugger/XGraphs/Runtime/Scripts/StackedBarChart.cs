using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Xeon.Common
{
    [RequireComponent(typeof(CanvasRenderer))]

    public class StackedBarChart : MaskableGraphic
    {
        [SerializeField]
        private RectOffset padding = new RectOffset();
        
        [SerializeField]
        private float min = 0f; // 最小値
        
        [SerializeField]
        private float max = 100f; // 最大値
        
        [SerializeField]
        private Color[] colors = new Color[0]; // セグメント色
        
        [SerializeField]
        private BarGraphMarker markerPrefab; // マーカーPrefab

        [SerializeField, HideInInspector]
        private List<BarGraphMarker> markers = new(); // 実体化済みマーカー

        [SerializeField]
        private List<BarGraphMarkerData> markerDataList = new(); // マーカー定義

        [SerializeField, Tooltip("Seriesを直接割り当ててスタックデータを生成する場合に使用します")]
        private List<Series> seriesBinding = new();

        [SerializeField, Tooltip("LegendからSeriesを解決する場合に参照するLegend")]
        private Legend legendSource;

        [SerializeField, Tooltip("Legendから取得するSeries名。空の場合は全Seriesを使用します")]
        private List<string> legendSeriesNames = new();

        private CircularBuffer<IStackedBarItemData> values; // 値バッファ

        private bool seriesBindingActive = false;
        private bool seriesBufferDirty = false;
        private readonly List<Series> resolvedSeries = new();

        // 最適化用キャッシュ/フラグ
        private bool geometryDirty = true; // 頂点再生成必要
        private bool rangeDirty = true;    // min/max/Rect変更でY変換再計算必要
        private bool colorDirty = true;    // colors または 本体color 変更

        private float scale;  // 値→高さ係数 ( (height)/(max-min) )
        private float baseY;  // y = baseY + value * scale
        private Color[] premultipliedColors; // colors * graphicColor
        private Color lastGraphicColor;      // 直前のGraphic色

        private float width => rectTransform.rect.width - padding.horizontal;

        //================ public API =================
        public void Initialize(CircularBuffer<IStackedBarItemData> buffer, Color[] colors)
        {
            values = buffer;
            this.colors = colors;
            DisableSeriesBinding();
            geometryDirty = true;
            rangeDirty = true;
            colorDirty = true;
            SetVerticesDirty();
        }

        public void SetMax(float max)
        {
            this.max = max;
            rangeDirty = true;
            geometryDirty = true; // スケール変化で再生成
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

        public void SetValues(CircularBuffer<IStackedBarItemData> values)
        {
            this.values = values;
            DisableSeriesBinding();
            geometryDirty = true;
            SetVerticesDirty();
        }

        public void AddValue(IStackedBarItemData item)
        {
            DisableSeriesBinding();
            values.PushBack(item);
            geometryDirty = true;
            SetVerticesDirty();
        }

        public void ClearVertices()
        {
            if (values != null)
                values.Clear();
            if (seriesBindingActive)
            {
                seriesBufferDirty = true;
                BuildSeriesBufferIfDirty();
            }
            geometryDirty = true;
            SetVerticesDirty();
        }

        public void SetSeries(IEnumerable<Series> newSeries)
        {
            seriesBinding.Clear();
            if (newSeries != null)
                seriesBinding.AddRange(newSeries);

            seriesBindingActive = seriesBinding.Count > 0;
            if (seriesBindingActive)
            {
                legendSource = null;
                legendSeriesNames.Clear();
                seriesBufferDirty = true;
                geometryDirty = true;
                rangeDirty = true;
                colorDirty = true;
                SetVerticesDirty();
            }
            else
            {
                DisableSeriesBinding();
            }
        }

        public void SetLegend(Legend legend, IEnumerable<string> seriesNames = null)
        {
            legendSource = legend;
            seriesBinding.Clear();
            legendSeriesNames.Clear();
            if (seriesNames != null)
            {
                foreach (var name in seriesNames)
                {
                    if (!string.IsNullOrEmpty(name))
                        legendSeriesNames.Add(name);
                }
            }
            seriesBindingActive = legendSource != null;
            seriesBufferDirty = seriesBindingActive;
            if (seriesBindingActive)
            {
                geometryDirty = true;
                rangeDirty = true;
                colorDirty = true;
                SetVerticesDirty();
            }
            else
            {
                DisableSeriesBinding();
            }
        }

        public void SetLegendSeriesNames(IEnumerable<string> seriesNames)
        {
            legendSeriesNames.Clear();
            if (seriesNames != null)
            {
                foreach (var name in seriesNames)
                {
                    if (!string.IsNullOrEmpty(name))
                        legendSeriesNames.Add(name);
                }
            }

            if (seriesBindingActive)
            {
                seriesBufferDirty = true;
                geometryDirty = true;
                rangeDirty = true;
                colorDirty = true;
                SetVerticesDirty();
            }
        }

        public void RefreshSeriesBinding()
        {
            if (!seriesBindingActive)
                return;

            seriesBufferDirty = true;
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
        }

        public void RemoveMarker(int index)
        {
            markerDataList.RemoveAt(index);
        }

        public void ClearMarkers()
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

        private void DisableSeriesBinding()
        {
            seriesBindingActive = false;
            seriesBufferDirty = false;
            resolvedSeries.Clear();
        }

        //================= 内部処理 =================
        private void RefreshMarkers()
        {
            ClearMarkers();
            if (markerPrefab == null) return;

            foreach (var data in markerDataList)
            {
                var marker = Instantiate(markerPrefab, transform);
                marker.Data = data;
                var pos = marker.transform.localPosition;
                pos.y = ValueToHeight(data.Value, out var inRange);
                marker.transform.localPosition = pos;
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

        private class SeriesStackedBarItemData : IStackedBarItemData
        {
            private readonly Series series;

            public SeriesStackedBarItemData(Series series)
            {
                this.series = series;
            }

            public float this[int index] => series?.GetValue(index) ?? 0f;

            public int Count => series?.Values?.Count ?? 0;

            public IEnumerator<float> GetEnumerator()
            {
                if (series?.Values == null)
                    yield break;

                for (var i = 0; i < series.Values.Count; i++)
                    yield return series.Values[i];
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private void BuildSeriesBufferIfDirty()
        {
            if (!seriesBindingActive || !seriesBufferDirty)
                return;

            resolvedSeries.Clear();

            if (seriesBinding != null && seriesBinding.Count > 0)
            {
                foreach (var series in seriesBinding)
                {
                    if (series != null)
                        resolvedSeries.Add(series);
                }
            }
            else if (legendSource != null)
            {
                if (legendSeriesNames != null && legendSeriesNames.Count > 0)
                {
                    foreach (var name in legendSeriesNames)
                    {
                        if (string.IsNullOrEmpty(name))
                            continue;

                        var series = legendSource.GetSeries(name);
                        if (series != null)
                            resolvedSeries.Add(series);
                    }
                }
                else
                {
                    var legendSeries = legendSource.SeriesList;
                    if (legendSeries != null)
                    {
                        foreach (var series in legendSeries)
                        {
                            if (series != null)
                                resolvedSeries.Add(series);
                        }
                    }
                }
            }

            if (resolvedSeries.Count == 0)
            {
                values = null;
                seriesBufferDirty = false;
                return;
            }

            var adapters = new IStackedBarItemData[resolvedSeries.Count];
            for (var i = 0; i < resolvedSeries.Count; i++)
                adapters[i] = new SeriesStackedBarItemData(resolvedSeries[i]);

            values = new CircularBuffer<IStackedBarItemData>(adapters.Length, adapters);
            seriesBufferDirty = false;
            geometryDirty = true;
            rangeDirty = true;
            colorDirty = true;
        }

        //================= Mesh生成 =================
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            BuildSeriesBufferIfDirty();
            if (values == null || values.Count == 0) return;

            if (rangeDirty)
                RecalculateRange();
            
            if (color != lastGraphicColor)
            {
                lastGraphicColor = color;
                colorDirty = true;
            }
            
            if (colorDirty)
                RecalculateColors();

            if (!geometryDirty) return;

            var barCount = values.Count;
            if (barCount <= 0) return;
            
            var stepX = width / barCount;
            var offsetX = rectTransform.rect.xMin + padding.left;

            for (var barIndex = 0; barIndex < barCount; barIndex++)
            {
                var data = values[barIndex];
                if (data == null || data.Count == 0)
                    continue;

                var cumulative = 0f;
                var left = offsetX + stepX * barIndex;
                var right = offsetX + stepX * (barIndex + 1);

                for (var segment = 0; segment < data.Count; segment++)
                {
                    var v = data[segment];
                    if (v == 0f)
                    {
                        cumulative += v;
                        continue;
                    }
                    
                    var bottom = FastValueToHeight(cumulative);
                    var top = FastValueToHeight(cumulative + v);
                    var color = GetSegmentColor(segment);
                    AddQuad(vh, left, right, bottom, top, color);
                    cumulative += v;
                }
            }
            geometryDirty = false;
        }

        private static void AddQuad(VertexHelper vh, float left, float right, float bottom, float top, Color32 color)
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

        // 値→高さ (高速パス: 正規化不要)
        private float FastValueToHeight(float value)
        {
            if (scale == 0f) return rectTransform.rect.yMin + padding.bottom; // max==min
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

        private void RecalculateColors()
        {
            if (colors == null) colors = Array.Empty<Color>();
            if (premultipliedColors == null || premultipliedColors.Length != colors.Length)
                premultipliedColors = new Color[colors.Length];
            for (var i = 0; i < colors.Length; i++)
                premultipliedColors[i] = colors[i] * this.color;
            colorDirty = false;
        }

        private Color GetSegmentColor(int index)
        {
            if (premultipliedColors == null || premultipliedColors.Length == 0)
                return this.color; // フォールバック
            if (index < premultipliedColors.Length)
                return premultipliedColors[index];
            return premultipliedColors[premultipliedColors.Length - 1]; // 足りない場合は最後
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

        //================ Unityイベント =================
        protected override void OnEnable()
        {
            base.OnEnable();
            if (!seriesBindingActive)
                seriesBindingActive = (seriesBinding != null && seriesBinding.Count > 0) || legendSource != null;
            if (seriesBindingActive)
                seriesBufferDirty = true;
            geometryDirty = true; rangeDirty = true; colorDirty = true; SetVerticesDirty();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            rangeDirty = true; geometryDirty = true; SetVerticesDirty();
        }

        public override Color color
        {
            get => base.color;
            set
            {
                base.color = value;
                colorDirty = true;
                SetVerticesDirty();
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (!Application.isPlaying)
            {
                seriesBindingActive = (seriesBinding != null && seriesBinding.Count > 0) || legendSource != null;
                if (seriesBindingActive)
                {
                    seriesBufferDirty = true;
                    BuildSeriesBufferIfDirty();
                }
                else if (testData != null)
                {
                    values = new CircularBuffer<IStackedBarItemData>(testData.Count, testData.ToArray());
                }
            }
            geometryDirty = true;
            rangeDirty = true;
            colorDirty = true;
            SetVerticesDirty();
        }
#endif

        //================= Editor用テストデータ =================
#if UNITY_EDITOR
        [Serializable]
        public class TestData : IStackedBarItemData
        {
            [SerializeField]
            private float item1;
            [SerializeField]
            private float item2;
            [SerializeField]
            private float item3;
            
            public int Count => 3;
            public float this[int index] => index switch
            {
                0 => item1,
                1 => item2,
                2 => item3,
                _ => throw new IndexOutOfRangeException()
            };

            public IEnumerator<float> GetEnumerator()
            {
                yield return item1;
                yield return item2;
                yield return item3;
            }
            
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
        [SerializeField] private List<TestData> testData = new();

        [UnityEditor.CustomEditor(typeof(StackedBarChart))]
        private class StackedBarChartEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                if (GUILayout.Button("ApplyMarkers"))
                    (target as StackedBarChart).RefreshMarkers();
                
                if (GUILayout.Button("ClearMarkers"))
                    (target as StackedBarChart).ClearMarkers();
            }
        }
#endif
    }
}
