using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Xeon.Common
{
    public class StackedBarChart : MonoBehaviour
    {
        [SerializeField]
        private StackedBarChartItem itemTemplate;
        [SerializeField]
        private List<StackedBarChartItem> items = new();
        [SerializeField, Tooltip("バーの色の設定。インデックスが大きいほど手前のバーの色")]
        private Color[] colors;

        [SerializeField]
        private float max = 100f;

        private CircularBuffer<IStackedBarItemData> values;

        public void Initialize(int capacity)
        {
            values = new(capacity);
            ReCreateItems(capacity, true);
        }

        public void Initialize(CircularBuffer<IStackedBarItemData> values, IEnumerable<Color> colors, bool isInitializeItems = false)
        {
            this.values = values;
            if (colors != null)
                this.colors = colors.ToArray();
            ReCreateItems(values.Count, isInitializeItems);
        }

        public void SetColors(params Color[] colors)
        {
            this.colors = colors;
            if (!items.Any())
                return;
            foreach (var item in items)
                item.SetColors(colors);
        }

        public void SetMax(float max, bool isUpdate = false)
        {
            this.max = max;
            if (isUpdate)
                UpdateValues();
        }

        public void AddValue(IStackedBarItemData item)
        {
            values.PushBack(item);
            UpdateValues();
        }

        public void SetValues(CircularBuffer<IStackedBarItemData> values)
        {
            if (values.Count != this.values.Count)
            {
                Initialize(values, null, true);
                return;
            }
            this.values = values;
            UpdateValues();
        }

        public void ReCreateItems(int count, bool isInitializeItems = false)
        {
            foreach (var item in items)
                Destroy(item.gameObject);
            items.Clear();
            for (var index = 0; index < count; index++)
            {
                var item = Instantiate(itemTemplate, transform);
                item.gameObject.SetActive(true);
                if (isInitializeItems)
                {
                    item.ReCreateBars(values[index].Count);
                    item.SetValues(NormalizeValues(values[index]));
                }
                if (colors != null)
                    item.SetColors(colors);
                items.Add(item);
            }
        }

        private float[] NormalizeValues(IStackedBarItemData values)
        {
            var result = new float[values.Count];
            for (var index = 0; index < values.Count; index++)
            {
                result[index] = values[index] / max;
            }

            return result;
        }

        private void UpdateValues()
        {
            for (var index = 0; index < values.Count; index++)
            {
                items[index].SetValues(NormalizeValues(values[index]));
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying) return;
            if (colors == null || colors.Length == 0) return;
            foreach (var item in items)
                item.SetColors(colors);
        }
#endif
    }
}
