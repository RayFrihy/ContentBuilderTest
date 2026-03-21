using System;
using NUnit.Framework;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Conditions;
using ReactiveFlowEngine.Conditions.Interaction;
using ReactiveFlowEngine.Conditions.Spatial;
using ReactiveFlowEngine.Conditions.TimeBased;
using ReactiveFlowEngine.Conditions.State;
using ReactiveFlowEngine.Conditions.StepFlow;
using ReactiveFlowEngine.Conditions.Composite;
using ReactiveFlowEngine.Conditions.Environment;
using ReactiveFlowEngine.Model;
using ReactiveFlowEngine.Tests.TestDoubles;

namespace ReactiveFlowEngine.Tests
{
    [TestFixture]
    public class ConditionFactoryTests
    {
        private ConditionFactory _factory;
        private MockSceneObjectResolver _resolver;
        private MockEventBus _eventBus;
        private MockStateStore _stateStore;
        private MockFlowEngine _flowEngine;
        private MockHistoryService _historyService;

        [SetUp]
        public void SetUp()
        {
            _resolver = new MockSceneObjectResolver();
            _eventBus = new MockEventBus();
            _stateStore = new MockStateStore();
            _flowEngine = new MockFlowEngine();
            _historyService = new MockHistoryService();
            _factory = new ConditionFactory(_resolver, _eventBus, _stateStore, _flowEngine, _historyService);
        }

