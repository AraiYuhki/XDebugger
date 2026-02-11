using NUnit.Framework;
using Xeon.XDebugger.Model;

namespace Xeon.XDebugger.Tests
{
    [TestFixture]
    public class LabelModelTests
    {
        [Test]
        public void Constructor_SetsTitle()
        {
            var model = new LabelModel("Status");
            Assert.AreEqual("Status", model.Title);
        }

        [Test]
        public void Constructor_SetsPriority()
        {
            var model = new LabelModel("Status", 5);
            Assert.AreEqual(5, model.Priority);
        }

        [Test]
        public void Constructor_DefaultPriorityIsZero()
        {
            var model = new LabelModel("Status");
            Assert.AreEqual(0, model.Priority);
        }

        [Test]
        public void SetText_UpdatesTitle()
        {
            var model = new LabelModel("Status");

            model.SetText("Updated");

            Assert.AreEqual("Updated", model.Title);
        }

        [Test]
        public void SetText_FiresChangedEvent()
        {
            var model = new LabelModel("Status");
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.SetText("Updated");

            Assert.IsTrue(changedFired);
        }

        [Test]
        public void SetText_SameValue_StillFiresChangedEvent()
        {
            var model = new LabelModel("Status");
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.SetText("Status");

            Assert.IsTrue(changedFired);
        }

        [Test]
        public void SetText_EmptyString_UpdatesTitle()
        {
            var model = new LabelModel("Status");

            model.SetText("");

            Assert.AreEqual("", model.Title);
        }

        [Test]
        public void SetText_MultipleCalls_UpdatesToLatest()
        {
            var model = new LabelModel("Status");

            model.SetText("First");
            model.SetText("Second");
            model.SetText("Third");

            Assert.AreEqual("Third", model.Title);
        }

        [Test]
        public void SetText_MultipleCalls_FiresChangedEventEachTime()
        {
            var model = new LabelModel("Status");
            var changedCount = 0;
            model.Changed += () => changedCount++;

            model.SetText("First");
            model.SetText("Second");

            Assert.AreEqual(2, changedCount);
        }
    }
}
