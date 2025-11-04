using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Xeon.XGraph.Model;

namespace Xeon.XGraph.View
{
    /// <summary>
    /// 円グラフクラス
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer)), ExecuteInEditMode]
    public class PieChart : ChartBase
    {
        private const float StartAngleOffset = 90f;

        [SerializeField, Range(0f, 359f)] private float startAngle = 0f;
        [SerializeField] private SingleValueSeries[] data = new SingleValueSeries[0];

        private Color[] preMultipliedColors = new Color[0];
        private List<UIVertex> vertices = new();
        private List<int> indices = new();

        public override Color Color
        {
            set
            {
                base.Color = value;
                RecalculateColors();
            }
        }

        public void SetData(params SingleValueSeries[] data)
        {
            FinalizeSeries(this.data);
            this.data = data;
            InitializeSeries(this.data);
            RecalculateColors();
            geometryDirty = true;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            vertices.Clear();
            indices.Clear();
            
            if (data == null || data.Length <= 0)
                return;
            if (!geometryDirty)
                return;

            var total = data.Sum(d => d.Value);
            var halfRadius = 0.5f * Mathf.Min(rectTransform.rect.width, rectTransform.rect.height);
            // 12時方向開始に変更 (標準の単位円で 12時は +90°)
            var startAngleDegree = StartAngleOffset - startAngle;
            var startIndex = 0;
            for (var index = 0; index < data.Length; index++)
            {
                var value = data[index].Value;
                var arcAngleDegree = 360f * value / total; // このセグメントの角度(度)
                var sliceColor = preMultipliedColors[index];
                var segments = SegmentsForArc(halfRadius, arcAngleDegree);
                var stepAngleDegree = arcAngleDegree / segments;

                var centerVertex = UIVertex.simpleVert;
                centerVertex.color = sliceColor;
                centerVertex.position = Vector3.zero;
                vertices.Add(centerVertex);
                
                // 時計回り: 角度を減少させる方向に進める (12時から右下方向へ遷移)
                for (var segment = 0; segment <= segments; segment++)
                {
                    var angleDegree = startAngleDegree - stepAngleDegree * segment;
                    var angleRadian = angleDegree * Mathf.Deg2Rad;
                    var outerVertex = UIVertex.simpleVert;
                    outerVertex.color = sliceColor;
                    outerVertex.position = new Vector3(Mathf.Cos(angleRadian) * halfRadius, Mathf.Sin(angleRadian) * halfRadius, 0f);
                    vertices.Add(outerVertex);

                    if (segment > 0)
                    {
                        indices.Add(startIndex);
                        indices.Add(vertices.Count - 2);
                        indices.Add(vertices.Count - 1);
                    }
                }

                startIndex = vertices.Count;
                startAngleDegree -= arcAngleDegree; // 次スライス開始角度を時計回り方向へ更新
            }
            vh.AddUIVertexStream(vertices, indices);
        }

        private int SegmentsForArc(float halfRadius, float angleDeg)
        {
            var scaleFactor = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);
            var radius = halfRadius * scaleFactor;
            if (radius <= 0f || angleDeg <= 0f)
                return 1; // 最低1分割

            const float maxEdgeLength = 4f; // 1辺の最大長 (調整可)
            var arcLength = 2f * Mathf.PI * radius * (angleDeg / 360f);
            var approx = Mathf.Max(1f, arcLength / maxEdgeLength);
            var segments = Mathf.Clamp(Mathf.CeilToInt(approx), 1, 512);
            return segments;
        }

        private void RecalculateColors()
        {
            if (preMultipliedColors == null || preMultipliedColors.Length != data.Length)
                preMultipliedColors = new Color[data.Length];
            for (var index = 0; index < data.Length; index++)
                preMultipliedColors[index] = data[index].Color * color;
        }
        
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (Application.isPlaying)
                return;
            RecalculateColors();
        }
#endif
    }
}
