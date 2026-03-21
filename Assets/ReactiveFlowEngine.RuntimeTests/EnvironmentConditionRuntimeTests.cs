using System;
using NUnit.Framework;
using R3;
using UnityEngine;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Conditions.Environment;
using ReactiveFlowEngine.Tests.TestDoubles;

namespace ReactiveFlowEngine.RuntimeTests
{
    [TestFixture]
    public class EnvironmentConditionRuntimeTests
    {
        private RuntimeTestHelper _helper;
        private MockSceneObjectResolver _resolver;
        private MockEventBus _eventBus;

        [SetUp]
        public void SetUp()
        {
            _helper = new RuntimeTestHelper();
            _resolver = new MockSceneObjectResolver();
            _eventBus = new MockEventBus();
        }

        [TearDown]
        public void TearDown()
        {
            _helper.TearDown();
            _eventBus.Dispose();
        }

        // === SceneLoadedCondition ===

        [Test]
        public void SceneLoadedCondition_Construction_Succeeds()
        {
            var condition = new SceneLoadedCondition(_eventBus, "TestScene");
            Assert.IsNotNull(condition);
        }

        [Test]
        public void SceneLoadedCondition_MatchingEvent_EmitsTrue()
        {
            var condition = new SceneLoadedCondition(_eventBus, "TestScene");
            bool? result = null;
            var sub = condition.Evaluate().Subscribe(v => result = v);

            _eventBus.Publish("SceneLoaded", "TestScene");
            Assert.AreEqual(true, result);
            sub.Dispose();
        }

        [Test]
        public void SceneLoadedCondition_NonMatchingEvent_EmitsFalse()
        {
            var condition = new SceneLoadedCondition(_eventBus, "TestScene");
            bool? result = null;
            var sub = condition.Evaluate().Subscribe(v => result = v);

            _eventBus.Publish("SceneLoaded", "OtherScene");
            Assert.AreEqual(false, result);
            sub.Dispose();
        }

        // === ObjectExistsCondition ===

        [Test]
        public void ObjectExistsCondition_Construction_Succeeds()
        {
            var condition = new ObjectExistsCondition(_resolver, "obj1");
            Assert.IsNotNull(condition);
        }

        [Test]
        public void ObjectExistsCondition_Evaluate_ReturnsObservable()
        {
            var condition = new ObjectExistsCondition(_resolver, "obj1");
            Assert.IsNotNull(condition.Evaluate());
        }

        // === ObjectDestroyedCondition ===

        [Test]
        public void ObjectDestroyedCondition_Construction_Succeeds()
        {
            var condition = new ObjectDestroyedCondition(_resolver, "obj1");
            Assert.IsNotNull(condition);
        }

        [Test]
        public void ObjectDestroyedCondition_Evaluate_ReturnsObservable()
        {
            var condition = new ObjectDestroyedCondition(_resolver, "obj1");
            Assert.IsNotNull(condition.Evaluate());
        }

        // === PhysicsCollisionCondition ===

        [Test]
        public void PhysicsCollisionCondition_Construction_Succeeds()
        {
            var condition = new PhysicsCollisionCondition(_eventBus, "objA", "objB");
            Assert.IsNotNull(condition);
        }

        [Test]
        public void PhysicsCollisionCondition_Evaluate_ReturnsObservable()
        {
            var condition = new PhysicsCollisionCondition(_eventBus, "objA", "objB");
            Assert.IsNotNull(condition.Evaluate());
        }

        // === TriggerEnterCondition ===

        [Test]
        public void TriggerEnterCondition_Construction_Succeeds()
        {
            var condition = new TriggerEnterCondition(_eventBus, "trigger1", "obj1");
            Assert.IsNotNull(condition);
        }

        [Test]
        public void TriggerEnterCondition_Evaluate_ReturnsObservable()
        {
            var condition = new TriggerEnterCondition(_eventBus, "trigger1", "obj1");
            Assert.IsNotNull(condition.Evaluate());
        }

        // === TriggerExitCondition ===

        [Test]
        public void TriggerExitCondition_Construction_Succeeds()
        {
            var condition = new TriggerExitCondition(_eventBus, "trigger1", "obj1");
            Assert.IsNotNull(condition);
        }

        [Test]
        public void TriggerExitCondition_Evaluate_ReturnsObservable()
        {
            var condition = new TriggerExitCondition(_eventBus, "trigger1", "obj1");
            Assert.IsNotNull(condition.Evaluate());
        }

        // === RaycastHitCondition ===

        [Test]
        public void RaycastHitCondition_Construction_Succeeds()
        {
            var condition = new RaycastHitCondition(_resolver, "src", "tgt", 100f, -1);
            Assert.IsNotNull(condition);
        }

        [Test]
        public void RaycastHitCondition_Evaluate_ReturnsObservable()
        {
            var condition = new RaycastHitCondition(_resolver, "src", "tgt", 100f, -1);
            Assert.IsNotNull(condition.Evaluate());
        }

        // === LayerMaskCondition ===

        [Test]
        public void LayerMaskCondition_Construction_Succeeds()
        {
            var condition = new LayerMaskCondition(_resolver, "obj1", 1);
            Assert.IsNotNull(condition);
        }

        [Test]
        public void LayerMaskCondition_Evaluate_ReturnsObservable()
        {
            var condition = new LayerMaskCondition(_resolver, "obj1", 1);
            Assert.IsNotNull(condition.Evaluate());
        }

        // === Reset/Dispose ===

        [Test]
        public void AllEnvironmentConditions_ResetAndDispose_DoNotThrow()
        {
            var conditions = new ICondition[]
            {
                new SceneLoadedCondition(_eventBus, "scene"),
                new ObjectExistsCondition(_resolver, "obj"),
                new ObjectDestroyedCondition(_resolver, "obj"),
                new PhysicsCollisionCondition(_eventBus, "a", "b"),
                new TriggerEnterCondition(_eventBus, "t", "o"),
                new TriggerExitCondition(_eventBus, "t", "o"),
                new RaycastHitCondition(_resolver, "s", "t", 100f, -1),
                new LayerMaskCondition(_resolver, "obj", 1),
            };

            foreach (var c in conditions)
            {
                Assert.DoesNotThrow(() => c.Reset(), $"Reset failed for {c.GetType().Name}");
                Assert.DoesNotThrow(() => c.Dispose(), $"Dispose failed for {c.GetType().Name}");
            }
        }
    }
}
