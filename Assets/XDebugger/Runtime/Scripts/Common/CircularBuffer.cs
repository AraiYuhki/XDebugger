using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Xeon.Common
{
    public class CircularBuffer<T> : IEnumerable<T>, IReadOnlyList<T>
    {
        private T[] buffer;
        
        private int start;
        private int end;

        public int Capacity => buffer.Length;
        public bool IsFull => Count == Capacity;
        public bool IsEmpty => Count == 0;
        public int Count { get; private set; }

        public T this[int index]
        {
            get
            {
                if (IsEmpty)
                    throw new IndexOutOfRangeException($"Cannot access index {index}. Buffer is empty.");
                if (index >= Count)
                    throw new IndexOutOfRangeException($"Cannot access index {index}. Buffer size is {Count}.");
                var actualIndex = (start + index) % Capacity;
                return buffer[actualIndex];
            }
            
            set
            {
                if (IsEmpty)
                    throw new IndexOutOfRangeException($"Cannot access index {index}. Buffer is empty.");
                if (index >= Count)
                    throw new IndexOutOfRangeException($"Cannot access index {index}. Buffer size is {Count}.");
                var actualIndex = (start + index) % Capacity;
                buffer[actualIndex] = value;
            }
        }

        public CircularBuffer(int capacity, T[] items)
        {
            if (capacity < 1)
                throw new ArgumentException("Circular buffer cannot have negative or zero capacity,", nameof(capacity));

            buffer = new T[capacity];
            if (items == null)
                return;
            Array.Copy(items, buffer, items.Length);
            
            Count = items.Length;
        }

        public CircularBuffer(int capacity)
        {
            if (capacity < 1)
                throw new ArgumentException("Circular buffer cannot have negative or zero capacity,", nameof(capacity));
            buffer = new T[capacity];
            for (var index = 0; index < capacity; index++)
                buffer[index] = default;
            Count = capacity;
        }

        public void Clear()
        {
            Count = start = end = 0;
        }

        public T Front()
        {
            ThrowIfEmpty();
            return buffer[start];
        }

        public T Back()
        {
            ThrowIfEmpty();
            var actualIndex = (end != 0 ? end : Count) - 1;
            return buffer[actualIndex];
        }

        public void PushBack(T item)
        {
            buffer[end] = item;
            Increment(ref end);
            if (IsFull)
            {
                start = end;
                return;
            }
            
            Count++;
        }

        public void PushFront(T item)
        {
            Decrement(ref start);
            buffer[start] = item;
            
            if (IsFull)
            {
                end = start;
                return;
            }
            
            Count++;
        }

        public void PopBack()
        {
            ThrowIfEmpty("Cannot take elements from an empty buffer.");
            Decrement(ref end);
            buffer[end] = default;
            Count--;
        }

        public void PopFront()
        {
            ThrowIfEmpty("Cannot taek elements from an empty buffer.");
            buffer[start] = default;
            Increment(ref start);
            Count--;
        }
        
        private void ThrowIfEmpty(string message = "cannot access an empty buffer.")
        {
            if (!IsEmpty) return;
            throw new InvalidOperationException(message);
        }

        private void Increment(ref int index)
        {
            index++;
            if (index == Capacity)
                index = 0;
        }

        private void Decrement(ref int index)
        {
            if (index == 0)
                index = Capacity;
            index--;
        }

        public T[] ToArray()
        {
            var newArray = new T[Count];
            var newArrayOffset = 0;
            for (var index = 0; index < Count; index++)
            {
                var actualIndex = (index + start) % Capacity;
                newArray[index] = buffer[actualIndex];
            }

            return newArray;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (var index = 0; index < Count; index++)
            {
                var actualIndex = (start + index) % Capacity;
                yield return buffer[actualIndex];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
