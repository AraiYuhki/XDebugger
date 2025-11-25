using System;
using System.Collections.Generic;
using UnityEngine;
using Xeon.Common.FlyweightScrollView.Model;
using Xeon.XDebugger.Console;

namespace Xeon.XDebugger.Model
{
    public class LogItemBuffer : CircularBuffer<LogItemData>
    {
        private bool visibleInfo = true;
        private bool visibleWarn = true;
        private bool visibleError = true;

        private CircularBuffer<LogItemData> filteredBuffer;

        public override int Count
        {
            get => VisibleAll ? base.Count : filteredBuffer.Count;
        }

        public override LogItemData this[int index]
        {
            get
            {
                if (VisibleAll)
                    return base[index];
                if (filteredBuffer.IsEmpty)
                    throw new IndexOutOfRangeException($"インデックス {index} にアクセスできません。バッファが空です。");
                if (index >= Count)
                    throw new IndexOutOfRangeException($"インデックス {index} にアクセスできません。バッファの要素数は {Count} です。");
                return filteredBuffer[index];
            }
        }

        public bool VisibleAll => VisibleInfo && VisibleWarn && VisibleError;

        public bool VisibleInfo
        {
            get => visibleInfo;
            set
            {
                visibleInfo = value;
            }
        }

        public bool VisibleWarn
        {
            get => visibleWarn;
            set
            {
                visibleWarn = value;
            }
        }

        public bool VisibleError
        {
            get => visibleError;
            set
            {
                visibleError = value;
            }
        }
        
        public LogItemBuffer(int capacity) : base(capacity, Array.Empty<LogItemData>())
        {
            filteredBuffer = new(capacity);
        }

        public LogItemBuffer(int capacity, bool isFill) : base(capacity, isFill)
        {
            filteredBuffer = new(capacity, isFill);
        }

        public override void PushBack(LogItemData item)
        {
            base.PushBack(item);
            switch (item.Type)
            {
                case LogType.Log:
                    if (visibleInfo)
                        filteredBuffer.PushBack(item);
                    break;
                case LogType.Warning:
                    if (visibleWarn)
                        filteredBuffer.PushBack(item);
                    break;
                default:
                    if (visibleError)
                        filteredBuffer.PushBack(item);
                    break;
            }
        }

        public override void PushFront(LogItemData item)
        {
            base.PushFront(item);
            switch (item.Type)
            {
                case LogType.Log:
                    if (visibleInfo)
                        filteredBuffer.PushFront(item);
                    break;
                case LogType.Warning:
                    if (visibleWarn)
                        filteredBuffer.PushFront(item);
                    break;
                default:
                    if (visibleError)
                        filteredBuffer.PushFront(item);
                    break;
            }
        }

        public override IEnumerator<LogItemData> GetEnumerator()
        {
            if (VisibleAll)
            {
                for (var index = 0; index < buffer.Length; index++)
                {
                    var actualIndex = (start + index) % Capacity;
                    yield return buffer[actualIndex];
                }

                yield break;
            }

            foreach (var item in filteredBuffer)
                yield return item;

        }
    }
}