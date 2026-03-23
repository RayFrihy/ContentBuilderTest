using NUnit.Framework;
using R3;
using ReactiveFlowEngine.Conditions.Environment;
using ReactiveFlowEngine.Tests.TestDoubles;

namespace ReactiveFlowEngine.Tests
{
    [TestFixture]
    public class RfeTriggerReporterTests
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
        public void TriggerEventData_StoresObjectIds()
        {
            var data = new TriggerEventData("trigger-1", "other-1");

            Assert.AreEqual("trigger-1", data.TriggerObjectId);
            Assert.AreEqual("other-1", data.OtherObjectId);
        }

        [Test]
        public void CollisionEventData_StoresObjectIds()
        {
            var data = new CollisionEventData("obj-a", "obj-b");

            Assert.AreEqual("obj-a", data.ObjectAId);
            Assert.AreEqual("obj-b", data.ObjectBId);
        }

        [Test]
        public void EventBus_TriggerEnter_PublishesCorrectData()
        {
            TriggerEventData received = null;
            _eventBus.On("TriggerEnter").Subscribe(p =>
            {
                if (p is TriggerEventData td)
                    received = td;
            });

            var data = new TriggerEventData("zone-1", "player-1");
            _eventBus.Publish("TriggerEnter", data);

            Assert.IsNotNull(received);
            Assert.AreEqual("zone-1", received.TriggerObjectId);
            Assert.AreEqual("player-1", received.OtherObjectId);
        }

        [Test]
        public void EventBus_TriggerExit_PublishesCorrectData()
        {
            TriggerEventData received = null;
            _eventBus.On("TriggerExit").Subscribe(p =>
            {
                if (p is TriggerEventData td)
                    received = td;
            });

            var data = new TriggerEventData("zone-1", "player-1");
            _eventBus.Publish("TriggerExit", data);

            Assert.IsNotNull(received);
            Assert.AreEqual("zone-1", received.TriggerObjectId);
            Assert.AreEqual("player-1", received.OtherObjectId);
        }

        [Test]
        public void EventBus_CollisionEnter_PublishesCorrectData()
        {
            CollisionEventData received = null;
            _eventBus.On("CollisionEnter").Subscribe(p =>
            {
                if (p is CollisionEventData cd)
                    received = cd;
            });

            var data = new CollisionEventData("obj-a", "obj-b");
            _eventBus.Publish("CollisionEnter", data);

            Assert.IsNotNull(received);
            Assert.AreEqual("obj-a", received.ObjectAId);
            Assert.AreEqual("obj-b", received.ObjectBId);
        }

        [Test]
        public void EventBus_CollisionExit_PublishesCorrectData()
        {
            CollisionEventData received = null;
            _eventBus.On("CollisionExit").Subscribe(p =>
            {
                if (p is CollisionEventData cd)
                    received = cd;
            });

            var data = new CollisionEventData("obj-a", "obj-b");
            _eventBus.Publish("CollisionExit", data);

            Assert.IsNotNull(received);
            Assert.AreEqual("obj-a", received.ObjectAId);
            Assert.AreEqual("obj-b", received.ObjectBId);
        }
    }
}
