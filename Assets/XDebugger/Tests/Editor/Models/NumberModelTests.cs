using NUnit.Framework;
using Xeon.XDebugger.Model;

namespace Xeon.XDebugger.Tests
{
    [TestFixture]
    public class NumberModelTests
    {
        [Test]
        public void Constructor_SetsTitle()
        {
            var model = new NumberModel("Count", 10f, 1f, null);
            Assert.AreEqual("Count", model.Title);
        }

        [Test]
        public void Constructor_SetsInitialValue()
        {
            var model = new NumberModel("Count", 10f, 1f, null);
            Assert.AreEqual(10f, model.Value);
        }

        [Test]
        public void Constructor_SetsStep()
        {
            var model = new NumberModel("Count", 10f, 0.5f, null);
            Assert.AreEqual(0.5f, model.Step);
        }

        [Test]
        public void Constructor_SetsPriority()
        {
            var model = new NumberModel("Count", 10f, 1f, null, 3);
            Assert.AreEqual(3, model.Priority);
        }

        [Test]
        public void Constructor_DefaultPriorityIsZero()
        {
            var model = new NumberModel("Count", 10f, 1f, null);
            Assert.AreEqual(0, model.Priority);
        }

        [Test]
        public void ValueProperty_Set_DoesNotFireCallback()
        {
            var callbackFired = false;
            var model = new NumberModel("Count", 10f, 1f, _ => callbackFired = true);

            model.Value = 20f;

            Assert.IsFalse(callbackFired);
        }

        [Test]
        public void ValueProperty_Set_ChangesValue()
        {
            var model = new NumberModel("Count", 10f, 1f, null);

            model.Value = 20f;

            Assert.AreEqual(20f, model.Value);
        }

        [Test]
        public void ValueProperty_Set_FiresChangedEvent()
        {
            var model = new NumberModel("Count", 10f, 1f, null);
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.Value = 20f;

            Assert.IsTrue(changedFired);
        }

        [Test]
        public void SetValue_WithNotifyTrue_FiresCallback()
        {
            float? receivedValue = null;
            var model = new NumberModel("Count", 10f, 1f, v => receivedValue = v);

            model.SetValue(20f, true);

            Assert.AreEqual(20f, receivedValue);
        }

        [Test]
        public void SetValue_WithNotifyFalse_DoesNotFireCallback()
        {
            var callbackFired = false;
            var model = new NumberModel("Count", 10f, 1f, _ => callbackFired = true);

            model.SetValue(20f, false);

            Assert.IsFalse(callbackFired);
        }

        [Test]
        public void SetValue_FiresChangedEvent()
        {
            var model = new NumberModel("Count", 10f, 1f, null);
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.SetValue(20f, false);

            Assert.IsTrue(changedFired);
        }

        [Test]
        public void SetValue_SameValue_DoesNotFireChangedEvent()
        {
            var model = new NumberModel("Count", 10f, 1f, null);
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.SetValue(10f, true);

            Assert.IsFalse(changedFired);
        }

        [Test]
        public void SetValue_SameValue_DoesNotFireCallback()
        {
            var callbackFired = false;
            var model = new NumberModel("Count", 10f, 1f, _ => callbackFired = true);

            model.SetValue(10f, true);

            Assert.IsFalse(callbackFired);
        }

        [Test]
        public void SetValue_SameValue_ValueUnchanged()
        {
            var model = new NumberModel("Count", 10f, 1f, null);

            model.SetValue(10f, true);

            Assert.AreEqual(10f, model.Value);
        }

        [Test]
        public void SetValue_ApproximatelyEqual_DoesNotFireChangedEvent()
        {
            var model = new NumberModel("Count", 1f, 1f, null);
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.SetValue(1f + 1e-8f, true);

            Assert.IsFalse(changedFired);
        }

        [Test]
        public void NotifyValueChangedFromView_FiresCallback()
        {
            float? receivedValue = null;
            var model = new NumberModel("Count", 10f, 1f, v => receivedValue = v);

            model.NotifyValueChangedFromView(30f);

            Assert.AreEqual(30f, receivedValue);
        }

        [Test]
        public void NotifyValueChangedFromView_ChangesValue()
        {
            var model = new NumberModel("Count", 10f, 1f, null);

            model.NotifyValueChangedFromView(30f);

            Assert.AreEqual(30f, model.Value);
        }

        [Test]
        public void NotifyValueChangedFromView_FiresChangedEvent()
        {
            var model = new NumberModel("Count", 10f, 1f, null);
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.NotifyValueChangedFromView(30f);

            Assert.IsTrue(changedFired);
        }

        [Test]
        public void NotifyValueChangedFromView_SameValue_DoesNotFireCallback()
        {
            var callbackFired = false;
            var model = new NumberModel("Count", 10f, 1f, _ => callbackFired = true);

            model.NotifyValueChangedFromView(10f);

            Assert.IsFalse(callbackFired);
        }

        [Test]
        public void SetValue_NullCallback_DoesNotThrow()
        {
            var model = new NumberModel("Count", 10f, 1f, null);

            Assert.DoesNotThrow(() => model.SetValue(20f, true));
        }
    }
}
