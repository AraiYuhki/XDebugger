using UnityEngine;


/// <summary>
/// ラインのジョイン（角）の計算に必要な前・現在・次の点および派生情報をまとめたコンテキスト
/// </summary>
public struct LineJoinContext
{
    /// <summary>極小長さ判定に用いる閾値</summary>
    private const float Epsilon = 1e-6f;

    /// <summary>前の点（先頭の場合は自身）</summary>
    public Vector2 Prev { get; }

    /// <summary>現在の点</summary>
    public Vector2 Current { get; }

    /// <summary>次の点（終端の場合は自身）</summary>
    public Vector2 Next { get; }

    /// <summary>前の点から現在の点へのベクトル</summary>
    public Vector2 PrevVector { get; }

    /// <summary>現在の点から次の点へのベクトル</summary>
    public Vector2 NextVector { get; }

    /// <summary>前ベクトルの正規化方向</summary>
    public Vector2 PrevDirection { get; }

    /// <summary>次ベクトルの正規化方向</summary>
    public Vector2 NextDirection { get; }

    /// <summary>前方向を左に90度回転させた法線</summary>
    public Vector2 PrevNormal { get; }

    /// <summary>次方向を左に90度回転させた法線</summary>
    public Vector2 NextNormal { get; }

    /// <summary>マイター方向（前法線 + 次法線 を正規化したもの）</summary>
    public Vector2 MiterVector => (PrevNormal + NextNormal).normalized;

    /// <summary>前方向から次方向へ左折しているか</summary>
    public bool IsLeftTurn { get; }

    /// <summary>前ベクトルの長さ</summary>
    public float PrevLength { get; }

    /// <summary>次ベクトルの長さ</summary>
    public float NextLength { get; }

    /// <summary>配列先頭かどうか</summary>
    public bool IsFirst { get; }

    /// <summary>配列終端かどうか</summary>
    public bool IsLast { get; }

    /// <summary>
    /// 指定インデックス位置の幾何情報を構築する
    /// </summary>
    /// <param name="index">対象インデックス</param>
    /// <param name="allPoints">全ポイント配列</param>
    public LineJoinContext(int index, Vector2[] allPoints)
    {
        Current = allPoints[index];
        Prev = Current;
        Next = Current;
        PrevVector = Vector2.zero;
        NextVector = Vector2.zero;
        PrevLength = 0f;
        NextLength = 0f;
        PrevDirection = Vector2.right;
        NextDirection = Vector2.right;
        PrevNormal = new Vector2(PrevDirection.y, -PrevDirection.x);
        NextNormal = new Vector2(NextDirection.y, -NextDirection.x);
        IsLeftTurn = false;
        IsFirst = index == 0;
        IsLast = index == allPoints.Length - 1;

        if (!IsFirst)
        {
            Prev = allPoints[index - 1];
            PrevVector = Current - Prev;
            PrevLength = PrevVector.magnitude;
            if (PrevLength > Epsilon)
                PrevDirection = PrevVector / PrevLength;
            PrevNormal = new Vector2(PrevDirection.y, -PrevDirection.x);
        }

        if (!IsLast)
        {
            Next = allPoints[index + 1];
            NextVector = Next - Current;
            NextLength = NextVector.magnitude;
            if (NextLength > Epsilon)
                NextDirection = NextVector / NextLength;
            NextNormal = new Vector2(NextDirection.y, -NextDirection.x);
        }

        if (!IsFirst && !IsLast)
        {
            IsLeftTurn = Cross(PrevDirection, NextDirection) > 0f;
        }
    }

    /// <summary>2次元ベクトルの外積（面積符号付き）</summary>
    private float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;
}