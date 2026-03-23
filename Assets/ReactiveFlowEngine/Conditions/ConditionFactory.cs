using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using ReactiveFlowEngine.Conditions.Interaction;
using ReactiveFlowEngine.Conditions.Spatial;
using ReactiveFlowEngine.Conditions.TimeBased;
using ReactiveFlowEngine.Conditions.State;
using ReactiveFlowEngine.Conditions.StepFlow;
using ReactiveFlowEngine.Conditions.Composite;
using ReactiveFlowEngine.Conditions.Environment;

namespace ReactiveFlowEngine.Conditions
{
    public class ConditionFactory
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly IEventBus _eventBus;
        private readonly IStateStore _stateStore;
        private readonly IFlowEngine _flowEngine;
        private readonly IHistoryService _historyService;

        public ConditionFactory(
            ISceneObjectResolver resolver,
            IEventBus eventBus,
            IStateStore stateStore,
            IFlowEngine flowEngine,
            IHistoryService historyService = null)
        {
            _resolver = resolver;
            _eventBus = eventBus;
            _stateStore = stateStore;
            _flowEngine = flowEngine;
            _historyService = historyService;
        }

        public ICondition Create(ConditionDefinition definition)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));

            return definition.TypeName switch
            {
                // Existing conditions
                "TimeoutCondition" => new TimeoutCondition(definition.GetFloat("Timeout")),
                "CompositeAndCondition" => CreateCompositeAnd(definition),
                "CompositeOrCondition" => CreateCompositeOr(definition),
                "CompositeNotCondition" => CreateCompositeNot(definition),

                // Interaction conditions (consolidated via EventBusCondition)
                "ObjectGrabbedCondition" => new EventBusCondition(_eventBus, "ObjectGrabbed", definition.GetString("TargetObjectId")),
                "GrabbedCondition" => new EventBusCondition(_eventBus, "ObjectGrabbed", definition.GetString("TargetObjectId") ?? definition.GetString("Targets")),
                "ObjectReleasedCondition" => new EventBusCondition(_eventBus, "ObjectReleased", definition.GetString("TargetObjectId")),
                "ObjectTouchedCondition" => new EventBusCondition(_eventBus, "ObjectTouched", definition.GetString("TargetObjectId")),
                "ObjectUsedCondition" => new EventBusCondition(_eventBus, "ObjectUsed", definition.GetString("TargetObjectId")),
                "ObjectSelectedCondition" => new EventBusCondition(_eventBus, "ObjectSelected", definition.GetString("TargetObjectId")),
                "ObjectDeselectedCondition" => new EventBusCondition(_eventBus, "ObjectDeselected", definition.GetString("TargetObjectId")),
                "ButtonPressedCondition" => new EventBusCondition(_eventBus, "ButtonPressed", definition.GetString("ButtonId")),
                "ButtonReleasedCondition" => new EventBusCondition(_eventBus, "ButtonReleased", definition.GetString("ButtonId")),
                "InputActionTriggeredCondition" => new EventBusCondition(_eventBus, "InputActionTriggered", definition.GetString("ActionName")),
                // ObjectHoveredCondition kept as separate class (dual enter/exit events)
                "ObjectHoveredCondition" => new ObjectHoveredCondition(_eventBus, definition.GetString("TargetObjectId")),
                "GesturePerformedCondition" => new GesturePerformedCondition(
                    _eventBus,
                    definition.GetString("TargetObjectId"),
                    definition.GetEnum<GestureType>("GestureType")),

                // Spatial conditions
                "ObjectInZoneCondition" => new ObjectInZoneCondition(_resolver, definition.GetString("TargetObjectId"), definition.GetString("ZoneObjectId")),
                "ObjectExitedZoneCondition" => new ObjectExitedZoneCondition(_resolver, definition.GetString("TargetObjectId"), definition.GetString("ZoneObjectId")),
                "ObjectNearCondition" => new ObjectNearCondition(_resolver, definition.GetString("TargetObjectId"), definition.GetString("ReferenceObjectId"), definition.GetFloat("Threshold")),
                "ObjectFarCondition" => new ObjectFarCondition(_resolver, definition.GetString("TargetObjectId"), definition.GetString("ReferenceObjectId"), definition.GetFloat("Threshold")),
                "ObjectAlignedCondition" => new ObjectAlignedCondition(_resolver, definition.GetString("TargetObjectId"), definition.GetString("ReferenceObjectId"), definition.GetFloat("AngleTolerance")),
                "ObjectRotationCondition" => new ObjectRotationCondition(
                    _resolver,
                    definition.GetString("TargetObjectId"),
                    new Vector3(definition.GetFloat("TargetX"), definition.GetFloat("TargetY"), definition.GetFloat("TargetZ")),
                    definition.GetFloat("AngleTolerance")),
                "ObjectPositionCondition" => new ObjectPositionCondition(
                    _resolver,
                    definition.GetString("TargetObjectId"),
                    new Vector3(definition.GetFloat("TargetX"), definition.GetFloat("TargetY"), definition.GetFloat("TargetZ")),
                    definition.GetFloat("DistanceTolerance")),
                "ObjectFacingCondition" => new ObjectFacingCondition(_resolver, definition.GetString("TargetObjectId"), definition.GetString("FacingObjectId"), definition.GetFloat("AngleTolerance")),
                "ObjectInsideBoundsCondition" => new ObjectInsideBoundsCondition(
                    _resolver,
                    definition.GetString("TargetObjectId"),
                    new Vector3(definition.GetFloat("CenterX"), definition.GetFloat("CenterY"), definition.GetFloat("CenterZ")),
                    new Vector3(definition.GetFloat("SizeX"), definition.GetFloat("SizeY"), definition.GetFloat("SizeZ"))),
                "ObjectOutsideBoundsCondition" => new ObjectOutsideBoundsCondition(
                    _resolver,
                    definition.GetString("TargetObjectId"),
                    new Vector3(definition.GetFloat("CenterX"), definition.GetFloat("CenterY"), definition.GetFloat("CenterZ")),
                    new Vector3(definition.GetFloat("SizeX"), definition.GetFloat("SizeY"), definition.GetFloat("SizeZ"))),
                "DistanceThresholdCondition" => new DistanceThresholdCondition(
                    _resolver,
                    definition.GetString("TargetObjectId"),
                    definition.GetString("ReferenceObjectId"),
                    definition.GetFloat("Threshold"),
                    definition.GetEnum<ComparisonOperator>("Operator")),

                // Time-based conditions
                "DelayElapsedCondition" => new DelayElapsedCondition(definition.GetFloat("Delay")),
                "TimerRunningCondition" => new TimerRunningCondition(_eventBus, definition.GetString("TimerId")),
                "CooldownCompleteCondition" => new CooldownCompleteCondition(_eventBus, definition.GetString("CooldownId"), definition.GetFloat("Duration")),
                "ElapsedTimeCondition" => new ElapsedTimeCondition(definition.GetFloat("RequiredElapsed"), definition.GetEnum<ComparisonOperator>("Operator")),

                // State conditions
                "BooleanStateCondition" => new BooleanStateCondition(_stateStore, definition.GetString("StateKey"), definition.GetBool("ExpectedValue")),
                "IntegerStateCondition" => new IntegerStateCondition(_stateStore, definition.GetString("StateKey"), definition.GetInt("CompareValue"), definition.GetEnum<ComparisonOperator>("Operator")),
                "FloatStateCondition" => new FloatStateCondition(_stateStore, definition.GetString("StateKey"), definition.GetFloat("CompareValue"), definition.GetEnum<ComparisonOperator>("Operator"), definition.GetFloat("Tolerance")),
                "StringStateCondition" => new StringStateCondition(_stateStore, definition.GetString("StateKey"), definition.GetString("ExpectedValue"), definition.GetBool("IgnoreCase")),
                "StateEqualsCondition" => new StateEqualsCondition(_stateStore, definition.GetString("StateKey"), definition.Parameters.ContainsKey("ExpectedValue") ? definition.Parameters["ExpectedValue"] : null),
                "StateChangedCondition" => new StateChangedCondition(_stateStore, definition.GetString("StateKey")),
                "StateExistsCondition" => new StateExistsCondition(_stateStore, definition.GetString("StateKey")),
                "StateNotExistsCondition" => new StateNotExistsCondition(_stateStore, definition.GetString("StateKey")),
                "VariableComparisonCondition" => new VariableComparisonCondition(_stateStore, definition.GetString("LeftKey"), definition.GetString("RightKey"), definition.GetEnum<ComparisonOperator>("Operator")),
                "ObjectPropertyCondition" => new ObjectPropertyCondition(_resolver, definition.GetString("TargetObjectId"), definition.GetString("PropertyName"), definition.Parameters.ContainsKey("ExpectedValue") ? definition.Parameters["ExpectedValue"] : null),
                "FlagSetCondition" => new FlagSetCondition(_stateStore, definition.GetString("FlagKey")),

                // Step/Flow conditions
                "StepCompletedCondition" => new StepCompletedCondition(_historyService, definition.GetString("StepId")),
                "StepActiveCondition" => new StepActiveCondition(_flowEngine, definition.GetString("StepId")),
                "PreviousStepCondition" => new PreviousStepCondition(_historyService, definition.GetString("ExpectedPreviousStepId")),
                "NextStepAvailableCondition" => new NextStepAvailableCondition(_flowEngine),
                "TransitionAvailableCondition" => new TransitionAvailableCondition(_flowEngine, definition.GetString("TargetStepId")),
                "ProcessStartedCondition" => new ProcessStartedCondition(_flowEngine),
                "ProcessCompletedCondition" => new ProcessCompletedCondition(_flowEngine),

                // Composite conditions
                "XorCondition" => CreateXor(definition),
                "CompositeCondition" => CreateComposite(definition),
                "WeightedCondition" => CreateWeighted(definition),
                "PriorityCondition" => CreatePriority(definition),

                // Environment conditions
                "SceneLoadedCondition" => new SceneLoadedCondition(_eventBus, definition.GetString("SceneName")),
                "ObjectExistsCondition" => new ObjectExistsCondition(_resolver, definition.GetString("TargetObjectId")),
                "ObjectDestroyedCondition" => new ObjectDestroyedCondition(_resolver, definition.GetString("TargetObjectId")),
                "PhysicsCollisionCondition" => new PhysicsCollisionCondition(_eventBus, definition.GetString("ObjectAId"), definition.GetString("ObjectBId")),
                "TriggerEnterCondition" => new TriggerEnterCondition(_eventBus, definition.GetString("TriggerObjectId"), definition.GetString("EnteringObjectId")),
                "TriggerExitCondition" => new TriggerExitCondition(_eventBus, definition.GetString("TriggerObjectId"), definition.GetString("ExitingObjectId")),
                "RaycastHitCondition" => new RaycastHitCondition(_resolver, definition.GetString("SourceObjectId"), definition.GetString("TargetObjectId"), definition.GetFloat("MaxDistance"), definition.GetInt("LayerMask")),
                "LayerMaskCondition" => new LayerMaskCondition(_resolver, definition.GetString("TargetObjectId"), definition.GetInt("ExpectedLayerMask")),

                _ => throw new ArgumentException($"Unknown condition type: {definition.TypeName}")
            };
        }

        private ICondition[] CreateChildConditions(ConditionDefinition definition)
        {
            if (definition.Children == null || definition.Children.Count == 0)
                return Array.Empty<ICondition>();

            return definition.Children.Select(child => Create(child)).ToArray();
        }

        private ICondition CreateCompositeAnd(ConditionDefinition definition)
        {
            return new CompositeAndCondition(CreateChildConditions(definition));
        }

        private ICondition CreateCompositeOr(ConditionDefinition definition)
        {
            return new CompositeOrCondition(CreateChildConditions(definition));
        }

        private ICondition CreateCompositeNot(ConditionDefinition definition)
        {
            var children = CreateChildConditions(definition);
            if (children.Length == 0)
                throw new ArgumentException("CompositeNotCondition requires exactly one child condition.");
            return new CompositeNotCondition(children[0]);
        }

        private ICondition CreateXor(ConditionDefinition definition)
        {
            return new XorCondition(CreateChildConditions(definition));
        }

        private ICondition CreateComposite(ConditionDefinition definition)
        {
            var mode = definition.GetEnum<CompositeMode>("Mode");
            var threshold = definition.GetInt("Threshold");
            return new CompositeCondition(mode, threshold, CreateChildConditions(definition));
        }

        private ICondition CreateWeighted(ConditionDefinition definition)
        {
            var requiredThreshold = definition.GetFloat("RequiredWeightThreshold");
            var children = CreateChildConditions(definition);
            var weights = definition.GetStringArray("Weights");

            var entries = new WeightedConditionEntry[children.Length];
            for (int i = 0; i < children.Length; i++)
            {
                float weight = i < weights.Length && float.TryParse(weights[i], out var w) ? w : 1f;
                entries[i] = new WeightedConditionEntry(children[i], weight);
            }

            return new WeightedCondition(requiredThreshold, entries);
        }

        private ICondition CreatePriority(ConditionDefinition definition)
        {
            var children = CreateChildConditions(definition);
            var priorities = definition.GetStringArray("Priorities");

            var entries = new PriorityConditionEntry[children.Length];
            for (int i = 0; i < children.Length; i++)
            {
                int priority = i < priorities.Length && int.TryParse(priorities[i], out var p) ? p : i;
                entries[i] = new PriorityConditionEntry(children[i], priority);
            }

            return new PriorityCondition(entries);
        }
    }
}
