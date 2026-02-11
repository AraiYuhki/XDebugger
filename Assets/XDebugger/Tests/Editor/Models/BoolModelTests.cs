using System;
using NUnit.Framework;
using Xeon.XDebugger.Model;

namespace Xeon.XDebugger.Tests
{
    [TestFixture]
    public class BoolModelTests
    {
        [Test]
        public void Constructor_SetsTitle()
        {
            var model = new BoolModel("Toggle", false, null);
            Assert.AreEqual("Toggle", model.Title);
        }

        [Test]
        public void Constructor_SetsInitialValue()
        {
            var model = new BoolModel("Toggle", true, null);
            Assert.IsTrue(model.Value);
        }

        [Test]
        public void Constructor_SetsInitialValueFalse()
        {
            var model = new BoolModel("Toggle", false, null);
            Assert.IsFalse(model.Value);
        }

        [Test]
        public void Constructor_SetsPriority()
        {
            var model = new BoolModel("Toggle", false, null, 5);
            Assert.AreEqual(5, model.Priority);
        }

        [Test]
        public void Constructor_DefaultPriorityIsZero()
        {
            var model = new BoolModel("Toggle", false, null);
            Assert.AreEqual(0, model.Priority);
        }

        [Test]
        public void ValueProperty_Set_DoesNotFireCallback()
        {
            var callbackFired = false;
            var model = new BoolModel("Toggle", false, _ => callbackFired = true);

            model.Value = true;

            Assert.IsFalse(callbackFired);
        }

        [Test]
        public void ValueProperty_Set_ChangesValue()
        {
            var model = new BoolModel("Toggle", false, null);

            model.Value = true;

            Assert.IsTrue(model.Value);
        }

        [Test]
        public void ValueProperty_Set_FiresChangedEvent()
        {
            var model = new BoolModel("Toggle", false, null);
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.Value = true;

            Assert.IsTrue(changedFired);
        }

        [Test]
        public void SetValue_WithNotifyCallbackTrue_FiresCallback()
        {
            bool? receivedValue = null;
            var model = new BoolModel("Toggle", false, v => receivedValue = v);

            model.SetValue(true, true);

            Assert.AreEqual(true, receivedValue);
        }

        [Test]
        public void SetValue_WithNotifyCallbackFalse_DoesNotFireCallback()
        {
            var callbackFired = false;
            var model = new BoolModel("Toggle", false, _ => callbackFired = true);

            model.SetValue(true, false);

            Assert.IsFalse(callbackFired);
        }

        [Test]
        public void SetValue_WithNotifyCallbackTrue_FiresChangedEvent()
        {
            var model = new BoolModel("Toggle", false, null);
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.SetValue(true, true);

            Assert.IsTrue(changedFired);
        }

        [Test]
        public void SetValue_SameValue_DoesNotFireChangedEvent()
        {
            var model = new BoolModel("Toggle", false, null);
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.SetValue(false, true);

            Assert.IsFalse(changedFired);
        }

        [Test]
        public void SetValue_SameValue_DoesNotFireCallback()
        {
            var callbackFired = false;
            var model = new BoolModel("Toggle", true, _ => callbackFired = true);

            model.SetValue(true, true);

            Assert.IsFalse(callbackFired);
        }

        [Test]
        public void SetValue_SameValue_ValueUnchanged()
        {
            var model = new BoolModel("Toggle", true, null);

            model.SetValue(true, true);

            Assert.IsTrue(model.Value);
        }

        [Test]
        public void NotifyValueChangedFromView_FiresCallback()
        {
            bool? receivedValue = null;
            var model = new BoolModel("Toggle", false, v => receivedValue = v);

            model.NotifyValueChangedFromView(true);

            Assert.AreEqual(true, receivedValue);
        }

        [Test]
        public void NotifyValueChangedFromView_ChangesValue()
        {
            var model = new BoolModel("Toggle", false, null);

            model.NotifyValueChangedFromView(true);

            Assert.IsTrue(model.Value);
        }

        [Test]
        public void NotifyValueChangedFromView_FiresChangedEvent()
        {
            var model = new BoolModel("Toggle", false, null);
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.NotifyValueChangedFromView(true);

            Assert.IsTrue(changedFired);
        }

        [Test]
        public void NotifyValueChangedFromView_SameValue_DoesNotFireCallback()
        {
            var callbackFired = false;
            var model = new BoolModel("Toggle", true, _ => callbackFired = true);

            model.NotifyValueChangedFromView(true);

            Assert.IsFalse(callbackFired);
        }

        [Test]
        public void SetValue_NullCallback_DoesNotThrow()
        {
            var model = new BoolModel("Toggle", false, null);

            Assert.DoesNotThrow(() => model.SetValue(true, true));
        }
    }
}
