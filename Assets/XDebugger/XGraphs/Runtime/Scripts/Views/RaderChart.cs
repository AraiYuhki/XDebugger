using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Xeon.XGraph.Model;


namespace Xeon.XGraph.View
{
    public class RaderChart : ChartBase
    {
        [SerializeField] private MultiValueSeries buffer = new();
        [SerializeField, Range(0f, 360f)] private float startAngle = 0f;
        [SerializeField] private float max = 0f;

        private List<UIVertex> vertices = new();
        private List<int> indices = new();

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            vertices.Clear();
            indices.Clear();

            if (buffer == null || buffer.Count <= 0)
                return;
            if (!geometryDirty)
                return;

            var legendCount = buffer.Count;
            var dataCount = buffer.First().Count;

            if (dataCount <= 0)
                return;

            var stepAngle = 2f * Mathf.PI / dataCount;
            var halfRadius = rectTransform.sizeDelta.x * 0.5f;
            var uiVertex = UIVertex.simpleVert;
            var startIndex = 0;
            for (var legendIndex = 0; legendIndex < legendCount; legendIndex++)
            {
                var series = buffer[legendIndex];
                uiVertex.color = buffer.GetSegmentColor(legendIndex, color);
                uiVertex.position = Vector3.zero;
                vertices.Add(uiVertex);
                var angle = startAngle * Mathf.Deg2Rad;
                
                for (var dataIndex = 0; dataIndex < dataCount; dataIndex++)
                {
                    var percent = (series[dataIndex] / max) * halfRadius;
                    uiVertex.position = new Vector3(Mathf.Sin(angle) * percent, Mathf.Cos(angle) * percent, 0f);
                    angle += stepAngle;
                    vertices.Add(uiVertex);
                    if (dataIndex > 0)
                    {
                        indices.Add(startIndex);
                        indices.Add(vertices.Count - 2);
                        indices.Add(vertices.Count - 1);
                    }
                }
                indices.Add(startIndex);
                indices.Add(vertices.Count - 1);
                indices.Add(vertices.Count - dataCount);
                startIndex = vertices.Count;
            }
            vh.AddUIVertexStream(vertices, indices);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            var size = rectTransform.sizeDelta;
            var minSize = Mathf.Min(size.x, size.y);
            rectTransform.sizeDelta = new Vector2(minSize, minSize);
            if (Application.isPlaying)
                return;
            buffer?.SetColor(color);
        }
#endif
    }
}
