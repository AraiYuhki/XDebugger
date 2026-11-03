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

        private static (VerticalGroupModel group, LabelModel a, LabelModel b) CreateGroup()
        {
            var group = new VerticalGroupModel("g");
            var a = new LabelModel("a");
            var b = new LabelModel("b");
            group.AddChild(a);
            group.AddChild(b);
            return (group, a, b);
        }

        [Test]
        public void PinGroup_PinsAllDescendantLeaves()
        {
            var (group, a, b) = CreateGroup();
            group.Pin();
            Assert.IsTrue(a.IsPinned);
            Assert.IsTrue(b.IsPinned);
            Assert.IsTrue(group.IsPinned);
        }

        [Test]
        public void Group_IsNotPinned_WhenAnyChildIsNotPinned()
        {
            var (group, a, b) = CreateGroup();
            group.Pin();
            a.Unpin();
            Assert.IsFalse(group.IsPinned);
            Assert.IsTrue(b.IsPinned);
        }

        [Test]
        public void Group_BecomesPinned_WhenAllChildrenArePinnedIndividually()
        {
            var (group, a, b) = CreateGroup();
            a.Pin();
            Assert.IsFalse(group.IsPinned);
            b.Pin();
            Assert.IsTrue(group.IsPinned);
        }

        [Test]
        public void TogglePin_OnPartiallyPinnedGroup_PinsAll()
        {
            var (group, a, b) = CreateGroup();
            a.Pin();
            PinManager.TogglePin(group);
            Assert.IsTrue(a.IsPinned);
            Assert.IsTrue(b.IsPinned);
        }

        [Test]
        public void TogglePin_OnFullyPinnedGroup_UnpinsAll()
        {
            var (group, a, b) = CreateGroup();
            group.Pin();
            PinManager.TogglePin(group);
            Assert.IsFalse(a.IsPinned);
            Assert.IsFalse(b.IsPinned);
        }

        [Test]
        public void NestedGroup_PinAndPartialUnpin()
        {
            var root = new VerticalGroupModel("root");
            var inner = new VerticalGroupModel("inner");
            var a = new LabelModel("a");
            var b = new LabelModel("b");
            inner.AddChild(a);
            root.AddChild(inner);
            root.AddChild(b);

            root.Pin();
            Assert.IsTrue(inner.IsPinned);

            a.Unpin();
            Assert.IsFalse(inner.IsPinned);
            Assert.IsFalse(root.IsPinned);
            Assert.IsTrue(b.IsPinned);
            Assert.IsFalse(PinManager.IsVisibleInPin(inner));
            Assert.IsTrue(PinManager.IsVisibleInPin(root));
        }

        [Test]
        public void RootOf_ReturnsTopLevelModel()
        {
            var root = new VerticalGroupModel("root");
            var inner = new VerticalGroupModel("inner");
            var a = new LabelModel("a");
            inner.AddChild(a);
            root.AddChild(inner);
            Assert.AreSame(root, PinManager.RootOf(a));
        }

        [Test]
        public void UnpinTree_UnpinsModelAndGroupDescendants()
        {
            var (group, a, b) = CreateGroup();
            var other = new LabelModel("o");
            group.Pin();
            other.Pin();

            PinManager.UnpinTree(new ControlModelBase[] { group });

            Assert.IsFalse(a.IsPinned);
            Assert.IsFalse(b.IsPinned);
            Assert.IsTrue(other.IsPinned);
        }
    }
}
