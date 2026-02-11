using System.Collections.Specialized;
using NUnit.Framework;
using UnityEngine;
using Xeon.XDebugger.Console;
using Xeon.XDebugger.Model;

namespace Xeon.XDebugger.Tests
{
    [TestFixture]
    public class LogItemBufferTests
    {
        private LogItemBuffer buffer;

        [SetUp]
        public void SetUp()
        {
            buffer = new LogItemBuffer(100);
        }

        [TearDown]
        public void TearDown()
        {
            buffer.Dispose();
        }

        [Test]
        public void Constructor_InitialCountIsZero()
        {
            Assert.AreEqual(0, buffer.Count);
        }

        [Test]
        public void Constructor_InitialCountsAreZero()
        {
            Assert.AreEqual(0, buffer.InfoCount);
            Assert.AreEqual(0, buffer.WarnCount);
            Assert.AreEqual(0, buffer.ErrorCount);
        }

        [Test]
        public void Constructor_VisibleFiltersDefaultToTrue()
        {
            Assert.IsTrue(buffer.VisibleInfo);
            Assert.IsTrue(buffer.VisibleWarn);
            Assert.IsTrue(buffer.VisibleError);
        }

        // --- Add / Count tests ---

        [Test]
        public void Add_LogType_Log_IncrementsInfoCount()
        {
            buffer.Add(new LogItemData(LogType.Log, "info message", "trace", 0));

            Assert.AreEqual(1, buffer.InfoCount);
            Assert.AreEqual(0, buffer.WarnCount);
            Assert.AreEqual(0, buffer.ErrorCount);
        }

        [Test]
        public void Add_LogType_Warning_IncrementsWarnCount()
        {
            buffer.Add(new LogItemData(LogType.Warning, "warn message", "trace", 0));

            Assert.AreEqual(0, buffer.InfoCount);
            Assert.AreEqual(1, buffer.WarnCount);
            Assert.AreEqual(0, buffer.ErrorCount);
        }

        [Test]
        public void Add_LogType_Error_IncrementsErrorCount()
        {
            buffer.Add(new LogItemData(LogType.Error, "error message", "trace", 0));

            Assert.AreEqual(0, buffer.InfoCount);
            Assert.AreEqual(0, buffer.WarnCount);
            Assert.AreEqual(1, buffer.ErrorCount);
        }

        [Test]
        public void Add_LogType_Exception_IncrementsErrorCount()
        {
            buffer.Add(new LogItemData(LogType.Exception, "exception", "trace", 0));

            Assert.AreEqual(1, buffer.ErrorCount);
        }

        [Test]
        public void Add_LogType_Assert_IncrementsErrorCount()
        {
            buffer.Add(new LogItemData(LogType.Assert, "assert", "trace", 0));

            Assert.AreEqual(1, buffer.ErrorCount);
        }

        [Test]
        public void Add_MultipleItems_CountIncreases()
        {
            buffer.Add(new LogItemData(LogType.Log, "msg1", "trace", 0));
            buffer.Add(new LogItemData(LogType.Warning, "msg2", "trace", 1));
            buffer.Add(new LogItemData(LogType.Error, "msg3", "trace", 2));

            Assert.AreEqual(3, buffer.Count);
            Assert.AreEqual(1, buffer.InfoCount);
            Assert.AreEqual(1, buffer.WarnCount);
            Assert.AreEqual(1, buffer.ErrorCount);
        }

        // --- Indexer tests ---

        [Test]
        public void Indexer_ReturnsCorrectElement()
        {
            var data0 = new LogItemData(LogType.Log, "first", "trace", 0);
            var data1 = new LogItemData(LogType.Warning, "second", "trace", 1);
            buffer.Add(data0);
            buffer.Add(data1);

            Assert.AreEqual("first", buffer[0].Contents);
            Assert.AreEqual("second", buffer[1].Contents);
        }

        [Test]
        public void Indexer_WithFilter_ReturnsFilteredElement()
        {
            buffer.Add(new LogItemData(LogType.Log, "info", "trace", 0));
            buffer.Add(new LogItemData(LogType.Warning, "warn", "trace", 1));
            buffer.Add(new LogItemData(LogType.Error, "error", "trace", 2));

            buffer.VisibleInfo = false;

            Assert.AreEqual(2, buffer.Count);
            Assert.AreEqual("warn", buffer[0].Contents);
            Assert.AreEqual("error", buffer[1].Contents);
        }

        // --- VisibleInfo filter tests ---

        [Test]
        public void VisibleInfo_SetFalse_HidesInfoLogs()
        {
            buffer.Add(new LogItemData(LogType.Log, "info", "trace", 0));
            buffer.Add(new LogItemData(LogType.Warning, "warn", "trace", 1));

            buffer.VisibleInfo = false;

            Assert.AreEqual(1, buffer.Count);
            Assert.AreEqual("warn", buffer[0].Contents);
        }

