using System;
using NUnit.Framework;
using Xeon.XDebugger.Model;

namespace Xeon.XDebugger.Tests
{
    [TestFixture]
    public class ActionModelTests
    {
        [Test]
        public void Constructor_SetsTitle()
        {
            var model = new ActionModel("Execute", () => { });
            Assert.AreEqual("Execute", model.Title);
        }

        [Test]
        public void Constructor_SetsPriority()
        {
            var model = new ActionModel("Execute", () => { }, 3);
            Assert.AreEqual(3, model.Priority);
        }

        [Test]
        public void Constructor_DefaultPriorityIsZero()
        {
            var model = new ActionModel("Execute", () => { });
            Assert.AreEqual(0, model.Priority);
        }

        [Test]
        public void ExecuteMethod_FiresAction()
        {
            var actionFired = false;
            var model = new ActionModel("Execute", () => actionFired = true);

            model.ExecuteMethod();

            Assert.IsTrue(actionFired);
        }

        [Test]
        public void ExecuteMethod_MultipleCalls_FiresActionEachTime()
        {
            var callCount = 0;
            var model = new ActionModel("Execute", () => callCount++);

            model.ExecuteMethod();
            model.ExecuteMethod();
            model.ExecuteMethod();

            Assert.AreEqual(3, callCount);
        }

        [Test]
        public void ExecuteMethod_NullAction_DoesNotThrow()
        {
            var model = new ActionModel("Execute", null);

            Assert.DoesNotThrow(() => model.ExecuteMethod());
        }
    }
}
