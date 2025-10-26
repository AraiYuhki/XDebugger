using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Xeon.Common
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class ScatterChart : MaskableGraphic
    {
        [SerializeField]
        private List<Series> series = new();

        [SerializeField]
        private float minX = 0f;

        [SerializeField]
        private float maxX = 1f;

        [SerializeField]
        private float minY = 0f;

        [SerializeField]
        private float maxY = 1f;

        [SerializeField]
        private float pointSize = 6f;

        public void SetSeries(List<Series> newSeries)
        {
            series = newSeries ?? new List<Series>();
            SetVerticesDirty();
        }

        public void SetRange(float minX, float maxX, float minY, float maxY)
        {
            this.minX = minX;
            this.maxX = maxX;
            this.minY = minY;
            this.maxY = maxY;
            SetVerticesDirty();
        }

        public void SetPointSize(float size)
        {
            pointSize = Mathf.Max(0.1f, size);
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (series == null || series.Count == 0)
                return;

            var rect = rectTransform.rect;
            var halfSize = pointSize * 0.5f;

            foreach (var data in series)
            {
                if (data == null)
                    continue;

                var finalColor = MultiplyColor(color, data.Color);
                foreach (var point in data.Points)
                {
                    if (maxX - minX == 0f || maxY - minY == 0f)
                        continue;

                    var normalizedX = Mathf.InverseLerp(minX, maxX, point.x);
                    var normalizedY = Mathf.InverseLerp(minY, maxY, point.y);

                    var position = new Vector2(Mathf.Lerp(rect.xMin, rect.xMax, normalizedX), Mathf.Lerp(rect.yMin, rect.yMax, normalizedY));
                    AddPoint(vh, position, halfSize, finalColor);
                }
            }
        }

        private void AddPoint(VertexHelper vh, Vector2 center, float halfSize, Color pointColor)
        {
            var startIndex = vh.currentVertCount;

            var vertex = UIVertex.simpleVert;
            vertex.color = pointColor;

            vertex.position = new Vector2(center.x - halfSize, center.y - halfSize);
            vh.AddVert(vertex);

            vertex.position = new Vector2(center.x - halfSize, center.y + halfSize);
            vh.AddVert(vertex);

            vertex.position = new Vector2(center.x + halfSize, center.y + halfSize);
            vh.AddVert(vertex);

            vertex.position = new Vector2(center.x + halfSize, center.y - halfSize);
            vh.AddVert(vertex);

            vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vh.AddTriangle(startIndex, startIndex + 2, startIndex + 3);
        }

        private static Color MultiplyColor(Color lhs, Color rhs)
        {
            return new Color(lhs.r * rhs.r, lhs.g * rhs.g, lhs.b * rhs.b, lhs.a * rhs.a);
        }
    }
}