        [Test]
        public void VisibleInfo_ToggleBackToTrue_RestoresInfoLogs()
        {
            buffer.Add(new LogItemData(LogType.Log, "info", "trace", 0));
            buffer.Add(new LogItemData(LogType.Warning, "warn", "trace", 1));

            buffer.VisibleInfo = false;
            Assert.AreEqual(1, buffer.Count);

            buffer.VisibleInfo = true;
            Assert.AreEqual(2, buffer.Count);
            Assert.AreEqual("info", buffer[0].Contents);
        }

        [Test]
        public void VisibleInfo_DoesNotAffectInfoCount()
        {
            buffer.Add(new LogItemData(LogType.Log, "info", "trace", 0));

            buffer.VisibleInfo = false;

            Assert.AreEqual(1, buffer.InfoCount);
        }

        // --- VisibleWarn filter tests ---

        [Test]
        public void VisibleWarn_SetFalse_HidesWarningLogs()
        {
            buffer.Add(new LogItemData(LogType.Log, "info", "trace", 0));
            buffer.Add(new LogItemData(LogType.Warning, "warn", "trace", 1));

            buffer.VisibleWarn = false;

            Assert.AreEqual(1, buffer.Count);
            Assert.AreEqual("info", buffer[0].Contents);
        }

        [Test]
        public void VisibleWarn_ToggleBackToTrue_RestoresWarningLogs()
        {
            buffer.Add(new LogItemData(LogType.Warning, "warn", "trace", 0));
            buffer.Add(new LogItemData(LogType.Error, "error", "trace", 1));

            buffer.VisibleWarn = false;
            Assert.AreEqual(1, buffer.Count);

            buffer.VisibleWarn = true;
            Assert.AreEqual(2, buffer.Count);
        }

        // --- VisibleError filter tests ---

        [Test]
        public void VisibleError_SetFalse_HidesErrorLogs()
        {
            buffer.Add(new LogItemData(LogType.Log, "info", "trace", 0));
            buffer.Add(new LogItemData(LogType.Error, "error", "trace", 1));

            buffer.VisibleError = false;

            Assert.AreEqual(1, buffer.Count);
            Assert.AreEqual("info", buffer[0].Contents);
        }

        [Test]
        public void VisibleError_ToggleBackToTrue_RestoresErrorLogs()
        {
            buffer.Add(new LogItemData(LogType.Error, "error", "trace", 0));
            buffer.Add(new LogItemData(LogType.Log, "info", "trace", 1));

            buffer.VisibleError = false;
            Assert.AreEqual(1, buffer.Count);

            buffer.VisibleError = true;
            Assert.AreEqual(2, buffer.Count);
        }

        [Test]
        public void VisibleError_AlsoHidesExceptionAndAssert()
        {
            buffer.Add(new LogItemData(LogType.Error, "error", "trace", 0));
            buffer.Add(new LogItemData(LogType.Exception, "exception", "trace", 1));
            buffer.Add(new LogItemData(LogType.Assert, "assert", "trace", 2));
            buffer.Add(new LogItemData(LogType.Log, "info", "trace", 3));

            buffer.VisibleError = false;

            Assert.AreEqual(1, buffer.Count);
            Assert.AreEqual("info", buffer[0].Contents);
        }

        // --- Multiple filter combination tests ---

        [Test]
        public void AllFiltersOff_CountIsZero()
        {
            buffer.Add(new LogItemData(LogType.Log, "info", "trace", 0));
            buffer.Add(new LogItemData(LogType.Warning, "warn", "trace", 1));
            buffer.Add(new LogItemData(LogType.Error, "error", "trace", 2));

            buffer.VisibleInfo = false;
            buffer.VisibleWarn = false;
            buffer.VisibleError = false;

            Assert.AreEqual(0, buffer.Count);
        }

        [Test]
        public void AllFiltersOn_ReadsFromBufferDirectly()
        {
            buffer.Add(new LogItemData(LogType.Log, "info", "trace", 0));
            buffer.Add(new LogItemData(LogType.Warning, "warn", "trace", 1));
            buffer.Add(new LogItemData(LogType.Error, "error", "trace", 2));

            Assert.AreEqual(3, buffer.Count);
            Assert.AreEqual("info", buffer[0].Contents);
            Assert.AreEqual("warn", buffer[1].Contents);
            Assert.AreEqual("error", buffer[2].Contents);
        }

        // --- Adding items while filter is off ---

        [Test]
        public void Add_WhileFilterOff_StillUpdatesCount()
        {
            buffer.VisibleInfo = false;

            buffer.Add(new LogItemData(LogType.Log, "info", "trace", 0));

            Assert.AreEqual(1, buffer.InfoCount);
            Assert.AreEqual(0, buffer.Count);
        }

        [Test]
        public void Add_WhileFilterOff_ItemAppearsAfterFilterOn()
        {
            buffer.VisibleInfo = false;

            buffer.Add(new LogItemData(LogType.Log, "info", "trace", 0));
            buffer.VisibleInfo = true;

            Assert.AreEqual(1, buffer.Count);
            Assert.AreEqual("info", buffer[0].Contents);
        }

