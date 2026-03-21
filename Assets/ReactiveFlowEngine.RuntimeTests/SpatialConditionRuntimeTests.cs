using System;
using NUnit.Framework;
using R3;
using UnityEngine;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Conditions.Spatial;
using ReactiveFlowEngine.Tests.TestDoubles;

namespace ReactiveFlowEngine.RuntimeTests
{
    [TestFixture]
    public class SpatialConditionRuntimeTests
    {
        private RuntimeTestHelper _helper;
        private MockSceneObjectResolver _resolver;

        [SetUp]
        public void SetUp()
        {
            _helper = new RuntimeTestHelper();
            _resolver = new MockSceneObjectResolver();
        }

        [TearDown]
        public void TearDown()
        {
            _helper.TearDown();
        }

        // === DistanceThresholdCondition ===

        [Test]
        public void DistanceThresholdCondition_NullResolver_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new DistanceThresholdCondition(null, "a", "b", 5f, ComparisonOperator.LessThan));
        }

        [Test]
        public void DistanceThresholdCondition_NullTargetId_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new DistanceThresholdCondition(_resolver, null, "b", 5f, ComparisonOperator.LessThan));
        }

        [Test]
        public void DistanceThresholdCondition_NullReferenceId_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new DistanceThresholdCondition(_resolver, "a", null, 5f, ComparisonOperator.LessThan));
        }

        [Test]
        public void DistanceThresholdCondition_Evaluate_ReturnsObservable()
        {
            var condition = new DistanceThresholdCondition(_resolver, "a", "b", 5f, ComparisonOperator.LessThan);
            var observable = condition.Evaluate();
            Assert.IsNotNull(observable);
        }

        [Test]
        public void DistanceThresholdCondition_ResetAndDispose_DoNotThrow()
        {
            var condition = new DistanceThresholdCondition(_resolver, "a", "b", 5f, ComparisonOperator.LessThan);
            Assert.DoesNotThrow(() => condition.Reset());
            Assert.DoesNotThrow(() => condition.Dispose());
        }

        // === ObjectNearCondition ===

        [Test]
        public void ObjectNearCondition_Construction_Succeeds()
        {
            var condition = new ObjectNearCondition(_resolver, "a", "b", 5f);
            Assert.IsNotNull(condition);
            Assert.AreEqual("a", condition.TargetObjectId);
        }

        [Test]
        public void ObjectNearCondition_Evaluate_ReturnsObservable()
        {
            var condition = new ObjectNearCondition(_resolver, "a", "b", 5f);
            Assert.IsNotNull(condition.Evaluate());
        }

        // === ObjectFarCondition ===

        [Test]
        public void ObjectFarCondition_Construction_Succeeds()
        {
            var condition = new ObjectFarCondition(_resolver, "a", "b", 10f);
            Assert.IsNotNull(condition);
            Assert.AreEqual("a", condition.TargetObjectId);
        }

        [Test]
        public void ObjectFarCondition_Evaluate_ReturnsObservable()
        {
            var condition = new ObjectFarCondition(_resolver, "a", "b", 10f);
            Assert.IsNotNull(condition.Evaluate());
        }

        // === ObjectFacingCondition ===

        [Test]
        public void ObjectFacingCondition_Construction_Succeeds()
        {
            var condition = new ObjectFacingCondition(_resolver, "a", "b", 15f);
            Assert.IsNotNull(condition);
            Assert.AreEqual("a", condition.TargetObjectId);
        }

        // === ObjectAlignedCondition ===

        [Test]
        public void ObjectAlignedCondition_Construction_Succeeds()
        {
            var condition = new ObjectAlignedCondition(_resolver, "a", "b", 5f);
            Assert.IsNotNull(condition);
        }

        // === ObjectInZoneCondition ===

        [Test]
        public void ObjectInZoneCondition_Construction_Succeeds()
        {
            var condition = new ObjectInZoneCondition(_resolver, "obj", "zone");
            Assert.IsNotNull(condition);
            Assert.AreEqual("obj", condition.TargetObjectId);
        }

        // === ObjectExitedZoneCondition ===

        [Test]
        public void ObjectExitedZoneCondition_Construction_Succeeds()
        {
            var condition = new ObjectExitedZoneCondition(_resolver, "obj", "zone");
            Assert.IsNotNull(condition);
        }

        // === ObjectInsideBoundsCondition ===

        [Test]
        public void ObjectInsideBoundsCondition_Construction_Succeeds()
        {
            var condition = new ObjectInsideBoundsCondition(_resolver, "obj", Vector3.zero, new Vector3(10, 10, 10));
            Assert.IsNotNull(condition);
            Assert.AreEqual("obj", condition.TargetObjectId);
        }

        // === ObjectOutsideBoundsCondition ===

        [Test]
        public void ObjectOutsideBoundsCondition_Construction_Succeeds()
        {
            var condition = new ObjectOutsideBoundsCondition(_resolver, "obj", Vector3.zero, new Vector3(10, 10, 10));
            Assert.IsNotNull(condition);
        }

        // === ObjectPositionCondition ===

        [Test]
        public void ObjectPositionCondition_Construction_Succeeds()
        {
            var condition = new ObjectPositionCondition(_resolver, "obj", new Vector3(1, 2, 3), 0.5f);
            Assert.IsNotNull(condition);
            Assert.AreEqual("obj", condition.TargetObjectId);
        }

        // === ObjectRotationCondition ===

        [Test]
        public void ObjectRotationCondition_Construction_Succeeds()
        {
            var condition = new ObjectRotationCondition(_resolver, "obj", new Vector3(0, 90, 0), 5f);
            Assert.IsNotNull(condition);
            Assert.AreEqual("obj", condition.TargetObjectId);
        }

        // === All spatial conditions Reset/Dispose ===

        [Test]
        public void AllSpatialConditions_ResetAndDispose_DoNotThrow()
        {
            var conditions = new ISpatialCondition[]
            {
                new ObjectNearCondition(_resolver, "a", "b", 5f),
                new ObjectFarCondition(_resolver, "a", "b", 10f),
                new ObjectFacingCondition(_resolver, "a", "b", 15f),
                new ObjectAlignedCondition(_resolver, "a", "b", 5f),
                new ObjectInZoneCondition(_resolver, "a", "b"),
                new ObjectExitedZoneCondition(_resolver, "a", "b"),
                new ObjectInsideBoundsCondition(_resolver, "a", Vector3.zero, Vector3.one),
                new ObjectOutsideBoundsCondition(_resolver, "a", Vector3.zero, Vector3.one),
                new ObjectPositionCondition(_resolver, "a", Vector3.zero, 1f),
                new ObjectRotationCondition(_resolver, "a", Vector3.zero, 5f),
            };

            foreach (var c in conditions)
            {
                Assert.DoesNotThrow(() => c.Reset(), $"Reset failed for {c.GetType().Name}");
                Assert.DoesNotThrow(() => c.Dispose(), $"Dispose failed for {c.GetType().Name}");
            }
        }
    }
}
