using System;
using System.Collections.Generic;
using NUnit.Framework;
using R3;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Conditions.StepFlow;
using ReactiveFlowEngine.Navigation;
using ReactiveFlowEngine.Tests.TestDoubles;

namespace ReactiveFlowEngine.Tests.Conditions
{
    [TestFixture]
    public class StepCompletedConditionTests
    {
        [Test]
        public void Constructor_NullHistoryService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new StepCompletedCondition(null, "step1"));
        }

        [Test]
        public void Constructor_NullStepId_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new StepCompletedCondition(new MockHistoryService(), null));
        }

        [Test]
        public void Constructor_ValidArgs_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
                new StepCompletedCondition(new MockHistoryService(), "step1"));
        }

        [Test]
        public void Evaluate_ReturnsNonNullObservable()
        {
            var condition = new StepCompletedCondition(new MockHistoryService(), "step1");
            Assert.IsNotNull(condition.Evaluate());
        }

        [Test]
        public void Reset_DoesNotThrow()
        {
            var condition = new StepCompletedCondition(new MockHistoryService(), "step1");
            Assert.DoesNotThrow(() => condition.Reset());
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            var condition = new StepCompletedCondition(new MockHistoryService(), "step1");
            Assert.DoesNotThrow(() => condition.Dispose());
        }
    }

    [TestFixture]
    public class StepActiveConditionTests
    {
        [Test]
        public void Constructor_NullFlowEngine_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new StepActiveCondition(null, "step1"));
        }

        [Test]
        public void Constructor_NullStepId_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new StepActiveCondition(new MockFlowEngine(), null));
        }

        [Test]
        public void Constructor_ValidArgs_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
                new StepActiveCondition(new MockFlowEngine(), "step1"));
        }

        [Test]
        public void Evaluate_EmitsTrue_WhenCurrentStepMatches()
        {
            var engine = new MockFlowEngine();
            var condition = new StepActiveCondition(engine, "step1");
            bool? result = null;
            var sub = condition.Evaluate().Subscribe(v => result = v);

            Assert.AreEqual(false, result); // null step initially

            engine.SetCurrentStep(new MockStep { Id = "step1" });
            Assert.AreEqual(true, result);

            sub.Dispose();
            engine.Dispose();
        }

        [Test]
        public void Evaluate_EmitsFalse_WhenCurrentStepDoesNotMatch()
        {
            var engine = new MockFlowEngine();
            var condition = new StepActiveCondition(engine, "step1");
            bool? result = null;
            var sub = condition.Evaluate().Subscribe(v => result = v);

            engine.SetCurrentStep(new MockStep { Id = "step2" });
            Assert.AreEqual(false, result);

            sub.Dispose();
            engine.Dispose();
        }

        [Test]
        public void Reset_DoesNotThrow()
        {
            var condition = new StepActiveCondition(new MockFlowEngine(), "step1");
            Assert.DoesNotThrow(() => condition.Reset());
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            var condition = new StepActiveCondition(new MockFlowEngine(), "step1");
            Assert.DoesNotThrow(() => condition.Dispose());
        }
    }

    [TestFixture]
    public class PreviousStepConditionTests
    {
        [Test]
        public void Constructor_NullHistoryService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new PreviousStepCondition(null, "step1"));
        }

        [Test]
        public void Constructor_NullExpectedPreviousStepId_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new PreviousStepCondition(new MockHistoryService(), null));
        }

        [Test]
        public void Constructor_ValidArgs_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
                new PreviousStepCondition(new MockHistoryService(), "step1"));
        }

        [Test]
        public void Evaluate_ReturnsNonNullObservable()
        {
            var condition = new PreviousStepCondition(new MockHistoryService(), "step1");
            Assert.IsNotNull(condition.Evaluate());
        }

        [Test]
        public void Reset_DoesNotThrow()
        {
            var condition = new PreviousStepCondition(new MockHistoryService(), "step1");
            Assert.DoesNotThrow(() => condition.Reset());
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            var condition = new PreviousStepCondition(new MockHistoryService(), "step1");
            Assert.DoesNotThrow(() => condition.Dispose());
        }
    }

    [TestFixture]
    public class NextStepAvailableConditionTests
    {
        [Test]
        public void Constructor_NullFlowEngine_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new NextStepAvailableCondition(null));
        }

        [Test]
        public void Constructor_ValidArgs_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
                new NextStepAvailableCondition(new MockFlowEngine()));
        }

        [Test]
        public void Evaluate_EmitsFalse_WhenNoCurrentStep()
        {
            var engine = new MockFlowEngine();
            var condition = new NextStepAvailableCondition(engine);
            bool? result = null;
            var sub = condition.Evaluate().Subscribe(v => result = v);

            Assert.AreEqual(false, result);

            sub.Dispose();
            engine.Dispose();
        }

        [Test]
        public void Evaluate_EmitsTrue_WhenCurrentStepHasTransitions()
        {
            var engine = new MockFlowEngine();
            var condition = new NextStepAvailableCondition(engine);
            bool? result = null;
            var sub = condition.Evaluate().Subscribe(v => result = v);

            var step = new MockStep
            {
                Id = "step1",
                Transitions = new List<ITransition>
                {
                    new MockTransition { TargetStep = new MockStep { Id = "step2" } }
                }
            };
            engine.SetCurrentStep(step);
            Assert.AreEqual(true, result);

            sub.Dispose();
            engine.Dispose();
        }

        [Test]
        public void Reset_DoesNotThrow()
        {
            var condition = new NextStepAvailableCondition(new MockFlowEngine());
            Assert.DoesNotThrow(() => condition.Reset());
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            var condition = new NextStepAvailableCondition(new MockFlowEngine());
            Assert.DoesNotThrow(() => condition.Dispose());
        }
    }

    [TestFixture]
    public class TransitionAvailableConditionTests
    {
        [Test]
        public void Constructor_NullFlowEngine_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new TransitionAvailableCondition(null, "step2"));
        }

        [Test]
        public void Constructor_NullTargetStepId_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new TransitionAvailableCondition(new MockFlowEngine(), null));
        }

        [Test]
        public void Constructor_ValidArgs_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
                new TransitionAvailableCondition(new MockFlowEngine(), "step2"));
        }

        [Test]
        public void Evaluate_EmitsTrue_WhenTransitionToTargetExists()
        {
            var engine = new MockFlowEngine();
            var condition = new TransitionAvailableCondition(engine, "step2");
            bool? result = null;
            var sub = condition.Evaluate().Subscribe(v => result = v);

            var step = new MockStep
            {
                Id = "step1",
                Transitions = new List<ITransition>
                {
                    new MockTransition { TargetStep = new MockStep { Id = "step2" } }
                }
            };
            engine.SetCurrentStep(step);
            Assert.AreEqual(true, result);

            sub.Dispose();
            engine.Dispose();
        }

        [Test]
        public void Evaluate_EmitsFalse_WhenNoMatchingTransition()
        {
            var engine = new MockFlowEngine();
            var condition = new TransitionAvailableCondition(engine, "step3");
            bool? result = null;
            var sub = condition.Evaluate().Subscribe(v => result = v);

            var step = new MockStep
            {
                Id = "step1",
                Transitions = new List<ITransition>
                {
                    new MockTransition { TargetStep = new MockStep { Id = "step2" } }
                }
            };
            engine.SetCurrentStep(step);
            Assert.AreEqual(false, result);

            sub.Dispose();
            engine.Dispose();
        }

        [Test]
        public void Reset_DoesNotThrow()
        {
            var condition = new TransitionAvailableCondition(new MockFlowEngine(), "step2");
            Assert.DoesNotThrow(() => condition.Reset());
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            var condition = new TransitionAvailableCondition(new MockFlowEngine(), "step2");
            Assert.DoesNotThrow(() => condition.Dispose());
        }
    }

    [TestFixture]
    public class ProcessStartedConditionTests
    {
        [Test]
        public void Constructor_NullFlowEngine_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ProcessStartedCondition(null));
        }

        [Test]
        public void Constructor_ValidArgs_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
                new ProcessStartedCondition(new MockFlowEngine()));
        }

        [Test]
        public void Evaluate_EmitsFalse_WhenIdle()
        {
            var engine = new MockFlowEngine();
            var condition = new ProcessStartedCondition(engine);
            bool? result = null;
            var sub = condition.Evaluate().Subscribe(v => result = v);

            Assert.AreEqual(false, result);

            sub.Dispose();
            engine.Dispose();
        }

        [Test]
        public void Evaluate_EmitsTrue_WhenRunning()
        {
            var engine = new MockFlowEngine();
            var condition = new ProcessStartedCondition(engine);
            bool? result = null;
            var sub = condition.Evaluate().Subscribe(v => result = v);

            Assert.AreEqual(false, result);
            engine.SetState(EngineState.Running);
            Assert.AreEqual(true, result);

            sub.Dispose();
            engine.Dispose();
        }

        [Test]
        public void Reset_DoesNotThrow()
        {
            var condition = new ProcessStartedCondition(new MockFlowEngine());
            Assert.DoesNotThrow(() => condition.Reset());
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            var condition = new ProcessStartedCondition(new MockFlowEngine());
            Assert.DoesNotThrow(() => condition.Dispose());
        }
    }

    [TestFixture]
    public class ProcessCompletedConditionTests
    {
        [Test]
        public void Constructor_NullFlowEngine_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ProcessCompletedCondition(null));
        }

        [Test]
        public void Constructor_ValidArgs_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
                new ProcessCompletedCondition(new MockFlowEngine()));
        }

        [Test]
        public void Evaluate_EmitsFalse_WhenIdle()
        {
            var engine = new MockFlowEngine();
            var condition = new ProcessCompletedCondition(engine);
            bool? result = null;
            var sub = condition.Evaluate().Subscribe(v => result = v);

            Assert.AreEqual(false, result);

            sub.Dispose();
            engine.Dispose();
        }

        [Test]
        public void Evaluate_EmitsTrue_WhenCompleted()
        {
            var engine = new MockFlowEngine();
            var condition = new ProcessCompletedCondition(engine);
            bool? result = null;
            var sub = condition.Evaluate().Subscribe(v => result = v);

            Assert.AreEqual(false, result);
            engine.SetState(EngineState.Completed);
            Assert.AreEqual(true, result);

            sub.Dispose();
            engine.Dispose();
        }

        [Test]
        public void Evaluate_EmitsFalse_WhenRunning()
        {
            var engine = new MockFlowEngine();
            var condition = new ProcessCompletedCondition(engine);
            bool? result = null;
            var sub = condition.Evaluate().Subscribe(v => result = v);

            engine.SetState(EngineState.Running);
            Assert.AreEqual(false, result);

            sub.Dispose();
            engine.Dispose();
        }

        [Test]
        public void Reset_DoesNotThrow()
        {
            var condition = new ProcessCompletedCondition(new MockFlowEngine());
            Assert.DoesNotThrow(() => condition.Reset());
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            var condition = new ProcessCompletedCondition(new MockFlowEngine());
            Assert.DoesNotThrow(() => condition.Dispose());
        }
    }

    #region Test Doubles

    internal class MockStep : IStep
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public StepType Type { get; set; }
        public IReadOnlyList<IBehavior> Behaviors { get; set; } = new List<IBehavior>();
        public IReadOnlyList<ITransition> Transitions { get; set; } = new List<ITransition>();
        public IChapter SubChapter { get; set; }
    }

    internal class MockTransition : ITransition
    {
        public int Priority { get; set; }
        public IReadOnlyList<ICondition> Conditions { get; set; } = new List<ICondition>();
        public IStep TargetStep { get; set; }
        public bool IsUnconditional { get; set; }
    }

    #endregion
}