        // --- Clear tests ---

        [Test]
        public void Clear_ResetsAllCounts()
        {
            buffer.Add(new LogItemData(LogType.Log, "info", "trace", 0));
            buffer.Add(new LogItemData(LogType.Warning, "warn", "trace", 1));
            buffer.Add(new LogItemData(LogType.Error, "error", "trace", 2));

            buffer.Clear();

            Assert.AreEqual(0, buffer.Count);
            Assert.AreEqual(0, buffer.InfoCount);
            Assert.AreEqual(0, buffer.WarnCount);
            Assert.AreEqual(0, buffer.ErrorCount);
        }

        [Test]
        public void Clear_WithNotifyTrue_FiresCollectionChanged()
        {
            buffer.Add(new LogItemData(LogType.Log, "info", "trace", 0));

            var eventFired = false;
            buffer.CollectionChanged += (_, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Reset)
                    eventFired = true;
            };

            buffer.Clear(true);

            Assert.IsTrue(eventFired);
        }

        [Test]
        public void Clear_WithNotifyFalse_DoesNotFireCollectionChanged()
        {
            buffer.Add(new LogItemData(LogType.Log, "info", "trace", 0));

            var eventFired = false;
            buffer.CollectionChanged += (_, _) => eventFired = true;

            buffer.Clear(false);

            Assert.IsFalse(eventFired);
        }

        // --- CollectionChanged event tests ---

        [Test]
        public void Add_WhenAllVisible_FiresCollectionChangedAdd()
        {
            NotifyCollectionChangedAction? receivedAction = null;
            buffer.CollectionChanged += (_, e) => receivedAction = e.Action;

            buffer.Add(new LogItemData(LogType.Log, "info", "trace", 0));

            Assert.AreEqual(NotifyCollectionChangedAction.Add, receivedAction);
        }

        [Test]
        public void VisibleInfo_SetFalse_FiresCollectionChangedReset()
        {
            buffer.Add(new LogItemData(LogType.Log, "info", "trace", 0));

            NotifyCollectionChangedAction? receivedAction = null;
            buffer.CollectionChanged += (_, e) => receivedAction = e.Action;

            buffer.VisibleInfo = false;

            Assert.AreEqual(NotifyCollectionChangedAction.Reset, receivedAction);
        }

        // --- Enumeration tests ---

        [Test]
        public void GetEnumerator_AllVisible_IteratesAllItems()
        {
            buffer.Add(new LogItemData(LogType.Log, "info", "trace", 0));
            buffer.Add(new LogItemData(LogType.Warning, "warn", "trace", 1));

            var count = 0;
            foreach (var item in buffer)
                count++;

            Assert.AreEqual(2, count);
        }

        [Test]
        public void GetEnumerator_WithFilter_IteratesFilteredItems()
        {
            buffer.Add(new LogItemData(LogType.Log, "info", "trace", 0));
            buffer.Add(new LogItemData(LogType.Warning, "warn", "trace", 1));

            buffer.VisibleInfo = false;

            var count = 0;
            string firstContent = null;
            foreach (var item in buffer)
            {
                firstContent ??= item.Contents;
                count++;
            }

            Assert.AreEqual(1, count);
            Assert.AreEqual("warn", firstContent);
        }

        // --- Dispose tests ---

        [Test]
        public void Dispose_ClearsBuffer()
        {
            buffer.Add(new LogItemData(LogType.Log, "info", "trace", 0));

            buffer.Dispose();

            Assert.AreEqual(0, buffer.Count);
        }

        [Test]
        public void Dispose_EventsNoLongerForwardedFromInternalBuffers()
        {
            var eventCount = 0;
            buffer.CollectionChanged += (_, _) => eventCount++;

            buffer.Dispose();

            // After dispose, the internal buffer events are unsubscribed,
            // so Clear (which fires on internal buffers) should not propagate.
            // Count is already 0, calling Clear again should not fire our handler.
            // Note: Dispose already called Clear internally, which will fire Reset events
            // through the internal buffers' own Clear calls, but our handler on LogItemBuffer
            // won't receive them because events were already unsubscribed.
            Assert.AreEqual(0, eventCount);
        }

        // --- Small capacity tests ---

        [Test]
        public void SmallCapacity_OverflowStillTracksCorrectCounts()
        {
            var smallBuffer = new LogItemBuffer(2);

            smallBuffer.Add(new LogItemData(LogType.Log, "info1", "trace", 0));
            smallBuffer.Add(new LogItemData(LogType.Warning, "warn1", "trace", 1));
            smallBuffer.Add(new LogItemData(LogType.Error, "error1", "trace", 2));

            Assert.AreEqual(1, smallBuffer.InfoCount);
            Assert.AreEqual(1, smallBuffer.WarnCount);
            Assert.AreEqual(1, smallBuffer.ErrorCount);
            Assert.AreEqual(2, smallBuffer.Count);

            smallBuffer.Dispose();
        }
    }
}
