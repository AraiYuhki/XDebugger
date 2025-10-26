using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Xeon.Common
{
    /// <summary>
    /// グラフの凡例を管理するコンポーネント。
    /// </summary>
    public class Legend : MonoBehaviour
    {
        [SerializeField]
        private List<Series> seriesList = new();

        public IReadOnlyList<Series> SeriesList => seriesList;

        public void SetSeries(IEnumerable<Series> newSeries)
        {
            seriesList.Clear();
            if (newSeries == null)
                return;

            seriesList.AddRange(newSeries);
        }

        public Series GetSeries(string name)
        {
            return seriesList.FirstOrDefault(series => series.Name == name);
        }
    }
}
