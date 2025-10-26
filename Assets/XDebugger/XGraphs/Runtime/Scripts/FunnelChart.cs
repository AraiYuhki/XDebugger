using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Xeon.Common
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class FunnelChart : MaskableGraphic
    {
        [SerializeField]
        private Series stages = new();

        [SerializeField]
        private float neckRatio = 0.1f;

        [SerializeField]
        private float segmentSpacing = 2f;

        [SerializeField]
        private bool invert = false;

        private readonly List<float> widthBuffer = new();

        public void SetSeries(Series newSeries)
        {
            stages = newSeries ?? new Series();
            SetVerticesDirty();
        }

        public void SetSpacing(float spacing)
        {
            segmentSpacing = Mathf.Max(0f, spacing);
            SetVerticesDirty();
        }

        public void SetInvert(bool value)
        {
            invert = value;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (stages == null || stages.Values.Count == 0)
                return;

            var rect = rectTransform.rect;
            var stageCount = stages.Values.Count;
            var height = rect.height;
            var stepY = height / stageCount;
            var spacing = Mathf.Min(segmentSpacing, stepY * 0.5f);
            var baseColor = MultiplyColor(color, stages.Color);

            var maxValue = 0f;
            for (var i = 0; i < stageCount; i++)
                maxValue = Mathf.Max(maxValue, stages.Values[i]);

            if (maxValue <= 0f)
                return;

            EnsureBuffer(widthBuffer, stageCount + 1);

            for (var i = 0; i < stageCount; i++)
            {
                var normalized = Mathf.Clamp01(stages.Values[i] / maxValue);
                widthBuffer[i] = (rect.width * 0.5f) * normalized;
            }
            widthBuffer[stageCount] = stageCount > 0 ? widthBuffer[stageCount - 1] * Mathf.Clamp01(neckRatio) : 0f;

            var centerX = rect.center.x;
            var currentY = invert ? rect.yMin : rect.yMax;

            for (var i = 0; i < stageCount; i++)
            {
                var topWidth = widthBuffer[i];
                var bottomWidth = widthBuffer[i + 1];

                var startY = currentY;
                var endY = invert ? currentY + stepY : currentY - stepY;

                var topY = invert ? endY - spacing : startY - spacing;
                var bottomY = invert ? startY + spacing : endY + spacing;

                if (topY < bottomY)
                {
                    var tmp = topY;
                    topY = bottomY;
                    bottomY = tmp;
                }

                AddTrapezoid(vh, centerX, topY, bottomY, topWidth, bottomWidth, baseColor);

                currentY = endY;
            }
        }

        private static void EnsureBuffer(List<float> buffer, int size)
        {
            if (buffer.Capacity < size)
                buffer.Capacity = size;

            if (buffer.Count < size)
            {
                var add = size - buffer.Count;
                for (var i = 0; i < add; i++)
                    buffer.Add(0f);
            }
        }

        private static void AddTrapezoid(VertexHelper vh, float centerX, float topY, float bottomY, float topHalf, float bottomHalf, Color fillColor)
        {
            var baseIndex = vh.currentVertCount;
            var vert = UIVertex.simpleVert;
            vert.color = fillColor;

            vert.position = new Vector2(centerX - topHalf, topY);
            vh.AddVert(vert);
            vert.position = new Vector2(centerX + topHalf, topY);
            vh.AddVert(vert);
            vert.position = new Vector2(centerX + bottomHalf, bottomY);
            vh.AddVert(vert);
            vert.position = new Vector2(centerX - bottomHalf, bottomY);
            vh.AddVert(vert);

            vh.AddTriangle(baseIndex, baseIndex + 1, baseIndex + 2);
            vh.AddTriangle(baseIndex, baseIndex + 2, baseIndex + 3);
        }

        private static Color MultiplyColor(Color lhs, Color rhs)
        {
            return new Color(lhs.r * rhs.r, lhs.g * rhs.g, lhs.b * rhs.b, lhs.a * rhs.a);
        }
    }
}
