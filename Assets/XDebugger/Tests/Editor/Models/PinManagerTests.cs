using NUnit.Framework;
using Xeon.XDebugger.Model;
using Xeon.XDebugger.Pin;

namespace Xeon.XDebugger.Tests
{
    [TestFixture]
    public class PinManagerTests
    {
        [SetUp]
        public void SetUp() => PinManager.ResetState();

        [Test]
        public void Pin_AddsModelAndNotifies()
        {
            var model = new LabelModel("a");
            ControlModelBase changed = null;
            var pinnedFlag = false;
            PinManager.PinChanged += (m, p) => { changed = m; pinnedFlag = p; };

            model.Pin();

            Assert.IsTrue(model.IsPinned);
            Assert.AreSame(model, changed);
            Assert.IsTrue(pinnedFlag);
        }

        [Test]
        public void Pin_Twice_DoesNotDuplicate()
        {
            var model = new LabelModel("a");
            model.Pin();
            model.Pin();
            Assert.AreEqual(1, PinManager.Pinned.Count);
        }

        [Test]
        public void Unpin_RemovesModel()
        {
            var model = new LabelModel("a");
            model.Pin();
            model.Unpin();
            Assert.IsFalse(model.IsPinned);
            Assert.AreEqual(0, PinManager.Pinned.Count);
        }

        [Test]
        public void TogglePin_Toggles()
        {
            var model = new LabelModel("a");
            PinManager.TogglePin(model);
            Assert.IsTrue(model.IsPinned);
            PinManager.TogglePin(model);
            Assert.IsFalse(model.IsPinned);
        }

        [Test]
        public void PinnedVisible_IsIndependentOfPinMode()
        {
            PinManager.SetPinMode(true);
            PinManager.SetPinMode(false);
            Assert.IsTrue(PinManager.PinnedVisible);
            PinManager.SetPinnedVisible(false);
            Assert.IsFalse(PinManager.PinnedVisible);
            Assert.IsFalse(PinManager.PinMode);
        }

        [Test]
        public void PinModeChanged_FiresOnlyOnChange()
        {
            var count = 0;
            PinManager.PinModeChanged += () => count++;
            PinManager.SetPinMode(true);
            PinManager.SetPinMode(true);
            Assert.AreEqual(1, count);
        }

        [Test]
        public void UnpinTree_UnpinsModelAndGroupDescendants()
        {
            var group = new VerticalGroupModel("g");
            var child = new LabelModel("c");
            var other = new LabelModel("o");
            group.AddChild(child);
            group.Pin();
            child.Pin();
            other.Pin();

            PinManager.UnpinTree(new ControlModelBase[] { group });

            Assert.IsFalse(group.IsPinned);
            Assert.IsFalse(child.IsPinned);
            Assert.IsTrue(other.IsPinned);
        }
    }
}
