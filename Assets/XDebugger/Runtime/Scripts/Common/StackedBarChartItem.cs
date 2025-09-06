using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Xeon.Common
{
    public class StackedBarChartItem : MonoBehaviour
    {
        [SerializeField]
        private Image barTemplate;
        [SerializeField]
        private List<Image> bars = new();

        public int Count => bars.Count;

        public void ReCreateBars(int count)
        {
            foreach (var bar in bars)
                Destroy(bar.gameObject);
            bars.Clear();
            for (var i = 0; i < count; i++)
            {
                var bar = Instantiate(barTemplate, transform);
                bar.gameObject.SetActive(true);
                bars.Add(bar);
            }
        }

        public void SetColors(params Color[] colors)
        {
            if (bars.Count != colors.Length)
                throw new ArgumentException("Colors count is not equals to bars count");
            for (var index = 0; index < colors.Length; index++)
                SetColor(index, colors[index]);
        }

        /// <summary>
        /// 指定したインデックスのバーの色を変更する
        /// インデックスが大きいほど手前のバー
        /// </summary>
        /// <param name="index"></param>
        /// <param name="color"></param>
        public void SetColor(int index, Color color)
        {
            bars[index].color = color;
        }

        public void SetValues(IEnumerable<float> values)
        {
            if (bars.Count != values.Count())
                throw new ArgumentException("Values count is not equals to bars count");

            foreach (var (value, index) in values.Select((value, index) => (value, index)))
                SetValue(index, value);
        }

        public void SetValues(params float[] values)
        {
            if (bars.Count != values.Length)
                throw new ArgumentException("Values count is not equals to bars count");
            for (var index = 0; index < values.Length; index++)
                SetValue(index, values[index]);
        }

        /// <summary>
        /// 指定したインデックスのfillAmountを変更する
        /// インデックスが大きいほど手前のバー
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void SetValue(int index, float value)
            => bars[index].fillAmount = value;
    }
}
