using NUnit.Framework;
using Xeon.XDebugger.Model;

namespace Xeon.XDebugger.Tests
{
    [TestFixture]
    public class StringModelTests
    {
        [Test]
        public void Constructor_SetsTitle()
        {
            var model = new StringModel("Name", "hello", null);
            Assert.AreEqual("Name", model.Title);
        }

        [Test]
        public void Constructor_SetsInitialText()
        {
            var model = new StringModel("Name", "hello", null);
            Assert.AreEqual("hello", model.Text);
        }

        [Test]
        public void Constructor_SetsPriority()
        {
            var model = new StringModel("Name", "hello", null, 7);
            Assert.AreEqual(7, model.Priority);
        }

        [Test]
        public void Constructor_DefaultPriorityIsZero()
        {
            var model = new StringModel("Name", "hello", null);
            Assert.AreEqual(0, model.Priority);
        }

        [Test]
        public void TextProperty_Set_DoesNotFireCallback()
        {
            var callbackFired = false;
            var model = new StringModel("Name", "hello", _ => callbackFired = true);

            model.Text = "world";

            Assert.IsFalse(callbackFired);
        }

        [Test]
        public void TextProperty_Set_ChangesValue()
        {
            var model = new StringModel("Name", "hello", null);

            model.Text = "world";

            Assert.AreEqual("world", model.Text);
        }

        [Test]
        public void TextProperty_Set_FiresChangedEvent()
        {
            var model = new StringModel("Name", "hello", null);
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.Text = "world";

            Assert.IsTrue(changedFired);
        }

        [Test]
        public void SetText_WithNotifyTrue_FiresCallback()
        {
            string receivedValue = null;
            var model = new StringModel("Name", "hello", v => receivedValue = v);

            model.SetText("world", true);

            Assert.AreEqual("world", receivedValue);
        }

        [Test]
        public void SetText_WithNotifyFalse_DoesNotFireCallback()
        {
            var callbackFired = false;
            var model = new StringModel("Name", "hello", _ => callbackFired = true);

            model.SetText("world", false);

            Assert.IsFalse(callbackFired);
        }

        [Test]
        public void SetText_FiresChangedEvent()
        {
            var model = new StringModel("Name", "hello", null);
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.SetText("world", false);

            Assert.IsTrue(changedFired);
        }

        [Test]
        public void SetText_SameValue_DoesNotFireChangedEvent()
        {
            var model = new StringModel("Name", "hello", null);
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.SetText("hello", true);

            Assert.IsFalse(changedFired);
        }

        [Test]
        public void SetText_SameValue_DoesNotFireCallback()
        {
            var callbackFired = false;
            var model = new StringModel("Name", "hello", _ => callbackFired = true);

            model.SetText("hello", true);

            Assert.IsFalse(callbackFired);
        }

        [Test]
        public void SetText_SameValue_ValueUnchanged()
        {
            var model = new StringModel("Name", "hello", null);

            model.SetText("hello", true);

            Assert.AreEqual("hello", model.Text);
        }

        [Test]
        public void NotifyTextChangedFromView_FiresCallback()
        {
            string receivedValue = null;
            var model = new StringModel("Name", "hello", v => receivedValue = v);

            model.NotifyTextChangedFromView("world");

            Assert.AreEqual("world", receivedValue);
        }

        [Test]
        public void NotifyTextChangedFromView_ChangesText()
        {
            var model = new StringModel("Name", "hello", null);

            model.NotifyTextChangedFromView("world");

            Assert.AreEqual("world", model.Text);
        }

        [Test]
        public void NotifyTextChangedFromView_FiresChangedEvent()
        {
            var model = new StringModel("Name", "hello", null);
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.NotifyTextChangedFromView("world");

            Assert.IsTrue(changedFired);
        }

        [Test]
        public void NotifyTextChangedFromView_SameValue_DoesNotFireCallback()
        {
            var callbackFired = false;
            var model = new StringModel("Name", "hello", _ => callbackFired = true);

            model.NotifyTextChangedFromView("hello");

            Assert.IsFalse(callbackFired);
        }

        [Test]
        public void SetText_NullCallback_DoesNotThrow()
        {
            var model = new StringModel("Name", "hello", null);

            Assert.DoesNotThrow(() => model.SetText("world", true));
        }
    }
}