        [Test]
        public void Create_NullDefinition_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _factory.Create(null));
        }

        [Test]
        public void Create_UnknownType_ThrowsArgumentException()
        {
            var def = new ConditionDefinition { TypeName = "NonExistentCondition" };
            Assert.Throws<ArgumentException>(() => _factory.Create(def));
        }

        [Test]
        public void Create_TimeoutCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "TimeoutCondition" };
            def.Parameters["Timeout"] = 5.0f;

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<TimeoutCondition>(condition);
        }

        [Test]
        public void Create_ButtonPressedCondition_ReturnsEventBusCondition()
        {
            var def = new ConditionDefinition { TypeName = "ButtonPressedCondition" };
            def.Parameters["ButtonId"] = "btn1";

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<EventBusCondition>(condition);
        }

        [Test]
        public void Create_ObjectTouchedCondition_ReturnsEventBusCondition()
        {
            var def = new ConditionDefinition { TypeName = "ObjectTouchedCondition" };
            def.Parameters["TargetObjectId"] = "obj1";

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<EventBusCondition>(condition);
        }

        [Test]
        public void Create_BooleanStateCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "BooleanStateCondition" };
            def.Parameters["StateKey"] = "testKey";
            def.Parameters["ExpectedValue"] = true;

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<BooleanStateCondition>(condition);
        }

        [Test]
        public void Create_CompositeAndCondition_WithChildren_CombinesCorrectly()
        {
            var child1 = new ConditionDefinition { TypeName = "TimeoutCondition" };
            child1.Parameters["Timeout"] = 1.0f;

            var child2 = new ConditionDefinition { TypeName = "TimeoutCondition" };
            child2.Parameters["Timeout"] = 2.0f;

            var def = new ConditionDefinition { TypeName = "CompositeAndCondition" };
            def.Children.Add(child1);
            def.Children.Add(child2);

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<CompositeAndCondition>(condition);
        }

        [Test]
        public void Create_DelayElapsedCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "DelayElapsedCondition" };
            def.Parameters["Delay"] = 3.0f;

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<DelayElapsedCondition>(condition);
        }

        [Test]
        public void Create_AllConsolidatedInteractionConditions_ReturnEventBusCondition()
        {
            var types = new[]
            {
                ("ObjectGrabbedCondition", "TargetObjectId"),
                ("ObjectReleasedCondition", "TargetObjectId"),
                ("ObjectTouchedCondition", "TargetObjectId"),
                ("ObjectUsedCondition", "TargetObjectId"),
                ("ObjectSelectedCondition", "TargetObjectId"),
                ("ObjectDeselectedCondition", "TargetObjectId"),
                ("ButtonPressedCondition", "ButtonId"),
                ("ButtonReleasedCondition", "ButtonId"),
                ("InputActionTriggeredCondition", "ActionName"),
            };

            foreach (var (typeName, paramKey) in types)
            {
                var def = new ConditionDefinition { TypeName = typeName };
                def.Parameters[paramKey] = "test-id";

                var condition = _factory.Create(def);
                Assert.IsNotNull(condition, $"Failed for {typeName}");
                Assert.IsInstanceOf<EventBusCondition>(condition, $"Failed for {typeName}");
            }
        }

        // === Interaction (non-EventBus) ===

        [Test]
        public void Create_ObjectHoveredCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "ObjectHoveredCondition" };
            def.Parameters["TargetObjectId"] = "obj1";

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<ObjectHoveredCondition>(condition);
        }

        [Test]
        public void Create_GesturePerformedCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "GesturePerformedCondition" };
            def.Parameters["TargetObjectId"] = "obj1";
            def.Parameters["GestureType"] = "Swipe";

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<GesturePerformedCondition>(condition);
        }

        // === Spatial ===

        [Test]
        public void Create_ObjectInZoneCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "ObjectInZoneCondition" };
            def.Parameters["TargetObjectId"] = "obj1";
            def.Parameters["ZoneObjectId"] = "zone1";

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<ObjectInZoneCondition>(condition);
        }

        [Test]
        public void Create_ObjectExitedZoneCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "ObjectExitedZoneCondition" };
            def.Parameters["TargetObjectId"] = "obj1";
            def.Parameters["ZoneObjectId"] = "zone1";

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<ObjectExitedZoneCondition>(condition);
        }

        [Test]
        public void Create_ObjectNearCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "ObjectNearCondition" };
            def.Parameters["TargetObjectId"] = "obj1";
            def.Parameters["ReferenceObjectId"] = "obj2";
            def.Parameters["Threshold"] = 1.0f;

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<ObjectNearCondition>(condition);
        }

        [Test]
        public void Create_ObjectFarCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "ObjectFarCondition" };
            def.Parameters["TargetObjectId"] = "obj1";
            def.Parameters["ReferenceObjectId"] = "obj2";
            def.Parameters["Threshold"] = 10.0f;

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<ObjectFarCondition>(condition);
        }

        [Test]
        public void Create_ObjectAlignedCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "ObjectAlignedCondition" };
            def.Parameters["TargetObjectId"] = "obj1";
            def.Parameters["ReferenceObjectId"] = "obj2";
            def.Parameters["AngleTolerance"] = 5.0f;

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<ObjectAlignedCondition>(condition);
        }

        [Test]
        public void Create_ObjectRotationCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "ObjectRotationCondition" };
            def.Parameters["TargetObjectId"] = "obj1";
            def.Parameters["TargetX"] = 0f;
            def.Parameters["TargetY"] = 90f;
            def.Parameters["TargetZ"] = 0f;
            def.Parameters["AngleTolerance"] = 5.0f;

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<ObjectRotationCondition>(condition);
        }

        [Test]
        public void Create_ObjectPositionCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "ObjectPositionCondition" };
            def.Parameters["TargetObjectId"] = "obj1";
            def.Parameters["TargetX"] = 1f;
            def.Parameters["TargetY"] = 2f;
            def.Parameters["TargetZ"] = 3f;
            def.Parameters["DistanceTolerance"] = 0.5f;

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<ObjectPositionCondition>(condition);
        }

        [Test]
        public void Create_ObjectFacingCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "ObjectFacingCondition" };
            def.Parameters["TargetObjectId"] = "obj1";
            def.Parameters["FacingObjectId"] = "obj2";
            def.Parameters["AngleTolerance"] = 10.0f;

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<ObjectFacingCondition>(condition);
        }

        [Test]
        public void Create_ObjectInsideBoundsCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "ObjectInsideBoundsCondition" };
            def.Parameters["TargetObjectId"] = "obj1";
            def.Parameters["CenterX"] = 0f;
            def.Parameters["CenterY"] = 0f;
            def.Parameters["CenterZ"] = 0f;
            def.Parameters["SizeX"] = 10f;
            def.Parameters["SizeY"] = 10f;
            def.Parameters["SizeZ"] = 10f;

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<ObjectInsideBoundsCondition>(condition);
        }

        [Test]
        public void Create_ObjectOutsideBoundsCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "ObjectOutsideBoundsCondition" };
            def.Parameters["TargetObjectId"] = "obj1";
            def.Parameters["CenterX"] = 0f;
            def.Parameters["CenterY"] = 0f;
            def.Parameters["CenterZ"] = 0f;
            def.Parameters["SizeX"] = 10f;
            def.Parameters["SizeY"] = 10f;
            def.Parameters["SizeZ"] = 10f;

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<ObjectOutsideBoundsCondition>(condition);
        }

        [Test]
        public void Create_DistanceThresholdCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "DistanceThresholdCondition" };
            def.Parameters["TargetObjectId"] = "obj1";
            def.Parameters["ReferenceObjectId"] = "obj2";
            def.Parameters["Threshold"] = 5.0f;
            def.Parameters["Operator"] = "LessThan";

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<DistanceThresholdCondition>(condition);
        }

        // === Time-based ===

        [Test]
        public void Create_ElapsedTimeCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "ElapsedTimeCondition" };
            def.Parameters["RequiredElapsed"] = 5.0f;
            def.Parameters["Operator"] = "GreaterThan";

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<ElapsedTimeCondition>(condition);
        }

        [Test]
        public void Create_TimerRunningCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "TimerRunningCondition" };
            def.Parameters["TimerId"] = "timer1";

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<TimerRunningCondition>(condition);
        }

        [Test]
        public void Create_CooldownCompleteCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "CooldownCompleteCondition" };
            def.Parameters["CooldownId"] = "cd1";
            def.Parameters["Duration"] = 3.0f;

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<CooldownCompleteCondition>(condition);
        }

        // === State ===

        [Test]
        public void Create_IntegerStateCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "IntegerStateCondition" };
            def.Parameters["StateKey"] = "count";
            def.Parameters["CompareValue"] = 5;
            def.Parameters["Operator"] = "Equal";

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<IntegerStateCondition>(condition);
        }

        [Test]
        public void Create_FloatStateCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "FloatStateCondition" };
            def.Parameters["StateKey"] = "score";
            def.Parameters["CompareValue"] = 3.14f;
            def.Parameters["Operator"] = "LessThan";
            def.Parameters["Tolerance"] = 0.01f;

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<FloatStateCondition>(condition);
        }

        [Test]
        public void Create_StringStateCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "StringStateCondition" };
            def.Parameters["StateKey"] = "name";
            def.Parameters["ExpectedValue"] = "hello";
            def.Parameters["IgnoreCase"] = true;

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<StringStateCondition>(condition);
        }

        [Test]
        public void Create_StateEqualsCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "StateEqualsCondition" };
            def.Parameters["StateKey"] = "myKey";
            def.Parameters["ExpectedValue"] = "myValue";

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<StateEqualsCondition>(condition);
        }

        [Test]
        public void Create_StateChangedCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "StateChangedCondition" };
            def.Parameters["StateKey"] = "myKey";

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<StateChangedCondition>(condition);
        }

        [Test]
        public void Create_StateExistsCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "StateExistsCondition" };
            def.Parameters["StateKey"] = "myKey";

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<StateExistsCondition>(condition);
        }

        [Test]
        public void Create_StateNotExistsCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "StateNotExistsCondition" };
            def.Parameters["StateKey"] = "myKey";

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<StateNotExistsCondition>(condition);
        }

        [Test]
        public void Create_VariableComparisonCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "VariableComparisonCondition" };
            def.Parameters["LeftKey"] = "keyA";
            def.Parameters["RightKey"] = "keyB";
            def.Parameters["Operator"] = "Equal";

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<VariableComparisonCondition>(condition);
        }

        [Test]
        public void Create_ObjectPropertyCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "ObjectPropertyCondition" };
            def.Parameters["TargetObjectId"] = "obj1";
            def.Parameters["PropertyName"] = "isActive";
            def.Parameters["ExpectedValue"] = true;

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<ObjectPropertyCondition>(condition);
        }

        [Test]
        public void Create_FlagSetCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "FlagSetCondition" };
            def.Parameters["FlagKey"] = "myFlag";

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<FlagSetCondition>(condition);
        }

        // === Step/Flow ===

        [Test]
        public void Create_StepCompletedCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "StepCompletedCondition" };
            def.Parameters["StepId"] = "step-1";

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<StepCompletedCondition>(condition);
        }

        [Test]
        public void Create_StepActiveCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "StepActiveCondition" };
            def.Parameters["StepId"] = "step-1";

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<StepActiveCondition>(condition);
        }

        [Test]
        public void Create_PreviousStepCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "PreviousStepCondition" };
            def.Parameters["ExpectedPreviousStepId"] = "step-0";

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<PreviousStepCondition>(condition);
        }

        [Test]
        public void Create_NextStepAvailableCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "NextStepAvailableCondition" };

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<NextStepAvailableCondition>(condition);
        }

        [Test]
        public void Create_TransitionAvailableCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "TransitionAvailableCondition" };
            def.Parameters["TargetStepId"] = "step-2";

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<TransitionAvailableCondition>(condition);
        }

        [Test]
        public void Create_ProcessStartedCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "ProcessStartedCondition" };

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<ProcessStartedCondition>(condition);
        }

        [Test]
        public void Create_ProcessCompletedCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "ProcessCompletedCondition" };

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<ProcessCompletedCondition>(condition);
        }

        // === Composite ===

        [Test]
        public void Create_CompositeOrCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "CompositeOrCondition" };

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<CompositeOrCondition>(condition);
        }

        [Test]
        public void Create_CompositeNotCondition_WithChild_ReturnsInstance()
        {
            var child = new ConditionDefinition { TypeName = "TimeoutCondition" };
            child.Parameters["Timeout"] = 1.0f;

            var def = new ConditionDefinition { TypeName = "CompositeNotCondition" };
            def.Children.Add(child);

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<CompositeNotCondition>(condition);
        }

        [Test]
        public void Create_XorCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "XorCondition" };

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<XorCondition>(condition);
        }

        [Test]
        public void Create_CompositeCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "CompositeCondition" };
            def.Parameters["Mode"] = "All";
            def.Parameters["Threshold"] = 0;

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<CompositeCondition>(condition);
        }

        [Test]
        public void Create_WeightedCondition_ReturnsInstance()
        {
            var child = new ConditionDefinition { TypeName = "TimeoutCondition" };
            child.Parameters["Timeout"] = 1.0f;

            var def = new ConditionDefinition { TypeName = "WeightedCondition" };
            def.Parameters["RequiredWeightThreshold"] = 0.5f;
            def.Parameters["Weights"] = new[] { "1.0" };
            def.Children.Add(child);

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<WeightedCondition>(condition);
        }

        [Test]
        public void Create_PriorityCondition_ReturnsInstance()
        {
            var child = new ConditionDefinition { TypeName = "TimeoutCondition" };
            child.Parameters["Timeout"] = 1.0f;

            var def = new ConditionDefinition { TypeName = "PriorityCondition" };
            def.Parameters["Priorities"] = new[] { "0" };
            def.Children.Add(child);

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<PriorityCondition>(condition);
        }

        // === Environment ===

        [Test]
        public void Create_SceneLoadedCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "SceneLoadedCondition" };
            def.Parameters["SceneName"] = "TestScene";

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<SceneLoadedCondition>(condition);
        }

        [Test]
        public void Create_ObjectExistsCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "ObjectExistsCondition" };
            def.Parameters["TargetObjectId"] = "obj1";

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<ObjectExistsCondition>(condition);
        }

        [Test]
        public void Create_ObjectDestroyedCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "ObjectDestroyedCondition" };
            def.Parameters["TargetObjectId"] = "obj1";

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<ObjectDestroyedCondition>(condition);
        }

        [Test]
        public void Create_PhysicsCollisionCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "PhysicsCollisionCondition" };
            def.Parameters["ObjectAId"] = "obj1";
            def.Parameters["ObjectBId"] = "obj2";

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<PhysicsCollisionCondition>(condition);
        }

        [Test]
        public void Create_TriggerEnterCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "TriggerEnterCondition" };
            def.Parameters["TriggerObjectId"] = "trigger1";
            def.Parameters["EnteringObjectId"] = "obj1";

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<TriggerEnterCondition>(condition);
        }

        [Test]
        public void Create_TriggerExitCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "TriggerExitCondition" };
            def.Parameters["TriggerObjectId"] = "trigger1";
            def.Parameters["ExitingObjectId"] = "obj1";

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<TriggerExitCondition>(condition);
        }

        [Test]
        public void Create_RaycastHitCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "RaycastHitCondition" };
            def.Parameters["SourceObjectId"] = "src1";
            def.Parameters["TargetObjectId"] = "tgt1";
            def.Parameters["MaxDistance"] = 100f;
            def.Parameters["LayerMask"] = -1;

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<RaycastHitCondition>(condition);
        }

        [Test]
        public void Create_LayerMaskCondition_ReturnsInstance()
        {
            var def = new ConditionDefinition { TypeName = "LayerMaskCondition" };
            def.Parameters["TargetObjectId"] = "obj1";
            def.Parameters["ExpectedLayerMask"] = 1;

            var condition = _factory.Create(def);
            Assert.IsNotNull(condition);
            Assert.IsInstanceOf<LayerMaskCondition>(condition);
        }
    }
}
