using NUnit.Framework;
using UnityEngine;
using Xeon.XGraph.Model;

namespace Xeon.XDebugger.Tests
{
    [TestFixture]
    public class RangeValueTests
    {
        // --- Default constructor tests ---

        [Test]
        public void DefaultConstructor_MinIsZero()
        {
            var range = new RangeValue();

            Assert.AreEqual(0f, range.Min);
        }

        [Test]
        public void DefaultConstructor_MaxIsHundred()
        {
            var range = new RangeValue();

            Assert.AreEqual(100f, range.Max);
        }

        [Test]
        public void DefaultConstructor_RangeIsZero()
        {
            // Default constructor does not set range field (left at 0)
            var range = new RangeValue();

            Assert.AreEqual(0f, range.Range);
        }

        [Test]
        public void DefaultConstructor_IsApproximatelyFalse()
        {
            var range = new RangeValue();

            Assert.IsFalse(range.IsApproximately);
        }

        // --- Parameterized constructor tests ---

        [Test]
        public void ParameterizedConstructor_SetsMin()
        {
            var range = new RangeValue(10f, 50f);

            Assert.AreEqual(10f, range.Min);
        }

        [Test]
        public void ParameterizedConstructor_SetsMax()
        {
            var range = new RangeValue(10f, 50f);

            Assert.AreEqual(50f, range.Max);
        }

        [Test]
        public void ParameterizedConstructor_CalculatesRange()
        {
            var range = new RangeValue(10f, 50f);

            Assert.AreEqual(40f, range.Range);
        }

        [Test]
        public void ParameterizedConstructor_NegativeValues()
        {
            var range = new RangeValue(-100f, -20f);

            Assert.AreEqual(-100f, range.Min);
            Assert.AreEqual(-20f, range.Max);
            Assert.AreEqual(80f, range.Range);
        }

        [Test]
        public void ParameterizedConstructor_ZeroWidth()
        {
            var range = new RangeValue(5f, 5f);

            Assert.AreEqual(0f, range.Range);
            Assert.IsTrue(range.IsApproximately);
        }

        // --- Min setter tests ---

        [Test]
        public void MinSetter_UpdatesMin()
        {
            var range = new RangeValue(0f, 100f);

            range.Min = 20f;

            Assert.AreEqual(20f, range.Min);
        }

        [Test]
        public void MinSetter_RecalculatesRange()
        {
            var range = new RangeValue(0f, 100f);

            range.Min = 40f;

            Assert.AreEqual(60f, range.Range);
        }

        [Test]
        public void MinSetter_UpdatesIsApproximately()
        {
            var range = new RangeValue(0f, 100f);

            range.Min = 100f;

            Assert.IsTrue(range.IsApproximately);
        }

        // --- Max setter tests ---

        [Test]
        public void MaxSetter_UpdatesMax()
        {
            var range = new RangeValue(0f, 100f);

            range.Max = 200f;

            Assert.AreEqual(200f, range.Max);
        }

        [Test]
        public void MaxSetter_RecalculatesRange()
        {
            var range = new RangeValue(0f, 100f);

            range.Max = 50f;

            Assert.AreEqual(50f, range.Range);
        }

        [Test]
        public void MaxSetter_UpdatesIsApproximately()
        {
            var range = new RangeValue(0f, 100f);

            range.Max = 0f;

            Assert.IsTrue(range.IsApproximately);
        }

        // --- Normalized tests ---

        [Test]
        public void Normalized_MidValue_ReturnsHalf()
        {
            var range = new RangeValue(0f, 100f);

            Assert.AreEqual(0.5f, range.Normalized(50f), 0.0001f);
        }

        [Test]
        public void Normalized_MinValue_ReturnsZero()
        {
            var range = new RangeValue(0f, 100f);

            Assert.AreEqual(0f, range.Normalized(0f), 0.0001f);
        }

        [Test]
        public void Normalized_MaxValue_ReturnsOne()
        {
            var range = new RangeValue(0f, 100f);

            Assert.AreEqual(1f, range.Normalized(100f), 0.0001f);
        }

        [Test]
        public void Normalized_WithOffset_CalculatesCorrectly()
        {
            var range = new RangeValue(20f, 60f);

            // (30 - 20) / 40 = 0.25
            Assert.AreEqual(0.25f, range.Normalized(30f), 0.0001f);
        }

        [Test]
        public void Normalized_BelowMin_ReturnsNegative()
        {
            var range = new RangeValue(10f, 20f);

            // (5 - 10) / 10 = -0.5
            Assert.AreEqual(-0.5f, range.Normalized(5f), 0.0001f);
        }

