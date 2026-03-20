using System.Collections.Generic;
using NUnit.Framework;
using R3;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Engine;
using ReactiveFlowEngine.Model;
using ReactiveFlowEngine.Tests.TestDoubles;

namespace ReactiveFlowEngine.Tests
{
    [TestFixture]
    public class TransitionEvaluatorTests
    {
        private TransitionEvaluator _evaluator;

        [SetUp]
        public void SetUp()
        {
            _evaluator = new TransitionEvaluator();
        }

        [Test]
        public void Evaluate_UnconditionalTransition_FiresImmediately()
        {
            var targetStep = new StepModel { Id = "target", Name = "Target" };
            var transition = new TransitionModel { TargetStepModel = targetStep };

            ITransition winner = null;
            var observable = _evaluator.Evaluate(new List<ITransition> { transition });
            observable.Subscribe(t => winner = t);

            Assert.IsNotNull(winner);
            Assert.AreEqual("target", winner.TargetStep.Id);
        }

        [Test]
        public void Evaluate_AllConditionsTrue_FiresTransition()
        {
            var cond1 = new TestCondition();
            var cond2 = new TestCondition();
            var targetStep = new StepModel { Id = "target", Name = "Target" };
            var transition = new TransitionModel { TargetStepModel = targetStep };
            transition.ConditionList.Add(cond1);
            transition.ConditionList.Add(cond2);

            ITransition winner = null;
            var observable = _evaluator.Evaluate(new List<ITransition> { transition });
            observable.Subscribe(t => winner = t);

            Assert.IsNull(winner); // Not fired yet

            cond1.EmitResult(true);
            Assert.IsNull(winner); // Still waiting for cond2

            cond2.EmitResult(true);
            Assert.IsNotNull(winner);
            Assert.AreEqual("target", winner.TargetStep.Id);
        }

        [Test]
        public void Evaluate_SomeConditionsFalse_DoesNotFire()
        {
            var cond1 = new TestCondition();
            var cond2 = new TestCondition();
            var transition = new TransitionModel();
            transition.ConditionList.Add(cond1);
            transition.ConditionList.Add(cond2);

            ITransition winner = null;
            var observable = _evaluator.Evaluate(new List<ITransition> { transition });
            observable.Subscribe(t => winner = t);

            cond1.EmitResult(true);
            cond2.EmitResult(false);

            Assert.IsNull(winner);
        }

        [Test]
        public void Evaluate_MultipleTransitions_ReturnsFirstToFire()
        {
            var cond1 = new TestCondition();
            var cond2 = new TestCondition();

            var step1 = new StepModel { Id = "target1", Name = "Target1" };
            var step2 = new StepModel { Id = "target2", Name = "Target2" };

            var t1 = new TransitionModel { TargetStepModel = step1 };
            t1.ConditionList.Add(cond1);

            var t2 = new TransitionModel { TargetStepModel = step2 };
            t2.ConditionList.Add(cond2);

            ITransition winner = null;
            var observable = _evaluator.Evaluate(new List<ITransition> { t1, t2 });
            observable.Subscribe(t => winner = t);

            cond2.EmitResult(true); // t2 fires first
            Assert.IsNotNull(winner);
            Assert.AreEqual("target2", winner.TargetStep.Id);
        }

        [Test]
        public void Evaluate_EmptyTransitions_ReturnsEmpty()
        {
            ITransition winner = null;
            bool completed = false;
            var observable = _evaluator.Evaluate(new List<ITransition>());
            observable.Subscribe(t => winner = t, _ => { }, () => completed = true);

            Assert.IsNull(winner);
            Assert.IsTrue(completed);
        }

        [Test]
        public void Evaluate_NullTransitions_ReturnsEmpty()
        {
            bool completed = false;
            var observable = _evaluator.Evaluate(null);
            observable.Subscribe(_ => { }, _ => { }, () => completed = true);

            Assert.IsTrue(completed);
        }

        [Test]
        public void Evaluate_NullConditions_TreatsAsUnconditional()
        {
            // A transition with an empty condition list is unconditional
            var transition = new TransitionModel { TargetStepModel = new StepModel { Id = "t" } };
            // ConditionList is empty by default

            ITransition winner = null;
            _evaluator.Evaluate(new List<ITransition> { transition })
                .Subscribe(t => winner = t);

            Assert.IsNotNull(winner);
        }
    }
}
