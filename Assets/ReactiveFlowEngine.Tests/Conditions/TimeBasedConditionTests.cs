using System;
using NUnit.Framework;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Conditions;
using ReactiveFlowEngine.Conditions.TimeBased;
using ReactiveFlowEngine.Tests.TestDoubles;

namespace ReactiveFlowEngine.Tests.Conditions
{
    [TestFixture]
    public class DelayElapsedConditionTests
    {
        [Test]
        public void Constructor_ValidDelay_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => new DelayElapsedCondition(2.5f));
        }

        [Test]
        public void Duration_ReturnsConstructorDelay()
        {
            var condition = new DelayElapsedCondition(3.0f);
            Assert.AreEqual(3.0f, condition.Duration);
        }

        [Test]
        public void Evaluate_ReturnsNonNullObservable()
        {
            var condition = new DelayElapsedCondition(1.0f);
            Assert.IsNotNull(condition.Evaluate());
        }

        [Test]
        public void Reset_DoesNotThrow()
        {
            var condition = new DelayElapsedCondition(1.0f);
            Assert.DoesNotThrow(() => condition.Reset());
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            var condition = new DelayElapsedCondition(1.0f);
            Assert.DoesNotThrow(() => condition.Dispose());
        }
    }

    [TestFixture]
    public class TimerRunningConditionTests
    {
        [Test]
        public void Constructor_NullEventBus_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new TimerRunningCondition(null, "timer1"));
        }

        [Test]
        public void Constructor_NullTimerId_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new TimerRunningCondition(new MockEventBus(), null));
        }

        [Test]
        public void Constructor_ValidArgs_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
                new TimerRunningCondition(new MockEventBus(), "timer1"));
        }

        [Test]
        public void Duration_ReturnsZero()
        {
            var condition = new TimerRunningCondition(new MockEventBus(), "timer1");
            Assert.AreEqual(0f, condition.Duration);
        }

        [Test]
        public void Evaluate_ReturnsNonNullObservable()
        {
            var condition = new TimerRunningCondition(new MockEventBus(), "timer1");
            Assert.IsNotNull(condition.Evaluate());
        }

        [Test]
        public void Reset_DoesNotThrow()
        {
            var condition = new TimerRunningCondition(new MockEventBus(), "timer1");
            Assert.DoesNotThrow(() => condition.Reset());
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            var condition = new TimerRunningCondition(new MockEventBus(), "timer1");
            Assert.DoesNotThrow(() => condition.Dispose());
        }
    }

    [TestFixture]
    public class CooldownCompleteConditionTests
    {
        [Test]
        public void Constructor_NullEventBus_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new CooldownCompleteCondition(null, "cd1", 5.0f));
        }

        [Test]
        public void Constructor_NullCooldownId_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new CooldownCompleteCondition(new MockEventBus(), null, 5.0f));
        }

        [Test]
        public void Constructor_ValidArgs_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
                new CooldownCompleteCondition(new MockEventBus(), "cd1", 5.0f));
        }

        [Test]
        public void Duration_ReturnsCooldownDuration()
        {
            var condition = new CooldownCompleteCondition(new MockEventBus(), "cd1", 7.5f);
            Assert.AreEqual(7.5f, condition.Duration);
        }

        [Test]
        public void Evaluate_ReturnsNonNullObservable()
        {
            var condition = new CooldownCompleteCondition(new MockEventBus(), "cd1", 5.0f);
            Assert.IsNotNull(condition.Evaluate());
        }

        [Test]
        public void Reset_DoesNotThrow()
        {
            var condition = new CooldownCompleteCondition(new MockEventBus(), "cd1", 5.0f);
            Assert.DoesNotThrow(() => condition.Reset());
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            var condition = new CooldownCompleteCondition(new MockEventBus(), "cd1", 5.0f);
            Assert.DoesNotThrow(() => condition.Dispose());
        }
    }

    [TestFixture]
    public class ElapsedTimeConditionTests
    {
        [Test]
        public void Constructor_ValidArgs_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
                new ElapsedTimeCondition(10.0f, ComparisonOperator.GreaterThan));
        }

        [Test]
        public void Duration_ReturnsRequiredElapsed()
        {
            var condition = new ElapsedTimeCondition(15.0f, ComparisonOperator.LessThan);
            Assert.AreEqual(15.0f, condition.Duration);
        }

        [Test]
        public void Reset_DoesNotThrow()
        {
            var condition = new ElapsedTimeCondition(5.0f, ComparisonOperator.Equal);
            Assert.DoesNotThrow(() => condition.Reset());
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            var condition = new ElapsedTimeCondition(5.0f, ComparisonOperator.Equal);
            Assert.DoesNotThrow(() => condition.Dispose());
        }
    }

    [TestFixture]
    public class TimeoutConditionTests
    {
        [Test]
        public void Constructor_ValidTimeout_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => new TimeoutCondition(5.0f));
        }

        [Test]
        public void Evaluate_ReturnsNonNullObservable()
        {
            var condition = new TimeoutCondition(3.0f);
            Assert.IsNotNull(condition.Evaluate());
        }

        [Test]
        public void Reset_DoesNotThrow()
        {
            var condition = new TimeoutCondition(3.0f);
            Assert.DoesNotThrow(() => condition.Reset());
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            var condition = new TimeoutCondition(3.0f);
            Assert.DoesNotThrow(() => condition.Dispose());
        }
    }
}
