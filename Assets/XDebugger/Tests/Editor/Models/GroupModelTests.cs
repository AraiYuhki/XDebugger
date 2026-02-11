using System;
using NUnit.Framework;
using Xeon.XDebugger.Model;

namespace Xeon.XDebugger.Tests
{
    [TestFixture]
    public class GroupModelTests
    {
        // --- HorizontalGroupModel basic tests ---

        [Test]
        public void HorizontalGroupModel_Constructor_SetsTitle()
        {
            var group = new HorizontalGroupModel("Horizontal");

            Assert.AreEqual("Horizontal", group.Title);
        }

        [Test]
        public void HorizontalGroupModel_Constructor_SetsPriority()
        {
            var group = new HorizontalGroupModel("Horizontal", 5);

            Assert.AreEqual(5, group.Priority);
        }

        [Test]
        public void HorizontalGroupModel_Constructor_DefaultPriorityIsZero()
        {
            var group = new HorizontalGroupModel("Horizontal");

            Assert.AreEqual(0, group.Priority);
        }

        // --- VerticalGroupModel basic tests ---

        [Test]
        public void VerticalGroupModel_Constructor_SetsTitle()
        {
            var group = new VerticalGroupModel("Vertical");

            Assert.AreEqual("Vertical", group.Title);
        }

        [Test]
        public void VerticalGroupModel_Constructor_SetsPriority()
        {
            var group = new VerticalGroupModel("Vertical", 3);

            Assert.AreEqual(3, group.Priority);
        }

        // --- AddChild tests ---

        [Test]
        public void AddChild_IncreasesChildrenCount()
        {
            var group = new HorizontalGroupModel("Group");

            group.AddChild(new LabelModel("child1"));

            Assert.AreEqual(1, group.Children.Count);
        }

        [Test]
        public void AddChild_MultipleChildren_CountIncreases()
        {
            var group = new VerticalGroupModel("Group");

            group.AddChild(new LabelModel("child1"));
            group.AddChild(new LabelModel("child2"));
            group.AddChild(new LabelModel("child3"));

            Assert.AreEqual(3, group.Children.Count);
        }

        [Test]
        public void AddChild_ChildrenContainsAddedModel()
        {
            var group = new HorizontalGroupModel("Group");
            var label = new LabelModel("test");

            group.AddChild(label);

            Assert.AreEqual(label, group.Children[0]);
        }

        // --- Children readonly list tests ---

        [Test]
        public void Children_ReturnsReadOnlyList()
        {
            var group = new HorizontalGroupModel("Group");
            group.AddChild(new LabelModel("child"));

            var children = group.Children;

            Assert.IsNotNull(children);
            Assert.AreEqual(1, children.Count);
        }

        [Test]
        public void Children_InitiallyEmpty()
        {
            var group = new VerticalGroupModel("Group");

            Assert.AreEqual(0, group.Children.Count);
        }

        [Test]
        public void Children_PreservesOrder()
        {
            var group = new HorizontalGroupModel("Group");
            var label1 = new LabelModel("first");
            var label2 = new LabelModel("second");
            var label3 = new LabelModel("third");

            group.AddChild(label1);
            group.AddChild(label2);
            group.AddChild(label3);

            Assert.AreEqual("first", group.Children[0].Title);
            Assert.AreEqual("second", group.Children[1].Title);
            Assert.AreEqual("third", group.Children[2].Title);
        }

        // --- Nested groups tests ---

        [Test]
        public void NestedGroups_GroupInsideGroup()
        {
            var outer = new HorizontalGroupModel("Outer");
            var inner = new VerticalGroupModel("Inner");
            inner.AddChild(new LabelModel("nested child"));

            outer.AddChild(inner);

            Assert.AreEqual(1, outer.Children.Count);
            var innerGroup = outer.Children[0] as GroupModel;
            Assert.IsNotNull(innerGroup);
            Assert.AreEqual(1, innerGroup.Children.Count);
            Assert.AreEqual("nested child", innerGroup.Children[0].Title);
        }

        [Test]
        public void NestedGroups_MultipleNestingLevels()
        {
            var level1 = new HorizontalGroupModel("Level1");
            var level2 = new VerticalGroupModel("Level2");
            var level3 = new HorizontalGroupModel("Level3");
            level3.AddChild(new LabelModel("deep child"));

            level2.AddChild(level3);
            level1.AddChild(level2);

            var l2 = level1.Children[0] as GroupModel;
            Assert.IsNotNull(l2);
            var l3 = l2.Children[0] as GroupModel;
            Assert.IsNotNull(l3);
            Assert.AreEqual("deep child", l3.Children[0].Title);
        }

        // --- Parent constructor tests ---

        [Test]
        public void HorizontalGroupModel_ParentConstructor_SetsParent()
        {
            var parent = new VerticalGroupModel("Parent");
            var child = new HorizontalGroupModel("Child", parent);

            Assert.AreEqual(parent, child.Parent);
        }

        [Test]
        public void VerticalGroupModel_ParentConstructor_SetsParent()
        {
            var parent = new HorizontalGroupModel("Parent");
            var child = new VerticalGroupModel("Child", parent, 7);

            Assert.AreEqual(parent, child.Parent);
            Assert.AreEqual(7, child.Priority);
        }
    }

