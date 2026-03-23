using NUnit.Framework;
using R3;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Tests.TestDoubles;

namespace ReactiveFlowEngine.Tests
{
    [TestFixture]
    public class RfeInteractableTests
    {
        [Test]
        public void InteractionFlags_All_IncludesAllFlags()
        {
            var all = InteractionFlags.All;
            Assert.IsTrue((all & InteractionFlags.Hoverable) != 0);
            Assert.IsTrue((all & InteractionFlags.Touchable) != 0);
            Assert.IsTrue((all & InteractionFlags.Grabbable) != 0);
            Assert.IsTrue((all & InteractionFlags.Usable) != 0);
            Assert.IsTrue((all & InteractionFlags.Selectable) != 0);
        }

        [Test]
        public void InteractionFlags_None_ExcludesAllFlags()
        {
            var none = InteractionFlags.None;
            Assert.IsFalse((none & InteractionFlags.Hoverable) != 0);
            Assert.IsFalse((none & InteractionFlags.Grabbable) != 0);
        }

        [Test]
        public void InteractionFlags_CanCombine()
        {
            var combined = InteractionFlags.Hoverable | InteractionFlags.Grabbable;
            Assert.IsTrue((combined & InteractionFlags.Hoverable) != 0);
            Assert.IsTrue((combined & InteractionFlags.Grabbable) != 0);
            Assert.IsFalse((combined & InteractionFlags.Touchable) != 0);
        }

        [Test]
        public void InteractionState_DefaultIsIdle()
        {
            var state = default(InteractionState);
            Assert.AreEqual(InteractionState.Idle, state);
        }

        [Test]
        public void MockInputProvider_EnableDisable_TracksState()
        {
            var provider = new MockInputProvider();
            Assert.IsTrue(provider.IsActive);

            provider.Disable();
            Assert.IsFalse(provider.IsActive);
            Assert.AreEqual(1, provider.DisableCount);

            provider.Enable();
            Assert.IsTrue(provider.IsActive);
            Assert.AreEqual(1, provider.EnableCount);
        }
    }
}
