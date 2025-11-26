using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using Xeon.Common.FlyweightScrollView.Model;
using Xeon.XDebugger.Console;

namespace Xeon.XDebugger.Model
{
    public class LogItemBuffer : ILogDataBuffer, IDisposable
    {
        private bool visibleInfo = true;
        private bool visibleWarn = true;
        private bool visibleError = true;


        private CircularBuffer<LogItemData> buffer;
        private CircularBuffer<LogItemData> filteredBuffer;

        public int InfoCount { get; private set; }
        public int WarnCount { get; private set; }
        public int ErrorCount { get; private set; }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private bool visibleAll => visibleInfo && visibleWarn && visibleError;

        public bool VisibleInfo
        {
            get => visibleInfo;
            set
            {
                visibleInfo = value;
                ApplyFilter();
            }
        }

        public bool VisibleWarn
        {
            get => visibleWarn;
            set
            {
                visibleWarn = value;
                ApplyFilter();
            }
        }

        public bool VisibleError
        {
            get => visibleError;
            set
            {
                visibleError = value;
                ApplyFilter();
            }
        }

        public int Count
        {
            get
            {
                return visibleAll ? buffer.Count : filteredBuffer.Count;
            }
        }

        public LogItemData this[int index]
        {
            get
            {
                return visibleAll ? buffer[index] : filteredBuffer[index];
            }
        }

        public LogItemBuffer(int capacity)
        {
            buffer = new CircularBuffer<LogItemData>(capacity);
            buffer.CollectionChanged += OnChangedCollection;
            filteredBuffer = new CircularBuffer<LogItemData>(capacity);
            filteredBuffer.CollectionChanged += OnChangedCollection;
        }

        public void Add(LogItemData data)
        {
            buffer.PushBack(data, visibleAll);
            switch (data.Type)
            {
                case LogType.Log:
                    InfoCount++;
                    break;
                case LogType.Warning:
                    WarnCount++;
                    break;
                default:
                    ErrorCount++;
                    break;
            }
            if (!CheckVisible(data))
            {
                return;
            }
            filteredBuffer.PushBack(data, !visibleAll);
        }

        private bool CheckVisible(LogItemData data)
        {
            return data.Type switch
            {
                LogType.Log => visibleInfo,
                LogType.Warning => visibleWarn,
                _ => visibleError
            };
        }

        public void Clear(bool isNotify = true)
        {
            if (isNotify)
            {
                buffer.Clear();
                filteredBuffer.Clear(false);
            }
            else
            {
                buffer.Clear(false);
                filteredBuffer.Clear(false);
            }
            InfoCount = 0;
            WarnCount = 0;
            ErrorCount = 0;
        }

        public IEnumerator<LogItemData> GetEnumerator()
        {
            var target = visibleAll ? buffer : filteredBuffer;
            foreach (var data in target)
                yield return data;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void ApplyFilter()
        {
            filteredBuffer.Clear(false);
            foreach (var data in buffer)
            {
                if (CheckVisible(data))
                    filteredBuffer.Add(data, false);
            }
            OnChangedCollection(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void OnChangedCollection(object sender, NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(sender, e);
        }

        public void Dispose()
        {
            buffer.CollectionChanged -= OnChangedCollection;
            filteredBuffer.CollectionChanged -= OnChangedCollection;

            buffer.Clear();
            filteredBuffer.Clear();
        }
    }
}