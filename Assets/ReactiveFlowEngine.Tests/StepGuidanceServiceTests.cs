using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using R3;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Conditions.Interaction;
using ReactiveFlowEngine.Runtime;
using ReactiveFlowEngine.Tests.TestDoubles;

namespace ReactiveFlowEngine.Tests
{
    [TestFixture]
    public class StepGuidanceServiceTests
    {
        private MockFlowEngine _engine;
        private MockSceneObjectResolver _resolver;
        private StepGuidanceService _service;
        private MockEventBus _eventBus;

        [SetUp]
        public void SetUp()
        {
            _engine = new MockFlowEngine();
            _resolver = new MockSceneObjectResolver();
            _eventBus = new MockEventBus();
            _service = new StepGuidanceService(_engine, _resolver);
        }

        [TearDown]
        public void TearDown()
        {
            _service.Dispose();
            _engine.Dispose();
            _eventBus.Dispose();
        }

        [Test]
        public void Enable_SubscribesToCurrentStep()
        {
            _service.Enable();

            IReadOnlyList<string> received = null;
            _service.CurrentTargetObjectIds.Subscribe(ids => received = ids);

            Assert.IsNotNull(received);
            Assert.AreEqual(0, received.Count);
        }

        [Test]
        public void StepChange_ExtractsTargetFromInteractionCondition()
        {
            _service.Enable();

            var condition = new EventBusCondition(_eventBus, "ObjectGrabbed", "guid-123");
            var step = CreateStepWithConditions(condition);

            _engine.SetCurrentStep(step);

            var targets = _service.CurrentTargetObjectIds.CurrentValue;
            Assert.AreEqual(1, targets.Count);
            Assert.AreEqual("guid-123", targets[0]);
        }

        [Test]
        public void StepChange_ExtractsTargetsFromMultipleConditions()
        {
            _service.Enable();

            var cond1 = new EventBusCondition(_eventBus, "ObjectGrabbed", "guid-1");
            var cond2 = new EventBusCondition(_eventBus, "ObjectTouched", "guid-2");
            var step = CreateStepWithConditions(cond1, cond2);

            _engine.SetCurrentStep(step);

            var targets = _service.CurrentTargetObjectIds.CurrentValue;
            Assert.AreEqual(2, targets.Count);
            Assert.Contains("guid-1", (System.Collections.ICollection)targets);
            Assert.Contains("guid-2", (System.Collections.ICollection)targets);
        }

        [Test]
        public void StepChange_DeduplicatesTargets()
        {
            _service.Enable();

            var cond1 = new EventBusCondition(_eventBus, "ObjectGrabbed", "guid-same");
            var cond2 = new EventBusCondition(_eventBus, "ObjectTouched", "guid-same");
            var step = CreateStepWithConditions(cond1, cond2);

            _engine.SetCurrentStep(step);

            var targets = _service.CurrentTargetObjectIds.CurrentValue;
            Assert.AreEqual(1, targets.Count);
            Assert.AreEqual("guid-same", targets[0]);
        }

        [Test]
        public void NullStep_ClearsTargets()
        {
            _service.Enable();

            var condition = new EventBusCondition(_eventBus, "ObjectGrabbed", "guid-123");
            var step = CreateStepWithConditions(condition);
            _engine.SetCurrentStep(step);
            Assert.AreEqual(1, _service.CurrentTargetObjectIds.CurrentValue.Count);

            _engine.SetCurrentStep(null);

            Assert.AreEqual(0, _service.CurrentTargetObjectIds.CurrentValue.Count);
        }

        [Test]
        public void StepWithNoConditions_ReturnsEmptyTargets()
        {
            _service.Enable();

            var step = new SimpleStep
            {
                Id = "step-1",
                Name = "Step 1",
                Transitions = new List<ITransition>
                {
                    new SimpleTransition { IsUnconditional = true }
                }
            };

            _engine.SetCurrentStep(step);

            Assert.AreEqual(0, _service.CurrentTargetObjectIds.CurrentValue.Count);
        }

        [Test]
        public void Disable_ClearsTargets()
        {
            _service.Enable();

            var condition = new EventBusCondition(_eventBus, "ObjectGrabbed", "guid-123");
            var step = CreateStepWithConditions(condition);
            _engine.SetCurrentStep(step);
            Assert.AreEqual(1, _service.CurrentTargetObjectIds.CurrentValue.Count);

            _service.Disable();

            Assert.AreEqual(0, _service.CurrentTargetObjectIds.CurrentValue.Count);
        }

        [Test]
        public void StepChange_ResolvesTargetsViaResolver()
        {
            _service.Enable();

            var condition = new EventBusCondition(_eventBus, "ObjectGrabbed", "guid-xyz");
            var step = CreateStepWithConditions(condition);

            _engine.SetCurrentStep(step);

            Assert.Contains("guid-xyz", (System.Collections.ICollection)_resolver.ResolvedGuids);
        }

        [Test]
        public void StepWithNullTransitions_ReturnsEmptyTargets()
        {
            _service.Enable();

            var step = new SimpleStep
            {
                Id = "step-1",
                Name = "Step 1",
                Transitions = null
            };

            _engine.SetCurrentStep(step);

            Assert.AreEqual(0, _service.CurrentTargetObjectIds.CurrentValue.Count);
        }

        private static IStep CreateStepWithConditions(params ICondition[] conditions)
        {
            return new SimpleStep
            {
                Id = "step-1",
                Name = "Step 1",
                Transitions = new List<ITransition>
                {
                    new SimpleTransition
                    {
                        Conditions = new List<ICondition>(conditions)
                    }
                }
            };
        }

        private class SimpleStep : IStep
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public StepType Type { get; set; }
            public IReadOnlyList<IBehavior> Behaviors { get; set; } = new List<IBehavior>();
            public IReadOnlyList<ITransition> Transitions { get; set; } = new List<ITransition>();
            public IChapter SubChapter { get; set; }
        }

        private class SimpleTransition : ITransition
        {
            public int Priority { get; set; }
            public IReadOnlyList<ICondition> Conditions { get; set; } = new List<ICondition>();
            public IStep TargetStep { get; set; }
            public bool IsUnconditional { get; set; }
        }
    }
}
