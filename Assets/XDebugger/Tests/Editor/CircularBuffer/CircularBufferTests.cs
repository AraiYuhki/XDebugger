using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using NUnit.Framework;
using Xeon.Common.FlyweightScrollView.Model;

namespace Xeon.XDebugger.Tests
{
    [TestFixture]
    public class CircularBufferTests
    {
        #region Constructor Tests

        [Test]
        public void Constructor_WithCapacity_CreatesEmptyBuffer()
        {
            var buffer = new CircularBuffer<int>(5);

            Assert.AreEqual(5, buffer.Capacity);
            Assert.AreEqual(0, buffer.Count);
            Assert.IsTrue(buffer.IsEmpty);
            Assert.IsFalse(buffer.IsFull);
            Assert.IsFalse(buffer.IsReadOnly);
        }

        [Test]
        public void Constructor_WithCapacityOne_CreatesBufferWithCapacityOne()
        {
            var buffer = new CircularBuffer<int>(1);

            Assert.AreEqual(1, buffer.Capacity);
            Assert.AreEqual(0, buffer.Count);
        }

        [Test]
        public void Constructor_WithFillTrue_CreatesFullBuffer()
        {
            var buffer = new CircularBuffer<int>(3, fill: true);

            Assert.AreEqual(3, buffer.Capacity);
            Assert.AreEqual(3, buffer.Count);
            Assert.IsTrue(buffer.IsFull);
            Assert.IsFalse(buffer.IsEmpty);
            Assert.AreEqual(default(int), buffer[0]);
            Assert.AreEqual(default(int), buffer[1]);
            Assert.AreEqual(default(int), buffer[2]);
        }

        [Test]
        public void Constructor_WithFillFalse_CreatesEmptyBuffer()
        {
            var buffer = new CircularBuffer<int>(3, fill: false);

            Assert.AreEqual(0, buffer.Count);
            Assert.IsTrue(buffer.IsEmpty);
        }

        [Test]
        public void Constructor_WithItems_CopiesItemsIntoBuffer()
        {
            var items = new[] { 10, 20, 30 };
            var buffer = new CircularBuffer<int>(5, items);

            Assert.AreEqual(5, buffer.Capacity);
            Assert.AreEqual(3, buffer.Count);
            Assert.AreEqual(10, buffer[0]);
            Assert.AreEqual(20, buffer[1]);
            Assert.AreEqual(30, buffer[2]);
        }

        [Test]
        public void Constructor_WithItems_ExceedingCapacity_TruncatesToCapacity()
        {
            var items = new[] { 10, 20, 30, 40, 50 };
            var buffer = new CircularBuffer<int>(3, items);

            Assert.AreEqual(3, buffer.Capacity);
            Assert.AreEqual(3, buffer.Count);
            Assert.AreEqual(10, buffer[0]);
            Assert.AreEqual(20, buffer[1]);
            Assert.AreEqual(30, buffer[2]);
        }

        [Test]
        public void Constructor_WithItems_ExactCapacity_FillsBuffer()
        {
            var items = new[] { 1, 2, 3 };
            var buffer = new CircularBuffer<int>(3, items);

            Assert.AreEqual(3, buffer.Count);
            Assert.IsTrue(buffer.IsFull);
        }

        [Test]
        public void Constructor_WithNullItems_CreatesEmptyBuffer()
        {
            var buffer = new CircularBuffer<int>(3, null);

            Assert.AreEqual(3, buffer.Capacity);
            Assert.AreEqual(0, buffer.Count);
        }