    [TestFixture]
    public class FoldingGroupModelTests
    {
        [Test]
        public void Constructor_SetsTitle()
        {
            var model = new FoldingGroupModel("Folding", false);

            Assert.AreEqual("Folding", model.Title);
        }

        [Test]
        public void Constructor_IsFolding_InitialValueFalse()
        {
            var model = new FoldingGroupModel("Folding", false);

            Assert.IsFalse(model.IsFolding);
        }

        [Test]
        public void Constructor_IsFolding_InitialValueTrue()
        {
            var model = new FoldingGroupModel("Folding", true);

            Assert.IsTrue(model.IsFolding);
        }

        [Test]
        public void Constructor_SetsPriority()
        {
            var model = new FoldingGroupModel("Folding", false, 10);

            Assert.AreEqual(10, model.Priority);
        }

        [Test]
        public void SetFoldingWithoutNotify_ChangesIsFolding()
        {
            var model = new FoldingGroupModel("Folding", false);

            model.SetFoldingWithoutNotify(true);

            Assert.IsTrue(model.IsFolding);
        }

        [Test]
        public void SetFoldingWithoutNotify_FiresOnChangedFoldingEvent()
        {
            var model = new FoldingGroupModel("Folding", false);
            bool? receivedValue = null;
            model.OnChangedFolding += v => receivedValue = v;

            // OnChangedFolding subscription immediately invokes with current value,
            // so reset before test action
            receivedValue = null;

            model.SetFoldingWithoutNotify(true);

            Assert.AreEqual(true, receivedValue);
        }

        [Test]
        public void OnChangedFolding_SubscriptionReceivesCurrentValue()
        {
            var model = new FoldingGroupModel("Folding", true);
            bool? receivedValue = null;

            model.OnChangedFolding += v => receivedValue = v;

            Assert.AreEqual(true, receivedValue);
        }

        [Test]
        public void OnChangedFolding_UnsubscribeStopsNotification()
        {
            var model = new FoldingGroupModel("Folding", false);
            var callCount = 0;
            Action<bool> handler = _ => callCount++;
            model.OnChangedFolding += handler;

            // Initial subscription fires, so callCount is 1
            Assert.AreEqual(1, callCount);

            model.OnChangedFolding -= handler;

            model.SetFoldingWithoutNotify(true);

            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void IsFolding_Setter_ChangesValue()
        {
            var model = new FoldingGroupModel("Folding", false);

            model.IsFolding = true;

            Assert.IsTrue(model.IsFolding);
        }

        [Test]
        public void IsFolding_Setter_FiresOnChangedFoldingEvent()
        {
            var model = new FoldingGroupModel("Folding", false);
            bool? receivedValue = null;
            model.OnChangedFolding += v => receivedValue = v;
            receivedValue = null;

            model.IsFolding = true;

            Assert.AreEqual(true, receivedValue);
        }

        [Test]
        public void AddChild_WorksOnFoldingGroupModel()
        {
            var model = new FoldingGroupModel("Folding", false);

            model.AddChild(new LabelModel("child"));

            Assert.AreEqual(1, model.Children.Count);
        }

        [Test]
        public void OnChangedFolding_DuplicateSubscription_OnlyFiresOnce()
        {
            var model = new FoldingGroupModel("Folding", false);
            var callCount = 0;
            Action<bool> handler = _ => callCount++;

            // Subscribe twice - should deduplicate (RemoveListener before AddListener pattern)
            model.OnChangedFolding += handler;
            callCount = 0; // reset after initial invoke
            model.OnChangedFolding += handler;
            callCount = 0; // reset after second initial invoke

            model.SetFoldingWithoutNotify(true);

            Assert.AreEqual(1, callCount);
        }
    }

    [TestFixture]
    public class DisableGroupModelTests
    {
        [Test]
        public void Constructor_SetsTitle()
        {
            var model = new DisableGroupModel("Disable");

            Assert.AreEqual("Disable", model.Title);
        }

        [Test]
        public void Constructor_IsDisabled_DefaultFalse()
        {
            var model = new DisableGroupModel("Disable");

            Assert.IsFalse(model.IsDisabled);
        }

        [Test]
        public void Constructor_SetsPriority()
        {
            var model = new DisableGroupModel("Disable", 4);

            Assert.AreEqual(4, model.Priority);
        }

        [Test]
        public void IsDisabled_Setter_ChangesValue()
        {
            var model = new DisableGroupModel("Disable");

            model.IsDisabled = true;

            Assert.IsTrue(model.IsDisabled);
        }

        [Test]
        public void IsDisabled_ToggleBackToFalse()
        {
            var model = new DisableGroupModel("Disable");

            model.IsDisabled = true;
            model.IsDisabled = false;

            Assert.IsFalse(model.IsDisabled);
        }

        [Test]
        public void AddChild_WorksOnDisableGroupModel()
        {
            var model = new DisableGroupModel("Disable");

            model.AddChild(new LabelModel("child"));

            Assert.AreEqual(1, model.Children.Count);
        }

        [Test]
        public void IsDisabled_DoesNotThrowWithoutControl()
        {
            var model = new DisableGroupModel("Disable");

            Assert.DoesNotThrow(() => model.IsDisabled = true);
        }
    }
}
