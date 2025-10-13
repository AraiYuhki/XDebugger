using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Xeon.Common
{
    public class CircularBuffer<T> : IObservableCollection<T>, IReadOnlyList<T>
    {
        protected T[] buffer;  // バッファ本体

        protected int start;   // 先頭インデックス
        protected int end;     // 次に追加される位置

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
                    throw new IndexOutOfRangeException($"インデックス {index} にアクセスできません。バッファが空です。");
                if (index >= Count)
                    throw new IndexOutOfRangeException($"インデックス {index} にアクセスできません。バッファの要素数は {Count} です。");

                var actualIndex = (start + index) % Capacity;
                return buffer[actualIndex];
            }

            set
            {
                if (IsEmpty)
                    throw new IndexOutOfRangeException($"インデックス {index} にアクセスできません。バッファが空です。");
                if (index >= Count)
                    throw new IndexOutOfRangeException($"インデックス {index} にアクセスできません。バッファの要素数は {Count} です。");

                var actualIndex = (start + index) % Capacity;
                var oldItem = buffer[actualIndex];
                buffer[actualIndex] = value;

                // 要素の置換を通知
                CollectionChanged?.Invoke(this,
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Replace, value, oldItem, index));
            }
        }

        public CircularBuffer(int capacity, T[] items)
        {
            if (capacity < 1)
                throw new ArgumentException("容量は1以上でなければなりません。", nameof(capacity));

            buffer = new T[capacity];
            if (items == null)
                return;

            var copyCount = Math.Min(items.Length, capacity);
            Array.Copy(items, buffer, copyCount);

            Count = copyCount;
            start = 0;
            end = copyCount % Capacity;
        }

        public CircularBuffer(int capacity)
        {
            if (capacity < 1)
                throw new ArgumentException("容量は1以上でなければなりません。", nameof(capacity));

            buffer = new T[capacity];
            Count = 0;
            start = 0;
            end = 0;
        }

        public virtual void Clear()
        {
            // バッファ全体をクリア
            for (var i = 0; i < Capacity; i++)
                buffer[i] = default;

            Count = start = end = 0;

            // コレクション全体がリセットされたことを通知
            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public T Front()
        {
            ThrowIfEmpty();
            return buffer[start];
        }

        public T Back()
        {
            ThrowIfEmpty();
            var lastIndex = (end == 0) ? Capacity - 1 : end - 1;
            return buffer[lastIndex];
        }

        /// <summary>
        /// 末尾に要素を追加（満杯の場合は先頭を上書き）
        /// </summary>
        public virtual void PushBack(T item)
        {
            if (IsFull)
            {
                // 満杯のときは古い要素を上書きする
                var oldItem = buffer[end];
                buffer[end] = item;

                // end を進め、start も追従させる
                Increment(ref end);
                start = end;

                // 古い要素が置き換わったことを通知（論理インデックス0が上書きされた扱い）
                CollectionChanged?.Invoke(this,
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Replace, item, oldItem, 0));
                return;
            }

            // 通常の追加処理
            buffer[end] = item;
            var logicalIndex = Count;
            Increment(ref end);
            Count++;

            // 要素が追加されたことを通知
            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add, item, logicalIndex));
        }

        /// <summary>
        /// 先頭に要素を追加（満杯の場合は末尾を上書き）
        /// </summary>
        public virtual void PushFront(T item)
        {
            Decrement(ref start);

            if (IsFull)
            {
                // 満杯時は最後尾の要素を上書き
                var oldItem = buffer[start];
                buffer[start] = item;
                end = start;

                // 最後の要素が置き換わったことを通知
                CollectionChanged?.Invoke(this,
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Replace, item, oldItem, Count - 1));
                return;
            }

            buffer[start] = item;
            Count++;

            // 要素が先頭に追加されたことを通知
            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add, item, 0));
        }

        /// <summary>
        /// 末尾の要素を削除
        /// </summary>
        public virtual void PopBack()
        {
            ThrowIfEmpty("バッファが空のため、要素を削除できません。");

            var lastIndex = (end == 0) ? Capacity - 1 : end - 1;
            var removedItem = buffer[lastIndex];

            Decrement(ref end);
            buffer[end] = default;
            Count--;

            // 要素削除を通知（論理インデックス Count は削除前の最後の要素位置）
            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove, removedItem, Count));
        }

        /// <summary>
        /// 先頭の要素を削除
        /// </summary>
        public virtual void PopFront()
        {
            ThrowIfEmpty("バッファが空のため、要素を削除できません。");

            var removedItem = buffer[start];
            buffer[start] = default;
            Increment(ref start);
            Count--;

            // 先頭要素削除を通知
            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove, removedItem, 0));
        }

        /// <summary>
        /// バッファが空の場合に例外を送出
        /// </summary>
        private void ThrowIfEmpty(string message = "バッファが空のためアクセスできません。")
        {
            if (!IsEmpty) return;
            throw new InvalidOperationException(message);
        }

        /// <summary>
        /// インデックスを1進める（末尾に達したら0に戻す）
        /// </summary>
        private void Increment(ref int index)
        {
            index++;
            if (index == Capacity)
                index = 0;
        }

        /// <summary>
        /// インデックスを1戻す（0なら末尾に戻す）
        /// </summary>
        private void Decrement(ref int index)
        {
            if (index == 0)
                index = Capacity;
            index--;
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
