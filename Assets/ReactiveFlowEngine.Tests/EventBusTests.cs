using NUnit.Framework;
using ReactiveFlowEngine.Tests.TestDoubles;

namespace ReactiveFlowEngine.Tests
{
    [TestFixture]
    public class EventBusTests
    {
        private MockEventBus _bus;

        [SetUp]
        public void SetUp()
        {
            _bus = new MockEventBus();
        }

        [TearDown]
        public void TearDown()
        {
            _bus.Dispose();
        }

        [Test]
        public void Publish_And_On_DeliversPayload()
        {
            object received = null;
            _bus.On("TestEvent").Subscribe(payload => received = payload);

            _bus.Publish("TestEvent", "hello");

            Assert.AreEqual("hello", received);
        }

        [Test]
        public void On_DifferentEvents_AreIsolated()
        {
            object received1 = null;
            object received2 = null;
            _bus.On("Event1").Subscribe(p => received1 = p);
            _bus.On("Event2").Subscribe(p => received2 = p);

            _bus.Publish("Event1", "data1");

            Assert.AreEqual("data1", received1);
            Assert.IsNull(received2);
        }

        [Test]
        public void On_SameEvent_MultipleSubscribers_AllReceive()
        {
            int count = 0;
            _bus.On("TestEvent").Subscribe(_ => count++);
            _bus.On("TestEvent").Subscribe(_ => count++);

            _bus.Publish("TestEvent", null);

            Assert.AreEqual(2, count);
        }

        [Test]
        public void On_BeforePublish_StillReceivesLaterEvents()
        {
            object received = null;
            _bus.On("TestEvent").Subscribe(p => received = p);

            // No publish yet
            Assert.IsNull(received);

            _bus.Publish("TestEvent", 42);
            Assert.AreEqual(42, received);
        }
    }
}
