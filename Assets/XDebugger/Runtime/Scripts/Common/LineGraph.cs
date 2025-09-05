using UnityEngine;
using UnityEngine.UI;
using Xeon.Common;

[RequireComponent(typeof(CanvasRenderer))]
public class LineGraph : MaskableGraphic
{
    [SerializeField]
    private float thicness = 1f;
    [SerializeField, Tooltip("thicnessの何倍までマイターを許容するか")]
    private float miterLimit = 4f;

    [SerializeField]
    private float min = 0f;
    [SerializeField]
    private float max = 100f;

    private DoubleCircularBuffer values;

    public double MaxValue => values.Max;

    private float halfWidth => thicness * 0.5f;

    public void Initialize(int bufferSize)
    {
        values = new DoubleCircularBuffer(bufferSize);
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

    /// <summary>
    /// 値を直接設定する
    /// </summary>
    /// <param name="newValues"></param>
    public void SetValues(DoubleCircularBuffer newValues)
    {
        values = newValues;
        SetVerticesDirty();
    }

    public DoubleCircularBuffer GetValues() => values;

    /// <summary>
    /// 値を追加する
    /// </summary>
    /// <param name="value"></param>
    public void AddValue(double value, bool updateVertices = true)
    {
        values.PushBack(value);
        if (updateVertices)
            SetVerticesDirty();
    }

    /// <summary>
    /// 値をクリアする
    /// </summary>
    public void ClearVertices()
    {
        values.Clear();
        SetVerticesDirty();
    }

    /// <summary>
    /// rectTransform.rectに収まるようにデータを補正する
    /// </summary>
    /// <returns></returns>
    private float[] NormzliedValues()
    {
        var result = new float[values.Count];
        var height = max - min;
        for (var index = 0; index < values.Count; index++)
        {
            var percent = (values[index] - min) / height;
            result[index] = Mathf.Lerp(rectTransform.rect.yMin + halfWidth, rectTransform.rect.yMax - halfWidth, (float)percent);
        }
        return result;
    }

    /// <summary>
    /// 頂点の作成
    /// </summary>
    /// <param name="vh"></param>
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (this.values == null || this.values.Count <= 1)
            return;

        var values = NormzliedValues();
        var stepX = rectTransform.rect.width / (values.Length - 1f);
        var offsetX = rectTransform.rect.xMin;

        var prev = Vector2.zero;
        var prevBebel = false;

        var vertexIndex = 0;
        for (var index = 0; index < values.Length; index++)
        {
            var current = new Vector2(stepX * index + offsetX, values[index]);
            var isBebel = false;
            if (index == 0)
            {
                ProcessStart(vh, stepX, current, values[1]);
                prev = current;
                continue;
            }
            if (index == values.Length - 1)
            {
                ProcessEnd(vh, current, prev);
                AddTriangle(vh, vertexIndex, vertexIndex + 1, vertexIndex + 2);
                AddTriangle(vh, vertexIndex + 2, vertexIndex + 1, vertexIndex + 3);
                continue;
            }

            var next = new Vector2(stepX * (index + 1) + offsetX, values[index + 1]);
            var (top, bottom, tmp) = GetJoinVertices(prev, current, next);
            AddVertices(vh, top, bottom);

            if (tmp.HasValue)
            {
                AddVertex(vh, tmp.Value);
                isBebel = true;
            }

            AddTriangles(vh, vertexIndex, (current - prev).normalized, isBebel, prevBebel);
            vertexIndex += isBebel ? 3 : 2;

            prev = current;
            prevBebel = isBebel;
        }
    }

    /// <summary>
    /// 始点の処理
    /// </summary>
    /// <param name="vh"></param>
    /// <param name="stepX"></param>
    /// <param name="current"></param>
    private void ProcessStart(VertexHelper vh, float stepX, Vector2 current, float nextHeight)
    {
        var next = new Vector2(stepX, nextHeight);
        var direction = (next - current).normalized;
        var (top, bottom) = GetEndCap(current, direction);
        AddVertices(vh, top, bottom);
    }

    /// <summary>
    /// 終点の処理
    /// </summary>
    /// <param name="vh"></param>
    /// <param name="current"></param>
    /// <param name="prev"></param>
    private void ProcessEnd(VertexHelper vh, Vector2 current, Vector2 prev)
    {
        var direction = (current - prev).normalized;
        var (top, bottom) = GetEndCap(current, direction);
        AddVertices(vh, top, bottom);
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
        var normal = GetNormal(direction);
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
    private void AddVertices(VertexHelper vh, params Vector2[] positions)
    {
        foreach (var position in positions)
            AddVertex(vh, position);
    }

    /// <summary>
    /// 頂点を追加する
    /// </summary>
    /// <param name="vh"></param>
    /// <param name="position"></param>
    private void AddVertex(VertexHelper vh, Vector2 position)
    {
        var vert = UIVertex.simpleVert;
        vert.position = new Vector3(position.x, position.y);
        vert.color = color;
        vh.AddVert(vert);
    }

    /// <summary>
    /// 面を追加する
    /// </summary>
    /// <param name="vh"></param>
    /// <param name="index"></param>
    /// <param name="direction"></param>
    /// <param name="isBebel"></param>
    /// <param name="prevIsBebel"></param>
    private void AddTriangles(VertexHelper vh, int index, Vector2 direction, bool isBebel, bool prevIsBebel)
    {
        if (isBebel)
        {
            AddTriangle(vh, index, index + 1, index + 2);
            AddTriangle(vh, index + 2, index + 1, index + 3);
            AddTriangle(vh, index + 2, index + 4, index + 3);
            return;
        }

        if (direction.y >= 0 || !prevIsBebel)
        {
            AddTriangle(vh, index, index + 1, index + 2);
            AddTriangle(vh, index + 2, index + 1, index + 3);
            return;
        }

        AddTriangle(vh, index, index + 1, index + 2);
        AddTriangle(vh, index + 3, index, index + 2);
    }

    /// <summary>
    /// 三角ポリゴンを追加する
    /// </summary>
    /// <param name="vh"></param>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    private void AddTriangle(VertexHelper vh, int a, int b, int c)
    {
        vh.AddTriangle(a, b, c);
    }
}
