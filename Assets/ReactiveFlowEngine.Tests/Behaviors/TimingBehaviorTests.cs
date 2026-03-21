using System.Threading;
using NUnit.Framework;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Behaviors;
using ReactiveFlowEngine.Tests.TestDoubles;

namespace ReactiveFlowEngine.Tests.Behaviors
{
    [TestFixture]
    public class DelayBehaviorTests
    {
        [Test]
        public void Constructor_SetsDefaultProperties()
        {
            var behavior = new DelayBehavior(2.5f);

            Assert.IsTrue(behavior.IsBlocking);
            Assert.AreEqual(ExecutionStages.Activation, behavior.Stages);
        }

        [Test]
        public void IsBlocking_AlwaysTrue()
        {
            var behavior = new DelayBehavior(1f);

            Assert.IsTrue(behavior.IsBlocking);
        }

        [Test]
        public void Stages_AlwaysActivation()
        {
            var behavior = new DelayBehavior(1f);

            Assert.AreEqual(ExecutionStages.Activation, behavior.Stages);
        }

        [Test]
        public void ExecuteAsync_ZeroDuration_ReturnsImmediately()
        {
            var behavior = new DelayBehavior(0f);

            // Should complete without delay
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        [Test]
        public void ExecuteAsync_NegativeDuration_ReturnsImmediately()
        {
            var behavior = new DelayBehavior(-1f);

            // Should complete without delay
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        [Test]
        public void ImplementsIBehavior()
        {
            var behavior = new DelayBehavior(1f);

            Assert.IsInstanceOf<IBehavior>(behavior);
        }
    }

    [TestFixture]
    public class TimeoutBehaviorTests
    {
        [Test]
        public void Constructor_SetsDefaultProperties()
        {
            var behavior = new TimeoutBehavior(5f);

            Assert.IsTrue(behavior.IsBlocking);
            Assert.AreEqual(ExecutionStages.Activation, behavior.Stages);
        }

        [Test]
        public void Constructor_CustomParameters_SetsProperties()
        {
            var behavior = new TimeoutBehavior(
                3f, warningMessage: "Timed out!", isBlocking: false,
                stages: ExecutionStages.Deactivation);

            Assert.IsFalse(behavior.IsBlocking);
            Assert.AreEqual(ExecutionStages.Deactivation, behavior.Stages);
        }

        [Test]
        public void Constructor_NullWarningMessage_DoesNotThrow()
        {
            var behavior = new TimeoutBehavior(1f, warningMessage: null);

            Assert.IsNotNull(behavior);
        }

        [Test]
        public void ImplementsIBehavior()
        {
            var behavior = new TimeoutBehavior(1f);

            Assert.IsInstanceOf<IBehavior>(behavior);
        }
    }

    [TestFixture]
    public class WaitUntilConditionBehaviorTests
    {
        [Test]
        public void Constructor_SetsDefaultProperties()
        {
            var condition = new TestCondition();
            var behavior = new WaitUntilConditionBehavior(condition);

            Assert.IsTrue(behavior.IsBlocking);
            Assert.AreEqual(ExecutionStages.Activation, behavior.Stages);
        }

        [Test]
        public void Constructor_CustomParameters_SetsProperties()
        {
            var condition = new TestCondition();
            var behavior = new WaitUntilConditionBehavior(
                condition, timeout: 10f, isBlocking: false,
                stages: ExecutionStages.Deactivation);

            Assert.IsFalse(behavior.IsBlocking);
            Assert.AreEqual(ExecutionStages.Deactivation, behavior.Stages);
        }

        [Test]
        public void ExecuteAsync_NullCondition_ReturnsImmediately()
        {
            var behavior = new WaitUntilConditionBehavior(null);

            // Should complete without waiting
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        [Test]
        public void ImplementsIBehavior()
        {
            var condition = new TestCondition();
            var behavior = new WaitUntilConditionBehavior(condition);

            Assert.IsInstanceOf<IBehavior>(behavior);
        }
    }
}
