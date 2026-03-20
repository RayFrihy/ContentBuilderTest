using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Engine;
using ReactiveFlowEngine.Model;
using ReactiveFlowEngine.Tests.TestDoubles;

namespace ReactiveFlowEngine.Tests
{
    [TestFixture]
    public class StepRunnerTests
    {
        private TransitionEvaluator _evaluator;
        private StepRunner _runner;

        [SetUp]
        public void SetUp()
        {
            _evaluator = new TransitionEvaluator();
            _runner = new StepRunner(_evaluator);
        }

        [Test]
        public void RunStepAsync_NullStep_ReturnsNull()
        {
            var result = _runner.RunStepAsync(null, CancellationToken.None).GetAwaiter().GetResult();
            Assert.IsNull(result);
        }

        [Test]
        public void RunStepAsync_UnconditionalTransition_ReturnsTransition()
        {
            var targetStep = new StepModel { Id = "target", Name = "Target" };
            var transition = new TransitionModel { TargetStepModel = targetStep };

            var step = new StepModel { Id = "s1", Name = "Step1" };
            step.TransitionModels.Add(transition);

            var result = _runner.RunStepAsync(step, CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsNotNull(result);
            Assert.AreEqual("target", result.TargetStep.Id);
        }

        [Test]
        public void RunStepAsync_ExecutesBlockingBehaviorsSequentially()
        {
            var b1 = new TestBehavior(isBlocking: true);
            var b2 = new TestBehavior(isBlocking: true);

            var step = new StepModel { Id = "s1", Name = "Step1" };
            step.BehaviorList.Add(b1);
            step.BehaviorList.Add(b2);
            step.TransitionModels.Add(new TransitionModel()); // unconditional end

            _runner.RunStepAsync(step, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, b1.ExecuteCount);
            Assert.AreEqual(1, b2.ExecuteCount);
        }

        [Test]
        public void RunStepAsync_FiresNonBlockingBehaviors()
        {
            var blocking = new TestBehavior(isBlocking: true);
            var nonBlocking = new TestBehavior(isBlocking: false);

            var step = new StepModel { Id = "s1", Name = "Step1" };
            step.BehaviorList.Add(blocking);
            step.BehaviorList.Add(nonBlocking);
            step.TransitionModels.Add(new TransitionModel()); // unconditional

            _runner.RunStepAsync(step, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, blocking.ExecuteCount);
            Assert.AreEqual(1, nonBlocking.ExecuteCount);
        }

        [Test]
        public void RunStepAsync_NoTransitions_ReturnsNull()
        {
            var step = new StepModel { Id = "s1", Name = "Step1" };
            // No transitions added

            var result = _runner.RunStepAsync(step, CancellationToken.None).GetAwaiter().GetResult();
            Assert.IsNull(result);
        }

        [Test]
        public void CancelCurrentStep_CancelsActiveStep()
        {
            // Just verify it doesn't throw when no step is active
            _runner.CancelCurrentStep();
        }
    }
}
