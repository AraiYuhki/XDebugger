using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Xeon.XGraph.Model;

namespace Xeon.XGraph.View
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class LineChart : ChartBase
    {
        [SerializeField]
        private float thicness = 1f;
        [SerializeField, Tooltip("thicnessの何倍までマイターを許容するか")]
        private float miterLimit = 4f;

        [SerializeField]
        private float min = 0f;
        [SerializeField]
        private float max = 100f;

        [SerializeField]
        private MultiValueSeries buffer = new();

        private float halfWidth => thicness * 0.5f;

        private float[,] normalizedValues = new float[0, 0];

        private List<UIVertex> vertices;
        private List<int> indices;

        public void Initialize(MultiValueSeries series) => SetSeries(series);

        public void SetMax(float max)
        {
            this.max = max;
            geometryDirty = true;
            SetVerticesDirty();
        }

        public void SetMin(float min)
        {
            this.min = min;
            geometryDirty = true;
            SetVerticesDirty();
        }

        public void SetValues(MultiValueSeries newValues) => SetSeries(newValues);

        public void SetSeries(MultiValueSeries newSeries)
        {
            FinalizeSeries(buffer);
            buffer = newSeries;
            InitializeSeries(buffer);
            buffer?.SetColor(color);
            geometryDirty = true;
            SetVerticesDirty();
        }

        protected override void OnEnable()
        {
            buffer?.SetColor(color);
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            buffer?.SetColor(color);
            base.OnDisable();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (buffer == null || buffer.Count <= 0 || buffer.Series == null)
                return;

            var legendCount = buffer.Count;
            if (legendCount <= 0)
                return;

            var dataCount = GetDataCount();
            if (dataCount <= 1)
                return;

            EnsureCapacity(legendCount, dataCount);

            if (geometryDirty || vertices == null || indices == null || vertices.Count == 0)
            {
                UpdateNormalizedValues(legendCount, dataCount);
                BuildGeometry(legendCount, dataCount);
                geometryDirty = false;
            }

            if (vertices != null && indices != null && vertices.Count > 0 && indices.Count > 0)
                vh.AddUIVertexStream(vertices, indices);
        }

        private int GetDataCount()
        {
            if (buffer?.Series == null)
                return 0;

            var count = int.MaxValue;
            var hasSeries = false;
            foreach (var series in buffer.Series)
            {
                if (series == null)
                    return 0;

                hasSeries = true;
                var seriesCount = series.Count;
                count = seriesCount < count ? seriesCount : count;
            }

            if (!hasSeries || count == int.MaxValue)
                return 0;

            return count;
        }

        private void EnsureCapacity(int legendCount, int dataCount)
        {
            var bufferSize = Mathf.Max(0, legendCount * dataCount);
            var vertexCapacity = Mathf.Max(0, bufferSize * 4);
            var indexCapacity = Mathf.Max(0, bufferSize * 6);

            vertices ??= new List<UIVertex>(vertexCapacity);
            if (vertices.Capacity < vertexCapacity)
                vertices.Capacity = vertexCapacity;

            indices ??= new List<int>(indexCapacity);
            if (indices.Capacity < indexCapacity)
                indices.Capacity = indexCapacity;
        }

        private void UpdateNormalizedValues(int legendCount, int dataCount)
        {
            if (legendCount <= 0 || dataCount <= 0)
            {
                normalizedValues = new float[0, 0];
                return;
            }

            if (normalizedValues == null ||
                normalizedValues.GetLength(0) != legendCount ||
                normalizedValues.GetLength(1) != dataCount)
            {
                normalizedValues = new float[legendCount, dataCount];
            }

            var height = max - min;
            var rect = rectTransform.rect;
            var yMin = rect.yMin + padding.bottom + halfWidth;
            var yMax = rect.yMax - padding.top - halfWidth;

            if (yMax < yMin)
            {
                var center = (yMin + yMax) * 0.5f;
                yMin = yMax = center;
            }

            var hasRange = !Mathf.Approximately(height, 0f);
            var mid = (yMin + yMax) * 0.5f;

            for (var legendIndex = 0; legendIndex < legendCount; legendIndex++)
            {
                var series = buffer.Series[legendIndex];
                for (var dataIndex = 0; dataIndex < dataCount; dataIndex++)
                {
                    var value = series[dataIndex];
                    float yPosition;
                    if (hasRange)
                    {
                        var percent = (value - min) / height;
                        yPosition = Mathf.Lerp(yMin, yMax, percent);
                    }
                    else
                    {
                        yPosition = mid;
                    }

                    normalizedValues[legendIndex, dataIndex] = yPosition;
                }
            }
        }

        private void BuildGeometry(int legendCount, int dataCount)
        {
            vertices.Clear();
            indices.Clear();

            var rect = rectTransform.rect;
            var offsetX = rect.xMin + padding.left;
            var contentWidth = Mathf.Max(0f, width);
            var stepX = dataCount <= 1 ? 0f : contentWidth / (dataCount - 1f);

            for (var legendIndex = 0; legendIndex < legendCount; legendIndex++)
            {
                var seriesColor = buffer.GetSegmentColor(legendIndex, color);
                var vert = UIVertex.simpleVert;
                vert.color = seriesColor;

                var vertexIndex = vertices.Count;
                var prev = Vector2.zero;
                var prevBebel = false;

                for (var dataIndex = 0; dataIndex < dataCount; dataIndex++)
                {
                    var current = new Vector2(stepX * dataIndex + offsetX, normalizedValues[legendIndex, dataIndex]);
                    var isBebel = false;

                    if (dataIndex == 0)
                    {
                        var nextHeight = normalizedValues[legendIndex, dataIndex + 1];
                        ProcessStart(stepX, current, nextHeight, ref vert);
                        prev = current;
                        continue;
                    }

                    if (dataIndex == dataCount - 1)
                    {
                        ProcessEnd(current, prev, ref vert);
                        AddTriangle(vertexIndex, vertexIndex + 1, vertexIndex + 2);
                        AddTriangle(vertexIndex + 2, vertexIndex + 1, vertexIndex + 3);
                        continue;
                    }

                    var next = new Vector2(stepX * (dataIndex + 1) + offsetX, normalizedValues[legendIndex, dataIndex + 1]);
                    var (top, bottom, tmp) = GetJoinVertices(prev, current, next);
                    AddVertices(ref vert, top, bottom);

                    if (tmp.HasValue)
                    {
                        AddVertex(tmp.Value, ref vert);
                        isBebel = true;
                    }

                    var diffY = current.y - prev.y;

                    AddTriangles(vertexIndex, diffY, isBebel, prevBebel);
                    vertexIndex += isBebel ? 3 : 2;

                    prev = current;
                    prevBebel = isBebel;
                }
            }
        }

        protected override void OnChangedColor()
        {
            buffer?.SetColor(color);
            base.OnChangedColor();
        }

        /// <summary>
        /// 始点の処理
        /// </summary>
        /// <param name="vh"></param>
        /// <param name="stepX"></param>
        /// <param name="current"></param>
        private void ProcessStart(float stepX, Vector2 current, float nextHeight, ref UIVertex vert)
        {
            var next = new Vector2(current.x + stepX, nextHeight);
            var (top, bottom) = GetEndCap(current, next - current);
            AddVertices(ref vert, top, bottom);
        }

        /// <summary>
        /// 終点の処理
        /// </summary>
        /// <param name="vh"></param>
        /// <param name="current"></param>
        /// <param name="prev"></param>
        private void ProcessEnd(Vector2 current, Vector2 prev, ref UIVertex vert)
        {
            var (top, bottom) = GetEndCap(current, (current - prev));
            AddVertices(ref vert, top, bottom);
        }

        /// <summary>
        /// マイター結合の計算をする(フォールバックはベベル結合)
        /// </summary>
        /// <param name="prev"></param>
        /// <param name="current"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        private (Vector2 top, Vector2 bottom, Vector2? nextTop) GetJoinVertices(Vector2 prev, Vector2 current, Vector2 next)
        {
            var prevDirection = (current - prev).normalized;
            var nextDirection = (next - current).normalized;
            var prevNormal = GetNormal(prevDirection);
            var nextNormal = GetNormal(nextDirection);

            var miterDirection = (prevNormal + nextNormal).normalized;
            var dotToNextNormal = Vector2.Dot(miterDirection, nextNormal);

            if (Mathf.Abs(dotToNextNormal) < float.Epsilon)
            {
                return (current + nextNormal * halfWidth, current - nextNormal * halfWidth, null);
            }

            var miterLength = halfWidth / dotToNextNormal;

            if (Mathf.Abs(miterLength) > (halfWidth * Mathf.Max(1f, miterLimit)))
            {
                var currentNormal = prevDirection.y > 0 ? prevNormal : -prevNormal;
                var top = current + currentNormal * halfWidth;
                var bottom = current - currentNormal * halfWidth;
                var diff = top - bottom;
                diff.x *= -1;
                var tmp = bottom + diff;
                var positions = (top, bottom, tmp);
                return positions;
            }

            return (current + miterDirection * miterLength, current - miterDirection * miterLength, null);
        }

        /// <summary>
        /// 始点と終点の頂点位置の計算を行う
        /// </summary>
        /// <param name="point"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        private (Vector2 top, Vector2 bottom) GetEndCap(Vector2 point, Vector2 direction)
        {
            var normal = GetNormal(direction.normalized);
            return (point + normal * halfWidth, point - normal * halfWidth);
        }

        /// <summary>
        /// 指定したベクトルの邦船を取得する(左に90度回転させる)
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        private Vector2 GetNormal(Vector2 vector) => new Vector2(-vector.y, vector.x);

        /// <summary>
        /// 頂点を一括で追加する
        /// </summary>
        /// <param name="vh"></param>
        /// <param name="positions"></param>
        private void AddVertices(ref UIVertex vert, params Vector2[] positions)
        {
            foreach (var position in positions)
                AddVertex(position, ref vert);
        }

        /// <summary>
        /// 頂点を追加する
        /// </summary>
        /// <param name="vh"></param>
        /// <param name="position"></param>
        private void AddVertex(Vector2 position, ref UIVertex vert)
        {
            vert.position = new Vector3(position.x, position.y);
            vertices.Add(vert);
        }

        /// <summary>
        /// 面を追加する
        /// </summary>
        /// <param name="vh"></param>
        /// <param name="index"></param>
        /// <param name="direction"></param>
        /// <param name="isBebel"></param>
        /// <param name="prevIsBebel"></param>
        private void AddTriangles(int index, float diffY, bool isBebel, bool prevIsBebel)
        {
            if (isBebel)
            {
                AddTriangle(index, index + 1, index + 2);
                AddTriangle(index + 2, index + 1, index + 3);
                AddTriangle(index + 2, index + 4, index + 3);
                return;
            }

            if (diffY >= 0 || !prevIsBebel)
            {
                AddTriangle(index, index + 1, index + 2);
                AddTriangle(index + 2, index + 1, index + 3);
                return;
            }

            AddTriangle(index, index + 1, index + 2);
            AddTriangle(index + 3, index, index + 2);
        }

        /// <summary>
        /// 三角ポリゴンを追加する
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        private void AddTriangle(int a, int b, int c)
        {
            indices.Add(a);
            indices.Add(b);
            indices.Add(c);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            buffer?.SetColor(color);
        }
#endif
    }
}
