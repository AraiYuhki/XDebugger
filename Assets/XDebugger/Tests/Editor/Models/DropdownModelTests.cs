using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xeon.XDebugger.Model;

namespace Xeon.XDebugger.Tests
{
    [TestFixture]
    public class DropdownModelTests
    {
        private static readonly string[] Labels = { "One", "Two", "Three" };
        private static readonly int[] Options = { 1, 2, 3 };

        [Test]
        public void Constructor_WithLabelsAndOptions_SetsTitle()
        {
            var model = new DropdownModel<int>("Dropdown", 0, Labels, Options, null);
            Assert.AreEqual("Dropdown", model.Title);
        }

        [Test]
        public void Constructor_WithLabelsAndOptions_SetsSelectedIndex()
        {
            var model = new DropdownModel<int>("Dropdown", 1, Labels, Options, null);
            Assert.AreEqual(1, model.SelectedIndex);
        }

        [Test]
        public void Constructor_WithLabelsAndOptions_SetsLabels()
        {
            var model = new DropdownModel<int>("Dropdown", 0, Labels, Options, null);
            Assert.AreEqual(Labels.ToList(), model.Labels);
        }

        [Test]
        public void Constructor_WithLabelsAndOptions_SetsSelectedItem()
        {
            var model = new DropdownModel<int>("Dropdown", 1, Labels, Options, null);
            Assert.AreEqual(2, model.SelectedItem);
        }

        [Test]
        public void Constructor_WithLabelsAndOptions_SetsPriority()
        {
            var model = new DropdownModel<int>("Dropdown", 0, Labels, Options, null, 5);
            Assert.AreEqual(5, model.Priority);
        }

        [Test]
        public void Constructor_OptionsOnly_GeneratesLabelsFromToString()
        {
            var model = new DropdownModel<int>("Dropdown", 0, Options, null);
            var expected = Options.Select(o => o.ToString()).ToList();
            Assert.AreEqual(expected, model.Labels);
        }

        [Test]
        public void Constructor_OptionsOnly_SetsSelectedItem()
        {
            var model = new DropdownModel<int>("Dropdown", 2, Options, null);
            Assert.AreEqual(3, model.SelectedItem);
        }

        [Test]
        public void Constructor_StringOptions_LabelsMatchOptions()
        {
            var options = new[] { "Alpha", "Beta", "Gamma" };
            var model = new DropdownModel<string>("Dropdown", 0, options, null);
            Assert.AreEqual(options.ToList(), model.Labels);
        }

        [Test]
        public void SelectedIndex_Set_DoesNotFireCallback()
        {
            var callbackFired = false;
            var model = new DropdownModel<int>("Dropdown", 0, Labels, Options, _ => callbackFired = true);

            model.SelectedIndex = 2;

            Assert.IsFalse(callbackFired);
        }

        [Test]
        public void SelectedIndex_Set_ChangesValue()
        {
            var model = new DropdownModel<int>("Dropdown", 0, Labels, Options, null);

            model.SelectedIndex = 2;

            Assert.AreEqual(2, model.SelectedIndex);
            Assert.AreEqual(3, model.SelectedItem);
        }

        [Test]
        public void SelectedIndex_Set_FiresChangedEvent()
        {
            var model = new DropdownModel<int>("Dropdown", 0, Labels, Options, null);
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.SelectedIndex = 1;

            Assert.IsTrue(changedFired);
        }

        [Test]
        public void SetSelectedIndex_ClampsToZeroWhenNegative()
        {
            var model = new DropdownModel<int>("Dropdown", 0, Labels, Options, null);

            model.SetSelectedIndex(-5, false);

            Assert.AreEqual(0, model.SelectedIndex);
        }

        [Test]
        public void SetSelectedIndex_ClampsToMaxWhenExceedsRange()
        {
            var model = new DropdownModel<int>("Dropdown", 0, Labels, Options, null);

            model.SetSelectedIndex(100, false);

            Assert.AreEqual(2, model.SelectedIndex);
        }

        [Test]
        public void SetSelectedIndex_WithNotifyTrue_FiresCallback()
        {
            int? receivedValue = null;
            var model = new DropdownModel<int>("Dropdown", 0, Labels, Options, v => receivedValue = v);

            model.SetSelectedIndex(1, true);

            Assert.AreEqual(2, receivedValue);
        }

        [Test]
        public void SetSelectedIndex_WithNotifyFalse_DoesNotFireCallback()
        {
            var callbackFired = false;
            var model = new DropdownModel<int>("Dropdown", 0, Labels, Options, _ => callbackFired = true);

            model.SetSelectedIndex(1, false);

            Assert.IsFalse(callbackFired);
        }

