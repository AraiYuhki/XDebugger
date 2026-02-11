using System;
using NUnit.Framework;
using UnityEngine;
using Xeon.XDebugger.Model;

namespace Xeon.XDebugger.Tests
{
    [TestFixture]
    public class FloatSliderModelTests
    {
        [Test]
        public void Constructor_SetsValue()
        {
            var model = new FloatSliderModel("Slider", 0.5f, 0f, 1f, null);
            Assert.AreEqual(0.5f, model.Value);
        }

        [Test]
        public void Constructor_SetsMin()
        {
            var model = new FloatSliderModel("Slider", 0.5f, 0f, 1f, null);
            Assert.AreEqual(0f, model.Min);
        }

        [Test]
        public void Constructor_SetsMax()
        {
            var model = new FloatSliderModel("Slider", 0.5f, 0f, 1f, null);
            Assert.AreEqual(1f, model.Max);
        }

        [Test]
        public void Constructor_SetsDigits()
        {
            var model = new FloatSliderModel("Slider", 0.5f, 0f, 1f, null, 3);
            Assert.AreEqual(3, model.Digits);
        }

        [Test]
        public void Constructor_DefaultDigitsIsTwo()
        {
            var model = new FloatSliderModel("Slider", 0.5f, 0f, 1f, null);
            Assert.AreEqual(2, model.Digits);
        }

        [Test]
        public void Constructor_SetsTitle()
        {
            var model = new FloatSliderModel("Volume", 0.5f, 0f, 1f, null);
            Assert.AreEqual("Volume", model.Title);
        }

        [Test]
        public void Constructor_SetsPriority()
        {
            var model = new FloatSliderModel("Slider", 0.5f, 0f, 1f, null, 2, 10);
            Assert.AreEqual(10, model.Priority);
        }

        [Test]
        public void ValueProperty_Get_ReturnsCurrentValue()
        {
            var model = new FloatSliderModel("Slider", 0.75f, 0f, 1f, null);
            Assert.AreEqual(0.75f, model.Value);
        }

        [Test]
        public void ValueProperty_Set_ChangesValue()
        {
            var model = new FloatSliderModel("Slider", 0.5f, 0f, 1f, null);

            model.Value = 0.8f;

            Assert.AreEqual(0.8f, model.Value);
        }

        [Test]
        public void ValueProperty_Set_DoesNotFireCallback()
        {
            var callbackFired = false;
            var model = new FloatSliderModel("Slider", 0.5f, 0f, 1f, _ => callbackFired = true);

            model.Value = 0.8f;

            Assert.IsFalse(callbackFired);
        }

        [Test]
        public void ValueProperty_Set_FiresChangedEvent()
        {
            var model = new FloatSliderModel("Slider", 0.5f, 0f, 1f, null);
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.Value = 0.8f;

            Assert.IsTrue(changedFired);
        }

        [Test]
        public void SetValue_WithNotifyCallbackTrue_FiresCallback()
        {
            float? receivedValue = null;
            var model = new FloatSliderModel("Slider", 0.5f, 0f, 1f, v => receivedValue = v);

            model.SetValue(0.8f, true);

            Assert.AreEqual(0.8f, receivedValue);
        }

        [Test]
        public void SetValue_WithNotifyCallbackFalse_DoesNotFireCallback()
        {
            var callbackFired = false;
            var model = new FloatSliderModel("Slider", 0.5f, 0f, 1f, _ => callbackFired = true);

            model.SetValue(0.8f, false);

            Assert.IsFalse(callbackFired);
        }

        [Test]
        public void SetValue_ApproximatelySameValue_DoesNotFireChangedEvent()
        {
            var model = new FloatSliderModel("Slider", 0.5f, 0f, 1f, null);
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.SetValue(0.5f, true);

            Assert.IsFalse(changedFired);
        }

        [Test]
        public void SetValue_ApproximatelySameValue_DoesNotFireCallback()
        {
            var callbackFired = false;
            var model = new FloatSliderModel("Slider", 0.5f, 0f, 1f, _ => callbackFired = true);

            model.SetValue(0.5f, true);

            Assert.IsFalse(callbackFired);
        }

        [Test]
        public void SetValue_NearlyEqualValue_DoesNotFireChangedEvent()
        {
            var value = 1f / 3f;
            var model = new FloatSliderModel("Slider", value, 0f, 1f, null);
            var changedFired = false;
            model.Changed += () => changedFired = true;

            // Mathf.Approximately should treat these as equal
            model.SetValue(value + Mathf.Epsilon * 0.5f, true);

            Assert.IsFalse(changedFired);
        }

