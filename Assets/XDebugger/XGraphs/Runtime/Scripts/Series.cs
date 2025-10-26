using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Xeon.Common
{
    /// <summary>
    /// グラフ間で共通利用可能なデータシリーズ。
    /// </summary>
    [Serializable]
    public class Series
    {
        [SerializeField]
        private string name = string.Empty;

        [SerializeField]
        private Color color = Color.white;

        [SerializeField]
        private List<float> values = new();

        [SerializeField]
        private List<Vector2> points = new();

        public string Name => name;

        public Color Color => color;

        public IReadOnlyList<float> Values => values;

        public IReadOnlyList<Vector2> Points => points;

        public void SetValues(IEnumerable<float> newValues)
        {
            values.Clear();
            if (newValues == null)
                return;

            values.AddRange(newValues);
        }

        public void SetPoints(IEnumerable<Vector2> newPoints)
        {
            points.Clear();
            if (newPoints == null)
                return;

            points.AddRange(newPoints);
        }

        public float SumValues()
        {
            return values.Sum();
        }

        public float GetValue(int index)
        {
            if (index < 0 || index >= values.Count)
                return 0f;

            return values[index];
        }
    }
}
