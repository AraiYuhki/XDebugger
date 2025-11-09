using System;
using System.Collections.Generic;

namespace Xeon.Common
{
    public class DoubleCircularBuffer : CircularBuffer<double>
    {
        private SortedSet<double> heap = new();

        public DoubleCircularBuffer(int capacity) : base(capacity)
        {
        }

        public DoubleCircularBuffer(int capacity, double[] items) : base(capacity, items)
        {
            foreach (var value in items)
                heap.Add(value);
        }

        public double Max => heap.Max;

        public override double this[int index]
        {
            get => base[index];
            set
            {
                if (IsEmpty)
                    throw new IndexOutOfRangeException($"Cannot access index {index}. Buffer is empty.");
                if (index >= Count)
                    throw new IndexOutOfRangeException($"Cannot access index {index}. Buffer size is {Count}.");

                var actualIndex = (start + index) % Capacity;
                heap.Remove(base[actualIndex]);
                heap.Add(value);
                buffer[actualIndex] = value;
            }
        }

        public override void Clear()
        {
            heap.Clear();
            base.Clear();
        }

        public override void PushBack(double item)
        {
            if (IsEmpty && item > 0)
            {
                heap.Add(item);
            }
            else
            {
                heap.Remove(buffer[end]);
                heap.Add(item);
            }
            base.PushBack(item);
        }

        public override void PushFront(double item)
        {
            if (IsEmpty && item > 0)
                heap.Add(item);
            else
            {
                heap.Remove(buffer[start]);
                heap.Add(item);
            }
            base.PushFront(item);
        }

        public override void PopBack()
        {
            heap.Remove(buffer[end]);
            base.PopBack();
        }

        public override void PopFront()
        {
            heap.Remove(buffer[start]);
            base.PopFront();
        }
    }
}
