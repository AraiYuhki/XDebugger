using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using Xeon.Common.FlyweightScrollView.Model;
using Xeon.XDebugger.Console;

namespace Xeon.XDebugger.Model
{
    /// <summary>
    /// ログ項目を循環バッファで管理し、ログ種別によるフィルタリング機能を提供するクラス。
    /// </summary>
    public class LogItemBuffer : ILogDataBuffer, IDisposable
    {
        private bool visibleInfo = true;
        private bool visibleWarn = true;
        private bool visibleError = true;


        private CircularBuffer<LogItemData> buffer;
        private CircularBuffer<LogItemData> filteredBuffer;

        /// <summary>Infoログの総数。</summary>
        public int InfoCount { get; private set; }
        /// <summary>Warningログの総数。</summary>
        public int WarnCount { get; private set; }
        /// <summary>Errorログの総数。</summary>
        public int ErrorCount { get; private set; }

        /// <summary>コレクション変更時に発火されるイベント。</summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private bool visibleAll => visibleInfo && visibleWarn && visibleError;

        /// <summary>Infoログの表示状態。変更時にフィルタが再適用される。</summary>
        public bool VisibleInfo
        {
            get => visibleInfo;
            set
            {
                visibleInfo = value;
                ApplyFilter();
            }
        }

        /// <summary>Warningログの表示状態。変更時にフィルタが再適用される。</summary>
        public bool VisibleWarn
        {
            get => visibleWarn;
            set
            {
                visibleWarn = value;
                ApplyFilter();
            }
        }

        /// <summary>Errorログの表示状態。変更時にフィルタが再適用される。</summary>
        public bool VisibleError
        {
            get => visibleError;
            set
            {
                visibleError = value;
                ApplyFilter();
            }
        }

        /// <summary>現在のフィルタ条件に基づくログ件数。</summary>
        public int Count
        {
            get
            {
                return visibleAll ? buffer.Count : filteredBuffer.Count;
            }
        }

        /// <summary>指定インデックスのログ項目を取得する。</summary>
        public LogItemData this[int index]
        {
            get
            {
                return visibleAll ? buffer[index] : filteredBuffer[index];
            }
        }

        /// <summary>
        /// <see cref="LogItemBuffer"/> のコンストラクタ。
        /// </summary>
        /// <param name="capacity">バッファの最大容量。</param>
        public LogItemBuffer(int capacity)
        {
            buffer = new CircularBuffer<LogItemData>(capacity);
            buffer.CollectionChanged += OnChangedCollection;
            filteredBuffer = new CircularBuffer<LogItemData>(capacity);
            filteredBuffer.CollectionChanged += OnChangedCollection;
        }

        /// <summary>
        /// ログ項目をバッファに追加する。
        /// </summary>
        /// <param name="data">追加するログ項目データ。</param>
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

        /// <summary>
        /// バッファ内の全ログを消去し、カウントをリセットする。
        /// </summary>
        /// <param name="isNotify">trueの場合、コレクション変更を通知する。</param>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public void Dispose()
        {
            buffer.CollectionChanged -= OnChangedCollection;
            filteredBuffer.CollectionChanged -= OnChangedCollection;

            buffer.Clear();
            filteredBuffer.Clear();
        }
    }
}