        [Test]
        public void Constructor_WithCapacityLessThanOne_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new CircularBuffer<int>(0));
            Assert.Throws<ArgumentException>(() => new CircularBuffer<int>(-1));
        }

        [Test]
        public void Constructor_WithItemsAndCapacityLessThanOne_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new CircularBuffer<int>(0, new[] { 1, 2 }));
            Assert.Throws<ArgumentException>(() => new CircularBuffer<int>(-5, new[] { 1 }));
        }

        [Test]
        public void Constructor_WithReferenceType_DefaultsToNull()
        {
            var buffer = new CircularBuffer<string>(3, fill: true);

            Assert.AreEqual(3, buffer.Count);
            Assert.IsNull(buffer[0]);
            Assert.IsNull(buffer[1]);
            Assert.IsNull(buffer[2]);
        }

        #endregion

        #region PushBack Tests

        [Test]
        public void PushBack_ToEmptyBuffer_AddsElement()
        {
            var buffer = new CircularBuffer<int>(3);

            buffer.PushBack(42);

            Assert.AreEqual(1, buffer.Count);
            Assert.AreEqual(42, buffer[0]);
            Assert.IsFalse(buffer.IsFull);
            Assert.IsFalse(buffer.IsEmpty);
        }

        [Test]
        public void PushBack_MultipleElements_AddsInOrder()
        {
            var buffer = new CircularBuffer<int>(5);

            buffer.PushBack(1);
            buffer.PushBack(2);
            buffer.PushBack(3);

            Assert.AreEqual(3, buffer.Count);
            Assert.AreEqual(1, buffer[0]);
            Assert.AreEqual(2, buffer[1]);
            Assert.AreEqual(3, buffer[2]);
        }

        [Test]
        public void PushBack_FillsToCapacity_MarksAsFull()
        {
            var buffer = new CircularBuffer<int>(3);

            buffer.PushBack(1);
            buffer.PushBack(2);
            buffer.PushBack(3);

            Assert.IsTrue(buffer.IsFull);
            Assert.AreEqual(3, buffer.Count);
        }

        [Test]
        public void PushBack_WhenFull_OverwritesOldestElement()
        {
            var buffer = new CircularBuffer<int>(3);

            buffer.PushBack(1);
            buffer.PushBack(2);
            buffer.PushBack(3);
            buffer.PushBack(4); // should overwrite 1

            Assert.AreEqual(3, buffer.Count);
            Assert.IsTrue(buffer.IsFull);
            Assert.AreEqual(2, buffer[0]);
            Assert.AreEqual(3, buffer[1]);
            Assert.AreEqual(4, buffer[2]);
        }

        [Test]
        public void PushBack_WhenFull_MultipleOverwrites()
        {
            var buffer = new CircularBuffer<int>(3);

            buffer.PushBack(1);
            buffer.PushBack(2);
            buffer.PushBack(3);
            buffer.PushBack(4); // overwrites 1
            buffer.PushBack(5); // overwrites 2

            Assert.AreEqual(3, buffer.Count);
            Assert.AreEqual(3, buffer[0]);
            Assert.AreEqual(4, buffer[1]);
            Assert.AreEqual(5, buffer[2]);
        }

        [Test]
        public void PushBack_CapacityOne_OverwritesSingleElement()
        {
            var buffer = new CircularBuffer<int>(1);

            buffer.PushBack(1);
            Assert.AreEqual(1, buffer[0]);

            buffer.PushBack(2);
            Assert.AreEqual(1, buffer.Count);
            Assert.AreEqual(2, buffer[0]);
        }

        [Test]
        public void Add_DelegatesToPushBack()
        {
            var buffer = new CircularBuffer<int>(3);

            buffer.Add(10);
            buffer.Add(20);

            Assert.AreEqual(2, buffer.Count);
            Assert.AreEqual(10, buffer[0]);
            Assert.AreEqual(20, buffer[1]);
        }

        #endregion

        #region PushFront Tests

        [Test]
        public void PushFront_ToEmptyBuffer_AddsElement()
        {
            var buffer = new CircularBuffer<int>(3);

            buffer.PushFront(42);

            Assert.AreEqual(1, buffer.Count);
            Assert.AreEqual(42, buffer[0]);
        }

        [Test]
        public void PushFront_MultipleElements_AddsInReverseOrder()
        {
            var buffer = new CircularBuffer<int>(5);

            buffer.PushFront(1);
            buffer.PushFront(2);
            buffer.PushFront(3);

            Assert.AreEqual(3, buffer.Count);
            // PushFront adds to the beginning, so last pushed is first
            Assert.AreEqual(3, buffer[0]);
            Assert.AreEqual(2, buffer[1]);
            Assert.AreEqual(1, buffer[2]);
        }

        [Test]
        public void PushFront_FillsToCapacity_MarksAsFull()
        {
            var buffer = new CircularBuffer<int>(3);

            buffer.PushFront(1);
            buffer.PushFront(2);
            buffer.PushFront(3);

            Assert.IsTrue(buffer.IsFull);
            Assert.AreEqual(3, buffer.Count);
        }

        [Test]
        public void PushFront_WhenFull_OverwritesLastElement()
        {
            var buffer = new CircularBuffer<int>(3);

            buffer.PushFront(1);
            buffer.PushFront(2);
            buffer.PushFront(3);
            // buffer is [3, 2, 1], now push 4 to front
            buffer.PushFront(4); // should overwrite the last element (1)

            Assert.AreEqual(3, buffer.Count);
            Assert.IsTrue(buffer.IsFull);
            Assert.AreEqual(4, buffer[0]);
            Assert.AreEqual(3, buffer[1]);
            Assert.AreEqual(2, buffer[2]);
        }

        [Test]
        public void PushFront_WhenFull_MultipleOverwrites()
        {
            var buffer = new CircularBuffer<int>(3);

            buffer.PushFront(1);
            buffer.PushFront(2);
            buffer.PushFront(3);
            buffer.PushFront(4); // overwrites 1
            buffer.PushFront(5); // overwrites 2

            Assert.AreEqual(3, buffer.Count);
            Assert.AreEqual(5, buffer[0]);
            Assert.AreEqual(4, buffer[1]);
            Assert.AreEqual(3, buffer[2]);
        }

        [Test]
        public void PushFront_And_PushBack_Mixed()
        {
            var buffer = new CircularBuffer<int>(5);

            buffer.PushBack(2);
            buffer.PushBack(3);
            buffer.PushFront(1);
            buffer.PushBack(4);
            buffer.PushFront(0);

            Assert.AreEqual(5, buffer.Count);
            Assert.AreEqual(0, buffer[0]);
            Assert.AreEqual(1, buffer[1]);
            Assert.AreEqual(2, buffer[2]);
            Assert.AreEqual(3, buffer[3]);
            Assert.AreEqual(4, buffer[4]);
        }

        #endregion

        #region PopBack Tests

        [Test]
        public void PopBack_RemovesLastElement()
        {
            var buffer = new CircularBuffer<int>(5);
            buffer.PushBack(1);
            buffer.PushBack(2);
            buffer.PushBack(3);

            buffer.PopBack();

            Assert.AreEqual(2, buffer.Count);
            Assert.AreEqual(1, buffer[0]);
            Assert.AreEqual(2, buffer[1]);
        }

        [Test]
        public void PopBack_SingleElement_MakesEmpty()
        {
            var buffer = new CircularBuffer<int>(3);
            buffer.PushBack(42);

            buffer.PopBack();

            Assert.AreEqual(0, buffer.Count);
            Assert.IsTrue(buffer.IsEmpty);
        }

        [Test]
        public void PopBack_AllElements_MakesEmpty()
        {
            var buffer = new CircularBuffer<int>(3);
            buffer.PushBack(1);
            buffer.PushBack(2);
            buffer.PushBack(3);

            buffer.PopBack();
            buffer.PopBack();
            buffer.PopBack();

            Assert.IsTrue(buffer.IsEmpty);
            Assert.AreEqual(0, buffer.Count);
        }

        [Test]
        public void PopBack_WhenEmpty_ThrowsInvalidOperationException()
        {
            var buffer = new CircularBuffer<int>(3);

            Assert.Throws<InvalidOperationException>(() => buffer.PopBack());
        }

        #endregion

        #region PopFront Tests

        [Test]
        public void PopFront_RemovesFirstElement()
        {
            var buffer = new CircularBuffer<int>(5);
            buffer.PushBack(1);
            buffer.PushBack(2);
            buffer.PushBack(3);

            buffer.PopFront();

            Assert.AreEqual(2, buffer.Count);
            Assert.AreEqual(2, buffer[0]);
            Assert.AreEqual(3, buffer[1]);
        }

        [Test]
        public void PopFront_SingleElement_MakesEmpty()
        {
            var buffer = new CircularBuffer<int>(3);
            buffer.PushBack(42);

            buffer.PopFront();

            Assert.AreEqual(0, buffer.Count);
            Assert.IsTrue(buffer.IsEmpty);
        }

        [Test]
        public void PopFront_AllElements_MakesEmpty()
        {
            var buffer = new CircularBuffer<int>(3);
            buffer.PushBack(1);
            buffer.PushBack(2);
            buffer.PushBack(3);

            buffer.PopFront();
            buffer.PopFront();
            buffer.PopFront();

            Assert.IsTrue(buffer.IsEmpty);
            Assert.AreEqual(0, buffer.Count);
        }

        [Test]
        public void PopFront_WhenEmpty_ThrowsInvalidOperationException()
        {
            var buffer = new CircularBuffer<int>(3);

            Assert.Throws<InvalidOperationException>(() => buffer.PopFront());
        }

        #endregion

        #region Front and Back Tests

        [Test]
        public void Front_ReturnsFrontElement()
        {
            var buffer = new CircularBuffer<int>(5);
            buffer.PushBack(10);
            buffer.PushBack(20);
            buffer.PushBack(30);

            Assert.AreEqual(10, buffer.Front());
        }

        [Test]
        public void Front_SingleElement_ReturnsThatElement()
        {
            var buffer = new CircularBuffer<int>(3);
            buffer.PushBack(42);

            Assert.AreEqual(42, buffer.Front());
        }

        [Test]
        public void Front_WhenEmpty_ThrowsInvalidOperationException()
        {
            var buffer = new CircularBuffer<int>(3);

            Assert.Throws<InvalidOperationException>(() => buffer.Front());
        }

        [Test]
        public void Back_ReturnsLastElement()
        {
            var buffer = new CircularBuffer<int>(5);
            buffer.PushBack(10);
            buffer.PushBack(20);
            buffer.PushBack(30);

            Assert.AreEqual(30, buffer.Back());
        }

        [Test]
        public void Back_SingleElement_ReturnsThatElement()
        {
            var buffer = new CircularBuffer<int>(3);
            buffer.PushBack(42);

            Assert.AreEqual(42, buffer.Back());
        }

        [Test]
        public void Back_WhenEmpty_ThrowsInvalidOperationException()
        {
            var buffer = new CircularBuffer<int>(3);

            Assert.Throws<InvalidOperationException>(() => buffer.Back());
        }

        [Test]
        public void Front_AfterOverwrite_ReturnsNewFront()
        {
            var buffer = new CircularBuffer<int>(3);
            buffer.PushBack(1);
            buffer.PushBack(2);
            buffer.PushBack(3);
            buffer.PushBack(4); // overwrites 1, front is now 2

            Assert.AreEqual(2, buffer.Front());
        }

        [Test]
        public void Back_AfterOverwrite_ReturnsNewBack()
        {
            var buffer = new CircularBuffer<int>(3);
            buffer.PushBack(1);
            buffer.PushBack(2);
            buffer.PushBack(3);
            buffer.PushBack(4); // overwrites 1, back is now 4

            Assert.AreEqual(4, buffer.Back());
        }

        #endregion

        #region Indexer Tests

        [Test]
        public void Indexer_Get_ReturnsCorrectElements()
        {
            var buffer = new CircularBuffer<int>(5);
            buffer.PushBack(10);
            buffer.PushBack(20);
            buffer.PushBack(30);

            Assert.AreEqual(10, buffer[0]);
            Assert.AreEqual(20, buffer[1]);
            Assert.AreEqual(30, buffer[2]);
        }

        [Test]
        public void Indexer_Set_UpdatesElement()
        {
            var buffer = new CircularBuffer<int>(5);
            buffer.PushBack(10);
            buffer.PushBack(20);
            buffer.PushBack(30);

            buffer[1] = 99;

            Assert.AreEqual(99, buffer[1]);
            Assert.AreEqual(10, buffer[0]);
            Assert.AreEqual(30, buffer[2]);
        }

        [Test]
        public void Indexer_Get_WhenEmpty_ThrowsIndexOutOfRangeException()
        {
            var buffer = new CircularBuffer<int>(3);

            Assert.Throws<IndexOutOfRangeException>(() => { var _ = buffer[0]; });
        }

        [Test]
        public void Indexer_Set_WhenEmpty_ThrowsIndexOutOfRangeException()
        {
            var buffer = new CircularBuffer<int>(3);

            Assert.Throws<IndexOutOfRangeException>(() => { buffer[0] = 42; });
        }

        [Test]
        public void Indexer_Get_OutOfRange_ThrowsIndexOutOfRangeException()
        {
            var buffer = new CircularBuffer<int>(5);
            buffer.PushBack(1);
            buffer.PushBack(2);

            Assert.Throws<IndexOutOfRangeException>(() => { var _ = buffer[2]; });
            Assert.Throws<IndexOutOfRangeException>(() => { var _ = buffer[5]; });
        }

        [Test]
        public void Indexer_Set_OutOfRange_ThrowsIndexOutOfRangeException()
        {
            var buffer = new CircularBuffer<int>(5);
            buffer.PushBack(1);
            buffer.PushBack(2);

            Assert.Throws<IndexOutOfRangeException>(() => { buffer[2] = 99; });
            Assert.Throws<IndexOutOfRangeException>(() => { buffer[5] = 99; });
        }

        [Test]
        public void Indexer_Get_AfterWrapAround_ReturnsCorrectLogicalOrder()
        {
            var buffer = new CircularBuffer<int>(3);
            buffer.PushBack(1);
            buffer.PushBack(2);
            buffer.PushBack(3);
            buffer.PushBack(4); // overwrites 1, logical: [2, 3, 4]
            buffer.PushBack(5); // overwrites 2, logical: [3, 4, 5]

            Assert.AreEqual(3, buffer[0]);
            Assert.AreEqual(4, buffer[1]);
            Assert.AreEqual(5, buffer[2]);
        }

        [Test]
        public void Indexer_Set_AfterWrapAround_SetsCorrectElement()
        {
            var buffer = new CircularBuffer<int>(3);
            buffer.PushBack(1);
            buffer.PushBack(2);
            buffer.PushBack(3);
            buffer.PushBack(4); // logical: [2, 3, 4]

            buffer[0] = 99;
            buffer[2] = 88;

            Assert.AreEqual(99, buffer[0]);
            Assert.AreEqual(3, buffer[1]);
            Assert.AreEqual(88, buffer[2]);
        }

        #endregion

        #region Clear Tests

        [Test]
        public void Clear_EmptiesBuffer()
        {
            var buffer = new CircularBuffer<int>(5);
            buffer.PushBack(1);
            buffer.PushBack(2);
            buffer.PushBack(3);

            buffer.Clear();

            Assert.AreEqual(0, buffer.Count);
            Assert.IsTrue(buffer.IsEmpty);
            Assert.IsFalse(buffer.IsFull);
        }

        [Test]
        public void Clear_AlreadyEmptyBuffer_DoesNotThrow()
        {
            var buffer = new CircularBuffer<int>(3);

            Assert.DoesNotThrow(() => buffer.Clear());
            Assert.AreEqual(0, buffer.Count);
        }

        [Test]
        public void Clear_WithNotify_FiresResetEvent()
        {
            var buffer = new CircularBuffer<int>(3);
            buffer.PushBack(1);
            buffer.PushBack(2);

            NotifyCollectionChangedAction? action = null;
            buffer.CollectionChanged += (sender, args) => { action = args.Action; };

            buffer.Clear(isNotify: true);

            Assert.AreEqual(NotifyCollectionChangedAction.Reset, action);
        }

        [Test]
        public void Clear_WithIsNotifyFalse_DoesNotFireEvent()
        {
            var buffer = new CircularBuffer<int>(3);
            buffer.PushBack(1);
            buffer.PushBack(2);

            var eventFired = false;
            buffer.CollectionChanged += (sender, args) => { eventFired = true; };

            buffer.Clear(isNotify: false);

            Assert.IsFalse(eventFired);
            Assert.AreEqual(0, buffer.Count);
        }

        [Test]
        public void Clear_AfterClear_CanAddElementsAgain()
        {
            var buffer = new CircularBuffer<int>(3);
            buffer.PushBack(1);
            buffer.PushBack(2);
            buffer.PushBack(3);
            buffer.Clear();

            buffer.PushBack(10);
            buffer.PushBack(20);

            Assert.AreEqual(2, buffer.Count);
            Assert.AreEqual(10, buffer[0]);
            Assert.AreEqual(20, buffer[1]);
        }

        #endregion

        #region CollectionChanged Event Tests

        [Test]
        public void CollectionChanged_PushBack_FiresAddAction()
        {
            var buffer = new CircularBuffer<int>(5);

            NotifyCollectionChangedAction? action = null;
            object newItem = null;
            int? index = null;
            buffer.CollectionChanged += (sender, args) =>
            {
                action = args.Action;
                newItem = args.NewItems?[0];
                index = args.NewStartingIndex;
            };

            buffer.PushBack(42);

            Assert.AreEqual(NotifyCollectionChangedAction.Add, action);
            Assert.AreEqual(42, newItem);
            Assert.AreEqual(0, index);
        }

        [Test]
        public void CollectionChanged_PushBack_WhenFull_FiresReplaceAction()
        {
            var buffer = new CircularBuffer<int>(2);
            buffer.PushBack(1);
            buffer.PushBack(2);

            NotifyCollectionChangedAction? action = null;
            object newItem = null;
            object oldItem = null;
            buffer.CollectionChanged += (sender, args) =>
            {
                action = args.Action;
                newItem = args.NewItems?[0];
                oldItem = args.OldItems?[0];
            };

            buffer.PushBack(3); // overwrites 1

            Assert.AreEqual(NotifyCollectionChangedAction.Replace, action);
            Assert.AreEqual(3, newItem);
            Assert.AreEqual(1, oldItem);
        }

        [Test]
        public void CollectionChanged_PushBack_WithIsNotifyFalse_DoesNotFire()
        {
            var buffer = new CircularBuffer<int>(5);

            var eventFired = false;
            buffer.CollectionChanged += (sender, args) => { eventFired = true; };

            buffer.PushBack(42, isNotify: false);

            Assert.IsFalse(eventFired);
        }

        [Test]
        public void CollectionChanged_PushFront_FiresAddAction()
        {
            var buffer = new CircularBuffer<int>(5);

            NotifyCollectionChangedAction? action = null;
            object newItem = null;
            int? index = null;
            buffer.CollectionChanged += (sender, args) =>
            {
                action = args.Action;
                newItem = args.NewItems?[0];
                index = args.NewStartingIndex;
            };

            buffer.PushFront(42);

            Assert.AreEqual(NotifyCollectionChangedAction.Add, action);
            Assert.AreEqual(42, newItem);
            Assert.AreEqual(0, index);
        }

        [Test]
        public void CollectionChanged_PushFront_WhenFull_FiresReplaceAction()
        {
            var buffer = new CircularBuffer<int>(2);
            buffer.PushBack(1);
            buffer.PushBack(2);

            NotifyCollectionChangedAction? action = null;
            object newItem = null;
            object oldItem = null;
            int? index = null;
            buffer.CollectionChanged += (sender, args) =>
            {
                action = args.Action;
                newItem = args.NewItems?[0];
                oldItem = args.OldItems?[0];
                index = args.NewStartingIndex;
            };

            buffer.PushFront(3); // overwrites tail

            Assert.AreEqual(NotifyCollectionChangedAction.Replace, action);
            Assert.AreEqual(3, newItem);
            Assert.AreEqual(buffer.Count - 1, index);
        }

        [Test]
        public void CollectionChanged_PushFront_WithIsNotifyFalse_DoesNotFire()
        {
            var buffer = new CircularBuffer<int>(5);

            var eventFired = false;
            buffer.CollectionChanged += (sender, args) => { eventFired = true; };

            buffer.PushFront(42, isNotify: false);

            Assert.IsFalse(eventFired);
        }

        [Test]
        public void CollectionChanged_PopBack_FiresRemoveAction()
        {
            var buffer = new CircularBuffer<int>(5);
            buffer.PushBack(10);
            buffer.PushBack(20);
            buffer.PushBack(30);

            NotifyCollectionChangedAction? action = null;
            object removedItem = null;
            int? index = null;
            buffer.CollectionChanged += (sender, args) =>
            {
                action = args.Action;
                removedItem = args.OldItems?[0];
                index = args.OldStartingIndex;
            };

            buffer.PopBack();

            Assert.AreEqual(NotifyCollectionChangedAction.Remove, action);
            Assert.AreEqual(30, removedItem);
            Assert.AreEqual(2, index);
        }

        [Test]
        public void CollectionChanged_PopBack_WithIsNotifyFalse_DoesNotFire()
        {
            var buffer = new CircularBuffer<int>(5);
            buffer.PushBack(10);

            var eventFired = false;
            buffer.CollectionChanged += (sender, args) => { eventFired = true; };

            buffer.PopBack(isNotify: false);

            Assert.IsFalse(eventFired);
        }

        [Test]
        public void CollectionChanged_PopFront_FiresRemoveAction()
        {
            var buffer = new CircularBuffer<int>(5);
            buffer.PushBack(10);
            buffer.PushBack(20);
            buffer.PushBack(30);

            NotifyCollectionChangedAction? action = null;
            object removedItem = null;
            int? index = null;
            buffer.CollectionChanged += (sender, args) =>
            {
                action = args.Action;
                removedItem = args.OldItems?[0];
                index = args.OldStartingIndex;
            };

            buffer.PopFront();

            Assert.AreEqual(NotifyCollectionChangedAction.Remove, action);
            Assert.AreEqual(10, removedItem);
            Assert.AreEqual(0, index);
        }

        [Test]
        public void CollectionChanged_PopFront_WithIsNotifyFalse_DoesNotFire()
        {
            var buffer = new CircularBuffer<int>(5);
            buffer.PushBack(10);

            var eventFired = false;
            buffer.CollectionChanged += (sender, args) => { eventFired = true; };

            buffer.PopFront(isNotify: false);

            Assert.IsFalse(eventFired);
        }

        [Test]
        public void CollectionChanged_IndexerSet_FiresReplaceAction()
        {
            var buffer = new CircularBuffer<int>(5);
            buffer.PushBack(10);
            buffer.PushBack(20);
            buffer.PushBack(30);

            NotifyCollectionChangedAction? action = null;
            object newItem = null;
            object oldItem = null;
            int? index = null;
            buffer.CollectionChanged += (sender, args) =>
            {
                action = args.Action;
                newItem = args.NewItems?[0];
                oldItem = args.OldItems?[0];
                index = args.NewStartingIndex;
            };

            buffer[1] = 99;

            Assert.AreEqual(NotifyCollectionChangedAction.Replace, action);
            Assert.AreEqual(99, newItem);
            Assert.AreEqual(20, oldItem);
            Assert.AreEqual(1, index);
        }

        [Test]
        public void CollectionChanged_Clear_FiresResetAction()
        {
            var buffer = new CircularBuffer<int>(5);
            buffer.PushBack(1);
            buffer.PushBack(2);

            NotifyCollectionChangedAction? action = null;
            buffer.CollectionChanged += (sender, args) => { action = args.Action; };

            buffer.Clear();

            Assert.AreEqual(NotifyCollectionChangedAction.Reset, action);
        }

        #endregion

        #region GetEnumerator Tests

        [Test]
        public void GetEnumerator_EmptyBuffer_ReturnsNoElements()
        {
            var buffer = new CircularBuffer<int>(3);

            var items = new List<int>();
            foreach (var item in buffer)
                items.Add(item);

            Assert.AreEqual(0, items.Count);
        }

        [Test]
        public void GetEnumerator_ReturnsElementsInLogicalOrder()
        {
            var buffer = new CircularBuffer<int>(5);
            buffer.PushBack(10);
            buffer.PushBack(20);
            buffer.PushBack(30);

            var items = new List<int>();
            foreach (var item in buffer)
                items.Add(item);

            Assert.AreEqual(3, items.Count);
            Assert.AreEqual(10, items[0]);
            Assert.AreEqual(20, items[1]);
            Assert.AreEqual(30, items[2]);
        }

        [Test]
        public void GetEnumerator_AfterWrapAround_ReturnsCorrectOrder()
        {
            var buffer = new CircularBuffer<int>(3);
            buffer.PushBack(1);
            buffer.PushBack(2);
            buffer.PushBack(3);
            buffer.PushBack(4); // overwrites 1
            buffer.PushBack(5); // overwrites 2

            var items = new List<int>();
            foreach (var item in buffer)
                items.Add(item);

            Assert.AreEqual(3, items.Count);
            Assert.AreEqual(3, items[0]);
            Assert.AreEqual(4, items[1]);
            Assert.AreEqual(5, items[2]);
        }

        [Test]
        public void GetEnumerator_SingleElement_ReturnsThatElement()
        {
            var buffer = new CircularBuffer<int>(3);
            buffer.PushBack(42);

            var items = new List<int>();
            foreach (var item in buffer)
                items.Add(item);

            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(42, items[0]);
        }

        [Test]
        public void GetEnumerator_FilledBuffer_ReturnsAllElements()
        {
            var buffer = new CircularBuffer<int>(3, fill: true);

            var items = new List<int>();
            foreach (var item in buffer)
                items.Add(item);

            Assert.AreEqual(3, items.Count);
            Assert.AreEqual(0, items[0]);
            Assert.AreEqual(0, items[1]);
            Assert.AreEqual(0, items[2]);
        }

        #endregion

        #region Wrap-Around Tests

        [Test]
        public void WrapAround_PushBackAndPopFront_CyclesCorrectly()
        {
            var buffer = new CircularBuffer<int>(3);

            // Fill and cycle multiple times
            for (var cycle = 0; cycle < 5; cycle++)
            {
                buffer.PushBack(cycle * 10 + 1);
                buffer.PushBack(cycle * 10 + 2);
                buffer.PopFront();
                buffer.PopFront();
            }

            Assert.AreEqual(0, buffer.Count);
            Assert.IsTrue(buffer.IsEmpty);

            // Should still work after cycling
            buffer.PushBack(100);
            Assert.AreEqual(1, buffer.Count);
            Assert.AreEqual(100, buffer[0]);
        }

        [Test]
        public void WrapAround_MaintainsLogicalOrderAfterManyCycles()
        {
            var buffer = new CircularBuffer<int>(4);

            // Push and pop to cycle start/end around the array
            for (var i = 0; i < 10; i++)
            {
                buffer.PushBack(i);
                if (buffer.Count > 2)
                    buffer.PopFront();
            }

            // Verify logical order is maintained
            var items = new List<int>();
            foreach (var item in buffer)
                items.Add(item);

            for (var i = 1; i < items.Count; i++)
            {
                Assert.Greater(items[i], items[i - 1],
                    $"Element at {i} should be greater than element at {i - 1}");
            }
        }

        [Test]
        public void WrapAround_PushFrontAndPopBack_CyclesCorrectly()
        {
            var buffer = new CircularBuffer<int>(3);

            for (var cycle = 0; cycle < 5; cycle++)
            {
                buffer.PushFront(cycle * 10 + 1);
                buffer.PushFront(cycle * 10 + 2);
                buffer.PopBack();
                buffer.PopBack();
            }

            Assert.AreEqual(0, buffer.Count);
            Assert.IsTrue(buffer.IsEmpty);

            buffer.PushFront(100);
            Assert.AreEqual(1, buffer.Count);
            Assert.AreEqual(100, buffer[0]);
        }

        [Test]
        public void WrapAround_InterleavedOperations_MaintainsIntegrity()
        {
            var buffer = new CircularBuffer<int>(5);

            // Interleave various operations
            buffer.PushBack(1);  // [1]
            buffer.PushBack(2);  // [1, 2]
            buffer.PushFront(0); // [0, 1, 2]
            buffer.PopBack();    // [0, 1]
            buffer.PushBack(3);  // [0, 1, 3]
            buffer.PushBack(4);  // [0, 1, 3, 4]
            buffer.PopFront();   // [1, 3, 4]
            buffer.PushFront(-1); // [-1, 1, 3, 4]

            Assert.AreEqual(4, buffer.Count);
            Assert.AreEqual(-1, buffer[0]);
            Assert.AreEqual(1, buffer[1]);
            Assert.AreEqual(3, buffer[2]);
            Assert.AreEqual(4, buffer[3]);
            Assert.AreEqual(-1, buffer.Front());
            Assert.AreEqual(4, buffer.Back());
        }

        [Test]
        public void WrapAround_FullOverwriteCycle_MaintainsCorrectState()
        {
            var buffer = new CircularBuffer<int>(3);

            // Push enough to overwrite the entire buffer multiple times
            for (var i = 1; i <= 12; i++)
                buffer.PushBack(i);

            Assert.AreEqual(3, buffer.Count);
            Assert.IsTrue(buffer.IsFull);
            Assert.AreEqual(10, buffer[0]);
            Assert.AreEqual(11, buffer[1]);
            Assert.AreEqual(12, buffer[2]);
        }

        [Test]
        public void WrapAround_ClearAndRefill_WorksCorrectly()
        {
            var buffer = new CircularBuffer<int>(3);

            // First fill
            buffer.PushBack(1);
            buffer.PushBack(2);
            buffer.PushBack(3);
            buffer.PushBack(4); // overwrite, start has moved

            buffer.Clear();

            // Second fill should work from clean state
            buffer.PushBack(10);
            buffer.PushBack(20);

            Assert.AreEqual(2, buffer.Count);
            Assert.AreEqual(10, buffer[0]);
            Assert.AreEqual(20, buffer[1]);
            Assert.AreEqual(10, buffer.Front());
            Assert.AreEqual(20, buffer.Back());
        }

        #endregion

        #region Edge Case Tests

        [Test]
        public void ReferenceType_PushAndPop_WorksCorrectly()
        {
            var buffer = new CircularBuffer<string>(3);

            buffer.PushBack("hello");
            buffer.PushBack("world");

            Assert.AreEqual("hello", buffer[0]);
            Assert.AreEqual("world", buffer[1]);

            buffer.PopFront();
            Assert.AreEqual("world", buffer[0]);
        }

        [Test]
        public void IsFull_And_IsEmpty_MutuallyExclusive_WhenCapacityGreaterThanOne()
        {
            var buffer = new CircularBuffer<int>(3);

            Assert.IsTrue(buffer.IsEmpty);
            Assert.IsFalse(buffer.IsFull);

            buffer.PushBack(1);
            Assert.IsFalse(buffer.IsEmpty);
            Assert.IsFalse(buffer.IsFull);

            buffer.PushBack(2);
            buffer.PushBack(3);
            Assert.IsFalse(buffer.IsEmpty);
            Assert.IsTrue(buffer.IsFull);
        }

        [Test]
        public void CapacityOne_IsFull_And_IsEmpty_AfterAddAndRemove()
        {
            var buffer = new CircularBuffer<int>(1);

            Assert.IsTrue(buffer.IsEmpty);
            Assert.IsFalse(buffer.IsFull);

            buffer.PushBack(1);
            Assert.IsFalse(buffer.IsEmpty);
            Assert.IsTrue(buffer.IsFull);

            buffer.PopBack();
            Assert.IsTrue(buffer.IsEmpty);
            Assert.IsFalse(buffer.IsFull);
        }

        [Test]
        public void Constructor_WithItems_FrontAndBack_AreCorrect()
        {
            var items = new[] { 10, 20, 30 };
            var buffer = new CircularBuffer<int>(5, items);

            Assert.AreEqual(10, buffer.Front());
            Assert.AreEqual(30, buffer.Back());
        }

        [Test]
        public void Add_WithIsNotifyFalse_SuppressesEvent()
        {
            var buffer = new CircularBuffer<int>(3);

            var eventFired = false;
            buffer.CollectionChanged += (sender, args) => { eventFired = true; };

            buffer.Add(10, isNotify: false);

            Assert.IsFalse(eventFired);
            Assert.AreEqual(1, buffer.Count);
        }

        [Test]
        public void PushBack_WhenFull_WithIsNotifyFalse_SuppressesEvent()
        {
            var buffer = new CircularBuffer<int>(2);
            buffer.PushBack(1);
            buffer.PushBack(2);

            var eventFired = false;
            buffer.CollectionChanged += (sender, args) => { eventFired = true; };

            buffer.PushBack(3, isNotify: false);

            Assert.IsFalse(eventFired);
            Assert.AreEqual(2, buffer[0]);
            Assert.AreEqual(3, buffer[1]);
        }

        [Test]
        public void PushFront_WhenFull_WithIsNotifyFalse_SuppressesEvent()
        {
            var buffer = new CircularBuffer<int>(2);
            buffer.PushBack(1);
            buffer.PushBack(2);

            var eventFired = false;
            buffer.CollectionChanged += (sender, args) => { eventFired = true; };

            buffer.PushFront(3, isNotify: false);

            Assert.IsFalse(eventFired);
        }

        #endregion
    }
}
