using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class LineRendererForUGUI : MaskableGraphic
{
    [SerializeField] private Sprite sprite;
    [SerializeField] private Vector2[] points = new Vector2[0]; // ローカル座標系になるので注意
    [SerializeField] private float thicness = 1f;
    [SerializeField] private float miterLimit = 4f;

    public override Texture mainTexture => sprite == null ? null : sprite.texture;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        // 座標が線を引くのに足りない場合は処理を行わない
        if (points == null || points.Length < 2)
            return;

        var vertexIndex = 0;
        var halfThickness = thicness / 2f;
        var vertex = UIVertex.simpleVert;
        vertex.color = color;
        for (var index = 0; index < points.Length; index++)
        {
            // 先頭
            if (index == 0)
            {
                ProcessStartPoint(vh, index, halfThickness);
                continue;
            }

            // 終端
            if (index == points.Length - 1)
            {
                ProcessEndPoint(vh, index, vertexIndex, halfThickness);
                continue;
            }
            
            // 端以外の処理
            var prevPoint = points[index - 1];
            var currentPoint = points[index];
            var nextPoint = points[index + 1];
            var prevDirection = (currentPoint - prevPoint).normalized;
            var prevNormal = new Vector2(prevDirection.y, -prevDirection.x);
            var nextDirection = (nextPoint - currentPoint).normalized;
            var nextNormal = new Vector2(nextDirection.y, -nextDirection.x);

            var miterDirection = (prevNormal + nextNormal).normalized; // マイター方向の算出

            // マイター突出長の算出
            var dotToNextNormal = Vector2.Dot(miterDirection, nextNormal);
            var miterLength = halfThickness / dotToNextNormal; 

            // 使用するのはマイター方向とマイター突出長を使用して頂点位置を算出する
            vertex.position = currentPoint + miterDirection * miterLength;
            vh.AddVert(vertex);

            vertex.position = currentPoint - miterDirection * miterLength;
            vh.AddVert(vertex);
            vh.AddTriangle(vertexIndex, vertexIndex + 2, vertexIndex + 1);
            vh.AddTriangle(vertexIndex + 1, vertexIndex + 2, vertexIndex + 3);

            // 中間点は共通で使用するので、2つずつインデックスを進める
            vertexIndex += 2;
        }
    }

    private void ProcessStartPoint(VertexHelper vh, int index, float halfThickness)
    {
        var currentPoint = points[index];
        var nextPoint = points[index + 1];
        var direction = (nextPoint - currentPoint).normalized;
        var normal = new Vector2(direction.y, -direction.x);
        var vertex = UIVertex.simpleVert;
        vertex.color = color;
        vertex.position = currentPoint + normal * halfThickness;
        vh.AddVert(vertex);

        vertex.position = currentPoint - normal * halfThickness;
        vh.AddVert(vertex);
    }

    private void ProcessEndPoint(VertexHelper vh, int index, int vertexIndex, float halfThickness)
    {
        var currentPoint = points[index];
        var prevPoint = points[index - 1];
        var direction = (currentPoint - prevPoint).normalized;
        var normal = new Vector2(direction.y, -direction.x);

        var vertex = UIVertex.simpleVert;
        vertex.position = currentPoint + normal * halfThickness;
        vh.AddVert(vertex);

        vertex.position = currentPoint - normal * halfThickness;
        vh.AddVert(vertex);
        vh.AddTriangle(vertexIndex, vertexIndex + 2, vertexIndex + 1);
        vh.AddTriangle(vertexIndex + 1, vertexIndex + 2, vertexIndex + 3);
    }
}