        [Test]
        public void SetMin_WithRefreshTrue_FiresChangedEvent()
        {
            var model = new FloatSliderModel("Slider", 0.5f, 0f, 1f, null);
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.SetMin(-1f, true);

            Assert.IsTrue(changedFired);
            Assert.AreEqual(-1f, model.Min);
        }

        [Test]
        public void SetMin_WithRefreshFalse_DoesNotFireChangedEvent()
        {
            var model = new FloatSliderModel("Slider", 0.5f, 0f, 1f, null);
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.SetMin(-1f, false);

            Assert.IsFalse(changedFired);
            Assert.AreEqual(-1f, model.Min);
        }

        [Test]
        public void SetMin_DefaultRefreshIsTrue()
        {
            var model = new FloatSliderModel("Slider", 0.5f, 0f, 1f, null);
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.SetMin(-1f);

            Assert.IsTrue(changedFired);
        }

        [Test]
        public void SetMax_WithRefreshTrue_FiresChangedEvent()
        {
            var model = new FloatSliderModel("Slider", 0.5f, 0f, 1f, null);
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.SetMax(2f, true);

            Assert.IsTrue(changedFired);
            Assert.AreEqual(2f, model.Max);
        }

        [Test]
        public void SetMax_WithRefreshFalse_DoesNotFireChangedEvent()
        {
            var model = new FloatSliderModel("Slider", 0.5f, 0f, 1f, null);
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.SetMax(2f, false);

            Assert.IsFalse(changedFired);
            Assert.AreEqual(2f, model.Max);
        }

        [Test]
        public void SetMax_DefaultRefreshIsTrue()
        {
            var model = new FloatSliderModel("Slider", 0.5f, 0f, 1f, null);
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.SetMax(2f);

            Assert.IsTrue(changedFired);
        }

        [Test]
        public void GetRoundedText_ReturnsRoundedValue()
        {
            var model = new FloatSliderModel("Slider", 0.5678f, 0f, 1f, null, 2);
            Assert.AreEqual(Math.Round(0.5678, 2).ToString(), model.GetRoundedText);
        }

        [Test]
        public void GetRoundedText_RespectsDigitsSetting()
        {
            var model = new FloatSliderModel("Slider", 0.12345f, 0f, 1f, null, 3);
            Assert.AreEqual(Math.Round(0.12345, 3).ToString(), model.GetRoundedText);
        }

        [Test]
        public void GetRoundedText_ZeroDigits()
        {
            var model = new FloatSliderModel("Slider", 3.7f, 0f, 10f, null, 0);
            Assert.AreEqual(Math.Round(3.7, 0).ToString(), model.GetRoundedText);
        }

        [Test]
        public void NotifyValueChangedFromView_FiresCallback()
        {
            float? receivedValue = null;
            var model = new FloatSliderModel("Slider", 0.5f, 0f, 1f, v => receivedValue = v);

            model.NotifyValueChangedFromView(0.9f);

            Assert.AreEqual(0.9f, receivedValue);
        }

        [Test]
        public void NotifyValueChangedFromView_ChangesValue()
        {
            var model = new FloatSliderModel("Slider", 0.5f, 0f, 1f, null);

            model.NotifyValueChangedFromView(0.9f);

            Assert.AreEqual(0.9f, model.Value);
        }

        [Test]
        public void NotifyValueChangedFromView_FiresChangedEvent()
        {
            var model = new FloatSliderModel("Slider", 0.5f, 0f, 1f, null);
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.NotifyValueChangedFromView(0.9f);

            Assert.IsTrue(changedFired);
        }

        [Test]
        public void NotifyValueChangedFromView_SameValue_DoesNotFireCallback()
        {
            var callbackFired = false;
            var model = new FloatSliderModel("Slider", 0.5f, 0f, 1f, _ => callbackFired = true);

            model.NotifyValueChangedFromView(0.5f);

            Assert.IsFalse(callbackFired);
        }

        [Test]
        public void SetValue_NullCallback_DoesNotThrow()
        {
            var model = new FloatSliderModel("Slider", 0.5f, 0f, 1f, null);

            Assert.DoesNotThrow(() => model.SetValue(0.8f, true));
        }
    }
}
