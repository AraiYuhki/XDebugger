using System;
using NUnit.Framework;
using Xeon.XDebugger.Model;

namespace Xeon.XDebugger.Tests
{
    [TestFixture]
    public class IntSliderModelTests
    {
        [Test]
        public void Constructor_SetsValue()
        {
            var model = new IntSliderModel("Slider", 5, 0, 10, null);
            Assert.AreEqual(5, model.Value);
        }

        [Test]
        public void Constructor_SetsMin()
        {
            var model = new IntSliderModel("Slider", 5, 0, 10, null);
            Assert.AreEqual(0, model.Min);
        }

        [Test]
        public void Constructor_SetsMax()
        {
            var model = new IntSliderModel("Slider", 5, 0, 10, null);
            Assert.AreEqual(10, model.Max);
        }

        [Test]
        public void Constructor_SetsTitle()
        {
            var model = new IntSliderModel("Count", 5, 0, 10, null);
            Assert.AreEqual("Count", model.Title);
        }

        [Test]
        public void Constructor_SetsPriority()
        {
            var model = new IntSliderModel("Slider", 5, 0, 10, null, 7);
            Assert.AreEqual(7, model.Priority);
        }

        [Test]
        public void Constructor_DefaultPriorityIsZero()
        {
            var model = new IntSliderModel("Slider", 5, 0, 10, null);
            Assert.AreEqual(0, model.Priority);
        }

        [Test]
        public void ValueProperty_Get_ReturnsCurrentValue()
        {
            var model = new IntSliderModel("Slider", 3, 0, 10, null);
            Assert.AreEqual(3, model.Value);
        }

        [Test]
        public void ValueProperty_Set_ChangesValue()
        {
            var model = new IntSliderModel("Slider", 5, 0, 10, null);

            model.Value = 8;

            Assert.AreEqual(8, model.Value);
        }

        [Test]
        public void ValueProperty_Set_DoesNotFireCallback()
        {
            var callbackFired = false;
            var model = new IntSliderModel("Slider", 5, 0, 10, _ => callbackFired = true);

            model.Value = 8;

            Assert.IsFalse(callbackFired);
        }

        [Test]
        public void ValueProperty_Set_FiresChangedEvent()
        {
            var model = new IntSliderModel("Slider", 5, 0, 10, null);
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.Value = 8;

            Assert.IsTrue(changedFired);
        }

        [Test]
        public void SetValue_WithNotifyCallbackTrue_FiresCallback()
        {
            int? receivedValue = null;
            var model = new IntSliderModel("Slider", 5, 0, 10, v => receivedValue = v);

            model.SetValue(8, true);

            Assert.AreEqual(8, receivedValue);
        }

        [Test]
        public void SetValue_WithNotifyCallbackFalse_DoesNotFireCallback()
        {
            var callbackFired = false;
            var model = new IntSliderModel("Slider", 5, 0, 10, _ => callbackFired = true);

            model.SetValue(8, false);

            Assert.IsFalse(callbackFired);
        }

        [Test]
        public void SetValue_WithNotifyCallbackTrue_FiresChangedEvent()
        {
            var model = new IntSliderModel("Slider", 5, 0, 10, null);
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.SetValue(8, true);

            Assert.IsTrue(changedFired);
        }

        [Test]
        public void SetValue_SameValue_DoesNotFireChangedEvent()
        {
            var model = new IntSliderModel("Slider", 5, 0, 10, null);
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.SetValue(5, true);

            Assert.IsFalse(changedFired);
        }

        [Test]
        public void SetValue_SameValue_DoesNotFireCallback()
        {
            var callbackFired = false;
            var model = new IntSliderModel("Slider", 5, 0, 10, _ => callbackFired = true);

            model.SetValue(5, true);

            Assert.IsFalse(callbackFired);
        }

        [Test]
        public void SetMin_WithRefreshTrue_FiresChangedEvent()
        {
            var model = new IntSliderModel("Slider", 5, 0, 10, null);
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.SetMin(-5, true);

            Assert.IsTrue(changedFired);
            Assert.AreEqual(-5, model.Min);
        }

        [Test]
        public void SetMin_WithRefreshFalse_DoesNotFireChangedEvent()
        {
            var model = new IntSliderModel("Slider", 5, 0, 10, null);
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.SetMin(-5, false);

            Assert.IsFalse(changedFired);
            Assert.AreEqual(-5, model.Min);
        }

        [Test]
        public void SetMin_DefaultRefreshIsTrue()
        {
            var model = new IntSliderModel("Slider", 5, 0, 10, null);
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.SetMin(-5);

            Assert.IsTrue(changedFired);
        }

        [Test]
        public void SetMax_WithRefreshTrue_FiresChangedEvent()
        {
            var model = new IntSliderModel("Slider", 5, 0, 10, null);
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.SetMax(20, true);

            Assert.IsTrue(changedFired);
            Assert.AreEqual(20, model.Max);
        }

        [Test]
        public void SetMax_WithRefreshFalse_DoesNotFireChangedEvent()
        {
            var model = new IntSliderModel("Slider", 5, 0, 10, null);
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.SetMax(20, false);

            Assert.IsFalse(changedFired);
            Assert.AreEqual(20, model.Max);
        }

        [Test]
        public void SetMax_DefaultRefreshIsTrue()
        {
            var model = new IntSliderModel("Slider", 5, 0, 10, null);
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.SetMax(20);

            Assert.IsTrue(changedFired);
        }

        [Test]
        public void NotifyValueChangedFromView_FiresCallback()
        {
            int? receivedValue = null;
            var model = new IntSliderModel("Slider", 5, 0, 10, v => receivedValue = v);

            model.NotifyValueChangedFromView(8);

            Assert.AreEqual(8, receivedValue);
        }

        [Test]
        public void NotifyValueChangedFromView_ChangesValue()
        {
            var model = new IntSliderModel("Slider", 5, 0, 10, null);

            model.NotifyValueChangedFromView(8);

            Assert.AreEqual(8, model.Value);
        }

        [Test]
        public void NotifyValueChangedFromView_FiresChangedEvent()
        {
            var model = new IntSliderModel("Slider", 5, 0, 10, null);
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.NotifyValueChangedFromView(8);

            Assert.IsTrue(changedFired);
        }

        [Test]
        public void NotifyValueChangedFromView_SameValue_DoesNotFireCallback()
        {
            var callbackFired = false;
            var model = new IntSliderModel("Slider", 5, 0, 10, _ => callbackFired = true);

            model.NotifyValueChangedFromView(5);

            Assert.IsFalse(callbackFired);
        }

        [Test]
        public void SetValue_NullCallback_DoesNotThrow()
        {
            var model = new IntSliderModel("Slider", 5, 0, 10, null);

            Assert.DoesNotThrow(() => model.SetValue(8, true));
        }
    }
}