        [Test]
        public void SetSelectedIndex_FiresChangedEvent()
        {
            var model = new DropdownModel<int>("Dropdown", 0, Labels, Options, null);
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.SetSelectedIndex(2, true);

            Assert.IsTrue(changedFired);
        }

        [Test]
        public void SetSelectedIndex_NullCallback_DoesNotThrow()
        {
            var model = new DropdownModel<int>("Dropdown", 0, Labels, Options, null);

            Assert.DoesNotThrow(() => model.SetSelectedIndex(1, true));
        }

        [Test]
        public void SetOptions_WithLabels_UpdatesLabelsAndOptions()
        {
            var model = new DropdownModel<int>("Dropdown", 0, Labels, Options, null);
            var newLabels = new[] { "A", "B" };
            var newOptions = new[] { 10, 20 };

            model.SetOptions(newLabels, newOptions);

            Assert.AreEqual(newLabels.ToList(), model.Labels);
            Assert.AreEqual(10, model.SelectedItem);
        }

        [Test]
        public void SetOptions_WithLabels_IsRefreshControlTrue_FiresChangedEvent()
        {
            var model = new DropdownModel<int>("Dropdown", 0, Labels, Options, null);
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.SetOptions(new[] { "A" }, new[] { 10 }, true);

            Assert.IsTrue(changedFired);
        }

        [Test]
        public void SetOptions_WithLabels_IsRefreshControlFalse_DoesNotFireChangedEvent()
        {
            var model = new DropdownModel<int>("Dropdown", 0, Labels, Options, null);
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.SetOptions(new[] { "A" }, new[] { 10 }, false);

            Assert.IsFalse(changedFired);
        }

        [Test]
        public void SetOptions_OptionsOnly_GeneratesLabelsFromToString()
        {
            var model = new DropdownModel<int>("Dropdown", 0, Labels, Options, null);
            var newOptions = new[] { 100, 200 };

            model.SetOptions(newOptions);

            Assert.AreEqual(new List<string> { "100", "200" }, model.Labels);
        }

        [Test]
        public void SetOptions_OptionsOnly_IsRefreshControlTrue_FiresChangedEvent()
        {
            var model = new DropdownModel<int>("Dropdown", 0, Labels, Options, null);
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.SetOptions(new[] { 10 }, true);

            Assert.IsTrue(changedFired);
        }

        [Test]
        public void SetOptions_OptionsOnly_IsRefreshControlFalse_DoesNotFireChangedEvent()
        {
            var model = new DropdownModel<int>("Dropdown", 0, Labels, Options, null);
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.SetOptions(new[] { 10 }, false);

            Assert.IsFalse(changedFired);
        }

        [Test]
        public void NotifySelectedIndexChangedFromView_FiresCallback()
        {
            int? receivedValue = null;
            var model = new DropdownModel<int>("Dropdown", 0, Labels, Options, v => receivedValue = v);

            model.NotifySelectedIndexChangedFromView(2);

            Assert.AreEqual(3, receivedValue);
        }

        [Test]
        public void NotifySelectedIndexChangedFromView_ChangesSelectedIndex()
        {
            var model = new DropdownModel<int>("Dropdown", 0, Labels, Options, null);

            model.NotifySelectedIndexChangedFromView(1);

            Assert.AreEqual(1, model.SelectedIndex);
        }

        [Test]
        public void NotifySelectedIndexChangedFromView_FiresChangedEvent()
        {
            var model = new DropdownModel<int>("Dropdown", 0, Labels, Options, null);
            var changedFired = false;
            model.Changed += () => changedFired = true;

            model.NotifySelectedIndexChangedFromView(1);

            Assert.IsTrue(changedFired);
        }

        [Test]
        public void EmptyOptions_SetSelectedIndex_WithNotify_DoesNotFireCallback()
        {
            var callbackFired = false;
            var model = new DropdownModel<int>("Dropdown", 0, Labels, Options, _ => callbackFired = true);
            model.SetOptions(new string[0], new int[0]);
            callbackFired = false;

            model.SetSelectedIndex(0, true);

            Assert.IsFalse(callbackFired);
        }

        [Test]
        public void EmptyOptions_SelectedIndex_ClampedToZero()
        {
            var model = new DropdownModel<int>("Dropdown", 0, Labels, Options, null);
            model.SetOptions(new string[0], new int[0]);

            model.SetSelectedIndex(5, false);

            Assert.AreEqual(0, model.SelectedIndex);
        }
    }
}
