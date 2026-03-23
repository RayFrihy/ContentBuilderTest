using System.Threading;
using NUnit.Framework;
using R3;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Behaviors;
using ReactiveFlowEngine.Tests.TestDoubles;

namespace ReactiveFlowEngine.Tests.Behaviors
{
    [TestFixture]
    public class TriggerEventBehaviorTests
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
        public void ExecuteAsync_PublishesEventWithCorrectName()
        {
            string receivedName = null;
            _eventBus.On("TestEvent").Subscribe(p => receivedName = "TestEvent");

            var behavior = new TriggerEventBehavior(_eventBus, "TestEvent");
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual("TestEvent", receivedName);
        }

        [Test]
        public void ExecuteAsync_PublishesEventWithCorrectPayload()
        {
            object receivedPayload = null;
            _eventBus.On("MyEvent").Subscribe(p => receivedPayload = p);

            var payload = new { Value = 42 };
            var behavior = new TriggerEventBehavior(_eventBus, "MyEvent", payload);
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreSame(payload, receivedPayload);
        }

        [Test]
        public void ExecuteAsync_PublishesEventWithNullPayload()
        {
            object receivedPayload = "not-null";
            _eventBus.On("NullPayloadEvent").Subscribe(p => receivedPayload = p);

            var behavior = new TriggerEventBehavior(_eventBus, "NullPayloadEvent");
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsNull(receivedPayload);
        }

        [Test]
        public void IsBlocking_DefaultsFalse()
        {
            var behavior = new TriggerEventBehavior(_eventBus, "evt");
            Assert.IsFalse(behavior.IsBlocking);
        }

        [Test]
        public void IsBlocking_RespectsConstructorArgument()
        {
            var behavior = new TriggerEventBehavior(_eventBus, "evt", isBlocking: true);
            Assert.IsTrue(behavior.IsBlocking);
        }

        [Test]
        public void Stages_DefaultsToActivation()
        {
            var behavior = new TriggerEventBehavior(_eventBus, "evt");
            Assert.AreEqual(ExecutionStages.Activation, behavior.Stages);
        }

        [Test]
        public void Stages_RespectsConstructorArgument()
        {
            var behavior = new TriggerEventBehavior(_eventBus, "evt", stages: ExecutionStages.Deactivation);
            Assert.AreEqual(ExecutionStages.Deactivation, behavior.Stages);
        }

        [Test]
        public void ImplementsIBehavior()
        {
            var behavior = new TriggerEventBehavior(_eventBus, "evt");
            Assert.IsInstanceOf<IBehavior>(behavior);
        }
    }

    [TestFixture]
    public class SendMessageBehaviorTests
    {
        private MockSceneObjectResolver _resolver;

        [SetUp]
        public void SetUp()
        {
            _resolver = new MockSceneObjectResolver();
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void ExecuteAsync_NullResolver_ReturnsSilently()
        {
            // Resolver returns null for unregistered guid
            var behavior = new SendMessageBehavior(_resolver, "unknown-guid", "DoSomething");
            Assert.DoesNotThrow(() =>
                behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult());
        }

        [Test]
        public void ExecuteAsync_ResolvesTargetWithCorrectGuid()
        {
            var behavior = new SendMessageBehavior(_resolver, "my-guid", "OnActivate");
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, _resolver.ResolvedGuids.Count);
            Assert.AreEqual("my-guid", _resolver.ResolvedGuids[0]);
        }

        [Test]
        public void IsBlocking_DefaultsTrue()
        {
            var behavior = new SendMessageBehavior(_resolver, "guid", "Method");
            Assert.IsTrue(behavior.IsBlocking);
        }

        [Test]
        public void IsBlocking_RespectsConstructorArgument()
        {
            var behavior = new SendMessageBehavior(_resolver, "guid", "Method", isBlocking: false);
            Assert.IsFalse(behavior.IsBlocking);
        }

        [Test]
        public void Stages_DefaultsToActivation()
        {
            var behavior = new SendMessageBehavior(_resolver, "guid", "Method");
            Assert.AreEqual(ExecutionStages.Activation, behavior.Stages);
        }

        [Test]
        public void Stages_RespectsConstructorArgument()
        {
            var behavior = new SendMessageBehavior(_resolver, "guid", "Method", stages: ExecutionStages.Deactivation);
            Assert.AreEqual(ExecutionStages.Deactivation, behavior.Stages);
        }

        [Test]
        public void ImplementsIBehavior()
        {
            var behavior = new SendMessageBehavior(_resolver, "guid", "Method");
            Assert.IsInstanceOf<IBehavior>(behavior);
        }
    }
}
