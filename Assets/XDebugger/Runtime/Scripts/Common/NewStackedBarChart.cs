using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Xeon.Common
{
    [RequireComponent(typeof(CanvasRenderer))]

    public class NewStackedBarChart : MaskableGraphic
    {
        [SerializeField]
        private RectOffset padding = new RectOffset();
        [SerializeField]
        private float min = 0f;
        [SerializeField]
        private float max = 100f;

        [SerializeField]
        private Color[] colors = new Color[0];

        private CircularBuffer<IStackedBarItemData> values;

        private float width => rectTransform.rect.width - padding.horizontal;
        private float height => rectTransform.rect.height - padding.vertical;

        public void Initialize(int bufferSize)
        {
            values = new CircularBuffer<IStackedBarItemData>(bufferSize);
        }

        public void Initialize(CircularBuffer<IStackedBarItemData> buffer, Color[] colors)
        {
            values = buffer;
            this.colors = colors;
            SetVerticesDirty();
        }

        public void SetMax(float max)
        {
            this.max = max;
            SetVerticesDirty();
        }

        public void SetMin(float min)
        {
            this.min = min;
            SetVerticesDirty();
        }

        public void SetValues(CircularBuffer<IStackedBarItemData> values)
        {
            this.values = values;
            SetVerticesDirty();
        }

        public void AddValue(IStackedBarItemData item, bool updateVertices = true)
        {
            values.PushBack(item);
            if (updateVertices)
                SetVerticesDirty();
        }

        public void ClearVertices()
        {
            values.Clear();
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (values == null || values.Count <= 0) return;
            if (values[0].Count != colors.Length)
                throw new InvalidOperationException("Colors length must match category count.");
            var stepX = width / values.Count;
            var offsetX = rectTransform.rect.xMin + padding.left;
            var vertexIndex = 0;

            var vertices = new List<UIVertex>();
            var indecies = new List<int>();
            for (var index = 0; index < values.Count; index++)
            {
                var item = values[index];
                var left = offsetX + stepX * index;
                var right = offsetX + stepX * (index + 1);
                var value = 0f;
                var positionY = ValueToHeight(value);
                for(var dataIndex = 0; dataIndex < item.Count; dataIndex++)
                {
                    var color = colors[dataIndex] * this.color;
                    vertices.Add(CreateVertex(new Vector2(left, positionY), color));
                    vertices.Add(CreateVertex(new Vector2(right, positionY), color));

                    value += item[dataIndex];
                    positionY = ValueToHeight(value);
                    vertices.Add(CreateVertex(new Vector2(left, positionY), color));
                    vertices.Add(CreateVertex(new Vector2(right, positionY), color));

                    indecies.Add(vertexIndex);
                    indecies.Add(vertexIndex + 1);
                    indecies.Add(vertexIndex + 2);
                    indecies.Add(vertexIndex + 1);
                    indecies.Add(vertexIndex + 2);
                    indecies.Add(vertexIndex + 3);
                    vertexIndex += 4;
                }
                vh.AddUIVertexStream(vertices, indecies);
            }
        }

        private float ValueToHeight(float value)
        {
            var normzliedValue = (value - min) / (max - min);
            return Mathf.Lerp(rectTransform.rect.yMin + padding.bottom, rectTransform.rect.yMax - padding.top, normzliedValue);
        }

        private UIVertex CreateVertex(Vector2 position, Color color)
        {
            var vert = UIVertex.simpleVert;
            vert.position = new Vector3(position.x, position.y);
            vert.color = color;
            return vert;
        }

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

            public float this[int index]
            {
                get
                {
                    return index switch
                    {
                        0 => item1,
                        1 => item2,
                        2 => item3,
                        _ => throw new IndexOutOfRangeException(),
                    };
                }
            }

            public IEnumerator<float> GetEnumerator()
            {
                yield return item1;
                yield return item2;
                yield return item3;
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [SerializeField]
        private List<TestData> testData = new();

        protected override void OnValidate()
        {
            base.OnValidate();
            if (Application.isPlaying) return;
            values = new CircularBuffer<IStackedBarItemData>(testData.Count, testData.ToArray());
        }
#endif
    }
}
