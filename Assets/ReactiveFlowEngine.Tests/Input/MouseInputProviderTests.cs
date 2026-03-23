using System.Collections.Generic;
using NUnit.Framework;
using R3;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Tests.TestDoubles;

namespace ReactiveFlowEngine.Tests.Input
{
    [TestFixture]
    public class MouseInputProviderTests
    {
        private MockEventBus _eventBus;

        [SetUp]
        public void SetUp()
        {
            _eventBus = new MockEventBus();
        }

        [TearDown]
        public void TearDown()
        {
            _eventBus.Dispose();
        }

        [Test]
        public void EventBus_ObjectGrabbed_EventIsPublished()
        {
            object received = null;
            _eventBus.On("ObjectGrabbed").Subscribe(p => received = p);

            _eventBus.Publish("ObjectGrabbed", "test-guid");

            Assert.AreEqual("test-guid", received);
        }

        [Test]
        public void EventBus_ObjectReleased_EventIsPublished()
        {
            object received = null;
            _eventBus.On("ObjectReleased").Subscribe(p => received = p);

            _eventBus.Publish("ObjectReleased", "test-guid");

            Assert.AreEqual("test-guid", received);
        }

        [Test]
        public void EventBus_HoverEnterExit_EventsArePublished()
        {
            var enterEvents = new List<object>();
            var exitEvents = new List<object>();
            _eventBus.On("ObjectHoverEnter").Subscribe(p => enterEvents.Add(p));
            _eventBus.On("ObjectHoverExit").Subscribe(p => exitEvents.Add(p));

            _eventBus.Publish("ObjectHoverEnter", "guid-1");
            _eventBus.Publish("ObjectHoverExit", "guid-1");

            Assert.AreEqual(1, enterEvents.Count);
            Assert.AreEqual(1, exitEvents.Count);
            Assert.AreEqual("guid-1", enterEvents[0]);
            Assert.AreEqual("guid-1", exitEvents[0]);
        }

        [Test]
        public void EventBus_ObjectTouched_EventIsPublished()
        {
            object received = null;
            _eventBus.On("ObjectTouched").Subscribe(p => received = p);

            _eventBus.Publish("ObjectTouched", "test-guid");

            Assert.AreEqual("test-guid", received);
        }

        [Test]
        public void EventBus_ObjectSelected_EventIsPublished()
        {
            object received = null;
            _eventBus.On("ObjectSelected").Subscribe(p => received = p);

            _eventBus.Publish("ObjectSelected", "test-guid");

            Assert.AreEqual("test-guid", received);
        }

        [Test]
        public void IInputProvider_Interface_CanBeImplemented()
        {
            var provider = new MockInputProvider();
            Assert.IsTrue(provider.IsActive);

            provider.Disable();
            Assert.IsFalse(provider.IsActive);

            provider.Enable();
            Assert.IsTrue(provider.IsActive);
        }
    }
}