        [Test]
        public void Normalized_AboveMax_ReturnsGreaterThanOne()
        {
            var range = new RangeValue(0f, 50f);

            // 100 / 50 = 2.0
            Assert.AreEqual(2f, range.Normalized(100f), 0.0001f);
        }

        // --- InRange tests ---

        [Test]
        public void InRange_ValueInside_ReturnsTrue()
        {
            var range = new RangeValue(0f, 100f);

            Assert.IsTrue(range.InRange(50f));
        }

        [Test]
        public void InRange_ValueAtMin_ReturnsTrue()
        {
            var range = new RangeValue(0f, 100f);

            Assert.IsTrue(range.InRange(0f));
        }

        [Test]
        public void InRange_ValueAtMax_ReturnsTrue()
        {
            var range = new RangeValue(0f, 100f);

            Assert.IsTrue(range.InRange(100f));
        }

        [Test]
        public void InRange_ValueBelowMin_ReturnsFalse()
        {
            var range = new RangeValue(0f, 100f);

            Assert.IsFalse(range.InRange(-1f));
        }

        [Test]
        public void InRange_ValueAboveMax_ReturnsFalse()
        {
            var range = new RangeValue(0f, 100f);

            Assert.IsFalse(range.InRange(101f));
        }

        [Test]
        public void InRange_NegativeRange_ValueInside_ReturnsTrue()
        {
            var range = new RangeValue(-50f, -10f);

            Assert.IsTrue(range.InRange(-30f));
        }

        [Test]
        public void InRange_NegativeRange_ValueOutside_ReturnsFalse()
        {
            var range = new RangeValue(-50f, -10f);

            Assert.IsFalse(range.InRange(0f));
        }

        // --- IsApproximately tests ---

        [Test]
        public void IsApproximately_DifferentMinMax_ReturnsFalse()
        {
            var range = new RangeValue(0f, 100f);

            Assert.IsFalse(range.IsApproximately);
        }

        [Test]
        public void IsApproximately_SameMinMax_ReturnsTrue()
        {
            var range = new RangeValue(42f, 42f);

            Assert.IsTrue(range.IsApproximately);
        }

        [Test]
        public void IsApproximately_WhenTrue_InRangeAlwaysReturnsFalse()
        {
            var range = new RangeValue(42f, 42f);

            Assert.IsTrue(range.IsApproximately);
            Assert.IsFalse(range.InRange(42f));
            Assert.IsFalse(range.InRange(0f));
            Assert.IsFalse(range.InRange(100f));
        }

        [Test]
        public void IsApproximately_UpdatedWhenMinChanges()
        {
            var range = new RangeValue(0f, 100f);
            Assert.IsFalse(range.IsApproximately);

            range.Min = 100f;

            Assert.IsTrue(range.IsApproximately);
        }

        [Test]
        public void IsApproximately_UpdatedWhenMaxChanges()
        {
            var range = new RangeValue(0f, 100f);
            Assert.IsFalse(range.IsApproximately);

            range.Max = 0f;

            Assert.IsTrue(range.IsApproximately);
        }

        // --- Edge case: negative range (min > max) ---

        [Test]
        public void NegativeRange_MinGreaterThanMax()
        {
            var range = new RangeValue(100f, 50f);

            Assert.AreEqual(-50f, range.Range);
        }

        [Test]
        public void NegativeRange_InRange_ReturnsFalse()
        {
            // When min > max, no value can satisfy min <= value <= max
            var range = new RangeValue(100f, 50f);

            Assert.IsFalse(range.InRange(75f));
            Assert.IsFalse(range.InRange(100f));
            Assert.IsFalse(range.InRange(50f));
        }

        // --- Edge case: zero at origin ---

        [Test]
        public void ZeroRange_AtZero()
        {
            var range = new RangeValue(0f, 0f);

            Assert.AreEqual(0f, range.Range);
            Assert.IsTrue(range.IsApproximately);
            Assert.IsFalse(range.InRange(0f));
        }

        // --- Setter recalculation round-trip ---

        [Test]
        public void SetterRoundTrip_MinThenMax_RecalculatesCorrectly()
        {
            var range = new RangeValue(0f, 100f);

            range.Min = 25f;
            range.Max = 75f;

            Assert.AreEqual(50f, range.Range);
            Assert.IsFalse(range.IsApproximately);
            Assert.IsTrue(range.InRange(50f));
            Assert.IsFalse(range.InRange(24f));
            Assert.IsFalse(range.InRange(76f));
        }
    }
}
