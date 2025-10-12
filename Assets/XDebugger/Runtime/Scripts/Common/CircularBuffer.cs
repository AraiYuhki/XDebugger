using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Xeon.Common
{
    public class CircularBuffer<T> : ICollection<T>, IEnumerable<T>, IReadOnlyList<T>, INotifyCollectionChanged
    {
        protected T[] buffer;
        
        protected int start;
        protected int end;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public int Capacity => buffer.Length;
        public bool IsFull => Count == Capacity;
        public bool IsEmpty => Count == 0;
        public int Count { get; protected set; }

        public bool IsReadOnly => false;

        public virtual T this[int index]
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
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, buffer[actualIndex], index));
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

        public virtual void Clear()
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

        public virtual void PushBack(T item)
        {
            buffer[end] = item;
            var prevIndex = end;
            Increment(ref end);
            var actionType = NotifyCollectionChangedAction.Add;
            if (IsFull)
            {
                actionType = NotifyCollectionChangedAction.Replace;
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(actionType, buffer[prevIndex], prevIndex));
                start = end;
                return;
            }
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(actionType, buffer[prevIndex], prevIndex));
            Count++;
        }

        public virtual void PushFront(T item)
        {
            Decrement(ref start);
            buffer[start] = item;
            var actionType = NotifyCollectionChangedAction.Add;
            if (IsFull)
            {
                actionType = NotifyCollectionChangedAction.Replace;
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(actionType, buffer[start], start));
                end = start;
                return;
            }
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(actionType, buffer[start], start));
            Count++;
        }

        public virtual void PopBack()
        {
            ThrowIfEmpty("Cannot take elements from an empty buffer.");
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, buffer[end], end));
            Decrement(ref end);
            buffer[end] = default;
            Count--;
        }

        public virtual void PopFront()
        {
            ThrowIfEmpty("Cannot taek elements from an empty buffer.");
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, buffer[start], start));
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

        public void Add(T item)
        {
            PushBack(item);
        }

        public bool Contains(T item)
        {
            return buffer.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            buffer.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            UnityEngine.Debug.LogWarning("CircularBuffer does not support the Remove function.\n Use PopBack or PopFront instead.");
            return false;
        }
    }
}
