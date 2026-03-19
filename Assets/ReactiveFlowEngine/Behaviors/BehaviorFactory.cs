using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Conditions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class BehaviorFactory
    {
        private readonly ISceneObjectResolver _sceneResolver;
        private readonly IStateStore _stateStore;
        private readonly INavigationService _navigationService;
        private readonly IFlowEngine _flowEngine;
        private readonly IStepRunner _stepRunner;
        private readonly IEventBus _eventBus;
        private readonly ConditionFactory _conditionFactory;

        public BehaviorFactory(
            ISceneObjectResolver sceneResolver,
            IStateStore stateStore,
            INavigationService navigationService,
            IFlowEngine flowEngine,
            IStepRunner stepRunner,
            IEventBus eventBus,
            ConditionFactory conditionFactory)
        {
            _sceneResolver = sceneResolver;
            _stateStore = stateStore;
            _navigationService = navigationService;
            _flowEngine = flowEngine;
            _stepRunner = stepRunner;
            _eventBus = eventBus;
            _conditionFactory = conditionFactory;
        }

        public IBehavior Create(BehaviorDefinition definition)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));

            return definition.TypeName switch
            {
                // === Existing ===
                "MoveObjectBehavior" => CreateMoveObject(definition),
                "ExecuteChapterBehavior" => CreateExecuteChapter(definition),
                "DelayBehavior" => new DelayBehavior(definition.GetFloat("Duration")),

                // === Object Control ===
                "RotateObjectBehavior" => CreateRotateObject(definition),
                "ScaleObjectBehavior" => CreateScaleObject(definition),
                "SetTransformBehavior" => CreateSetTransform(definition),
                "TeleportObjectBehavior" => CreateTeleportObject(definition),
                "EnableObjectBehavior" => new EnableObjectBehavior(
                    _sceneResolver, definition.GetString("TargetObject"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "DisableObjectBehavior" => new DisableObjectBehavior(
                    _sceneResolver, definition.GetString("TargetObject"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "DestroyObjectBehavior" => new DestroyObjectBehavior(
                    _sceneResolver, definition.GetString("TargetObject"),
                    definition.GetFloat("Delay"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "SpawnObjectBehavior" => new SpawnObjectBehavior(
                    _sceneResolver, definition.GetString("PrefabObject"),
                    definition.GetVector3("Position"), definition.GetQuaternion("Rotation"),
                    definition.GetString("ParentObject"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "AttachObjectBehavior" => new AttachObjectBehavior(
                    _sceneResolver, definition.GetString("ChildObject"), definition.GetString("ParentObject"),
                    definition.GetBool("WorldPositionStays"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "DetachObjectBehavior" => new DetachObjectBehavior(
                    _sceneResolver, definition.GetString("ChildObject"),
                    definition.GetBool("WorldPositionStays"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "SetParentBehavior" => new SetParentBehavior(
                    _sceneResolver, definition.GetString("ChildObject"), definition.GetString("ParentObject"),
                    definition.GetBool("WorldPositionStays"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),

                // === Visual ===
                "HighlightObjectBehavior" => new HighlightObjectBehavior(
                    _sceneResolver, definition.GetString("TargetObject"),
                    definition.GetColor("HighlightColor"), definition.GetFloat("Intensity"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "UnhighlightObjectBehavior" => new UnhighlightObjectBehavior(
                    _sceneResolver, definition.GetString("TargetObject"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "ChangeMaterialBehavior" => new ChangeMaterialBehavior(
                    _sceneResolver, definition.GetString("TargetObject"),
                    definition.GetString("MaterialPath"), definition.GetInt("MaterialIndex"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "FadeObjectBehavior" => new FadeObjectBehavior(
                    _sceneResolver, definition.GetString("TargetObject"),
                    definition.GetFloat("TargetAlpha"), definition.GetFloat("Duration"),
                    definition.GetAnimationCurve("AnimationCurve"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "ShowObjectBehavior" => new ShowObjectBehavior(
                    _sceneResolver, definition.GetString("TargetObject"),
                    definition.GetBool("IncludeChildren"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "HideObjectBehavior" => new HideObjectBehavior(
                    _sceneResolver, definition.GetString("TargetObject"),
                    definition.GetBool("IncludeChildren"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "PlayAnimationBehavior" => new PlayAnimationBehavior(
                    _sceneResolver, definition.GetString("TargetObject"),
                    definition.GetString("StateName"), definition.GetInt("Layer"),
                    definition.GetBool("WaitForCompletion"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "StopAnimationBehavior" => new StopAnimationBehavior(
                    _sceneResolver, definition.GetString("TargetObject"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "SetAnimationStateBehavior" => new SetAnimationStateBehavior(
                    _sceneResolver, definition.GetString("TargetObject"),
                    definition.GetString("ParameterName"), definition.GetObject("ParameterValue"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "TriggerParticleEffectBehavior" => new TriggerParticleEffectBehavior(
                    _sceneResolver, definition.GetString("TargetObject"),
                    definition.GetBool("Play"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),

                // === Audio ===
                "PlayAudioBehavior" => new PlayAudioBehavior(
                    _sceneResolver, definition.GetString("AudioSource"),
                    definition.GetString("ClipPath"), definition.GetBool("WaitForCompletion"),
                    definition.GetFloat("Volume"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "StopAudioBehavior" => new StopAudioBehavior(
                    _sceneResolver, definition.GetString("AudioSource"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "PauseAudioBehavior" => new PauseAudioBehavior(
                    _sceneResolver, definition.GetString("AudioSource"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "ResumeAudioBehavior" => new ResumeAudioBehavior(
                    _sceneResolver, definition.GetString("AudioSource"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "LoopAudioBehavior" => new LoopAudioBehavior(
                    _sceneResolver, definition.GetString("AudioSource"),
                    definition.GetString("ClipPath"), definition.GetFloat("Volume"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "PlaySpatialAudioBehavior" => new PlaySpatialAudioBehavior(
                    _sceneResolver, definition.GetString("AudioSource"),
                    definition.GetString("ClipPath"), definition.GetString("PositionObject"),
                    definition.GetFloat("Volume"), definition.GetBool("WaitForCompletion"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "TextToSpeechBehavior" => new TextToSpeechBehavior(
                    definition.GetString("Text"), definition.GetString("Language"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),

                // === Flow/Timing ===
                "WaitUntilConditionBehavior" => CreateWaitUntilCondition(definition),
                "TimeoutBehavior" => new TimeoutBehavior(
                    definition.GetFloat("Duration"), definition.GetString("WarningMessage"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "StartTimerBehavior" => new StartTimerBehavior(
                    _stateStore, definition.GetString("TimerName"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "StopTimerBehavior" => new StopTimerBehavior(
                    _stateStore, definition.GetString("TimerName"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "ResetTimerBehavior" => new ResetTimerBehavior(
                    _stateStore, definition.GetString("TimerName"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),

                // === State/Data ===
                "SetStateBehavior" => new SetStateBehavior(
                    _stateStore, definition.GetString("Key"), definition.GetObject("Value"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "UpdateStateBehavior" => new UpdateStateBehavior(
                    _stateStore, definition.GetString("Key"), definition.GetObject("Value"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "IncrementStateBehavior" => new IncrementStateBehavior(
                    _stateStore, definition.GetString("Key"), definition.GetFloat("Amount"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "DecrementStateBehavior" => new DecrementStateBehavior(
                    _stateStore, definition.GetString("Key"), definition.GetFloat("Amount"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "ToggleStateBehavior" => new ToggleStateBehavior(
                    _stateStore, definition.GetString("Key"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "ClearStateBehavior" => new ClearStateBehavior(
                    _stateStore, definition.GetString("Key"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "CopyStateBehavior" => new CopyStateBehavior(
                    _stateStore, definition.GetString("SourceKey"), definition.GetString("DestinationKey"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "SaveStateBehavior" => new SaveStateBehavior(
                    _stateStore, definition.GetString("SnapshotKey"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "LoadStateBehavior" => new LoadStateBehavior(
                    _stateStore, definition.GetString("SnapshotKey"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),

                // === Flow Control ===
                "ForceTransitionBehavior" => new ForceTransitionBehavior(
                    _stepRunner,
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "CancelTransitionBehavior" => new CancelTransitionBehavior(
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "RestartStepBehavior" => new RestartStepBehavior(
                    _navigationService,
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "SkipStepBehavior" => new SkipStepBehavior(
                    _navigationService,
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "BranchBehavior" => CreateBranch(definition),
                "EndProcessBehavior" => new EndProcessBehavior(
                    _flowEngine,
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),

                // === Sequence/Composition ===
                "BehaviorSequence" => CreateBehaviorSequence(definition),
                "ParallelBehavior" => CreateParallelBehavior(definition),
                "ConditionalBehavior" => CreateConditionalBehavior(definition),
                "RepeatBehavior" => CreateRepeatBehavior(definition),
                "LoopBehavior" => CreateLoopBehavior(definition),
                "RetryBehavior" => CreateRetryBehavior(definition),

                // === Environment ===
                "LoadSceneBehavior" => new LoadSceneBehavior(
                    definition.GetString("SceneName"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "UnloadSceneBehavior" => new UnloadSceneBehavior(
                    definition.GetString("SceneName"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "EnablePhysicsBehavior" => new EnablePhysicsBehavior(
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "DisablePhysicsBehavior" => new DisablePhysicsBehavior(
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "TriggerEventBehavior" => new TriggerEventBehavior(
                    _eventBus, definition.GetString("EventName"), definition.GetObject("Payload"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "SendMessageBehavior" => new SendMessageBehavior(
                    _sceneResolver, definition.GetString("TargetObject"),
                    definition.GetString("MethodName"), definition.GetObject("Argument"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),
                "RaycastBehavior" => new RaycastBehavior(
                    _sceneResolver, _stateStore, definition.GetString("OriginObject"),
                    definition.GetVector3("Direction"), definition.GetFloat("MaxDistance"),
                    definition.GetString("ResultStateKey"),
                    definition.GetBool("IsBlocking"), (ExecutionStages)definition.GetInt("ExecutionStages")),

                _ => throw new ArgumentException($"Unknown behavior type: {definition.TypeName}")
            };
        }

        // === Private helper methods ===

        private IBehavior CreateMoveObject(BehaviorDefinition def) =>
            new MoveObjectBehavior(
                _sceneResolver,
                def.GetString("TargetObject"),
                def.GetString("FinalPosition"),
                def.GetFloat("Duration"),
                def.GetAnimationCurve("AnimationCurve"),
                def.GetBool("IsBlocking"),
                (ExecutionStages)def.GetInt("ExecutionStages"));

        private IBehavior CreateExecuteChapter(BehaviorDefinition def) =>
            new ExecuteChapterBehavior(
                def.GetChapter("Chapter"),
                null); // Runner delegate set at runtime by FlowEngine

        private IBehavior CreateRotateObject(BehaviorDefinition def) =>
            new RotateObjectBehavior(
                _sceneResolver,
                def.GetString("TargetObject"),
                def.GetVector3("TargetRotation"),
                def.GetFloat("Duration"),
                def.GetAnimationCurve("AnimationCurve"),
                def.GetBool("UseLocalRotation"),
                def.GetBool("IsBlocking"),
                (ExecutionStages)def.GetInt("ExecutionStages"));

        private IBehavior CreateScaleObject(BehaviorDefinition def) =>
            new ScaleObjectBehavior(
                _sceneResolver,
                def.GetString("TargetObject"),
                def.GetVector3("TargetScale"),
                def.GetFloat("Duration"),
                def.GetAnimationCurve("AnimationCurve"),
                def.GetBool("IsBlocking"),
                (ExecutionStages)def.GetInt("ExecutionStages"));

        private IBehavior CreateSetTransform(BehaviorDefinition def)
        {
            Vector3? pos = def.GetObject("Position") != null ? def.GetVector3("Position") : (Vector3?)null;
            Quaternion? rot = def.GetObject("Rotation") != null ? def.GetQuaternion("Rotation") : (Quaternion?)null;
            Vector3? scale = def.GetObject("Scale") != null ? def.GetVector3("Scale") : (Vector3?)null;

            return new SetTransformBehavior(
                _sceneResolver, def.GetString("TargetObject"),
                pos, rot, scale,
                def.GetBool("UseLocal"),
                def.GetBool("IsBlocking"),
                (ExecutionStages)def.GetInt("ExecutionStages"));
        }

        private IBehavior CreateTeleportObject(BehaviorDefinition def) =>
            new TeleportObjectBehavior(
                _sceneResolver,
                def.GetString("TargetObject"),
                def.GetString("DestinationObject"),
                def.GetBool("IsBlocking"),
                (ExecutionStages)def.GetInt("ExecutionStages"));

        private IBehavior CreateWaitUntilCondition(BehaviorDefinition def)
        {
            ICondition condition = null;
            if (def.GetObject("Condition") is ConditionDefinition condDef && _conditionFactory != null)
                condition = _conditionFactory.Create(condDef);

            return new WaitUntilConditionBehavior(
                condition,
                def.GetFloat("Timeout"),
                def.GetBool("IsBlocking"),
                (ExecutionStages)def.GetInt("ExecutionStages"));
        }

        private IBehavior CreateBranch(BehaviorDefinition def)
        {
            IBehavior trueBranch = null;
            IBehavior falseBranch = null;

            if (def.GetObject("TrueBranch") is BehaviorDefinition trueDef)
                trueBranch = Create(trueDef);
            if (def.GetObject("FalseBranch") is BehaviorDefinition falseDef)
                falseBranch = Create(falseDef);

            return new BranchBehavior(
                _stateStore, def.GetString("ConditionKey"),
                trueBranch, falseBranch,
                def.GetBool("IsBlocking"),
                (ExecutionStages)def.GetInt("ExecutionStages"));
        }

        private IBehavior CreateBehaviorSequence(BehaviorDefinition def)
        {
            var childDefs = def.GetBehaviorDefinitionList("Children");
            var children = childDefs.Select(d => Create(d)).ToList();
            return new BehaviorSequence(children,
                def.GetBool("IsBlocking"),
                (ExecutionStages)def.GetInt("ExecutionStages"));
        }

        private IBehavior CreateParallelBehavior(BehaviorDefinition def)
        {
            var childDefs = def.GetBehaviorDefinitionList("Children");
            var children = childDefs.Select(d => Create(d)).ToList();
            return new ParallelBehavior(children,
                def.GetBool("WaitForAll"),
                def.GetBool("IsBlocking"),
                (ExecutionStages)def.GetInt("ExecutionStages"));
        }

        private IBehavior CreateConditionalBehavior(BehaviorDefinition def)
        {
            IBehavior child = null;
            if (def.GetObject("Child") is BehaviorDefinition childDef)
                child = Create(childDef);

            return new ConditionalBehavior(
                _stateStore, def.GetString("ConditionKey"), child,
                def.GetBool("IsBlocking"),
                (ExecutionStages)def.GetInt("ExecutionStages"));
        }

        private IBehavior CreateRepeatBehavior(BehaviorDefinition def)
        {
            IBehavior child = null;
            if (def.GetObject("Child") is BehaviorDefinition childDef)
                child = Create(childDef);

            return new RepeatBehavior(child,
                def.GetInt("Count"),
                def.GetBool("IsBlocking"),
                (ExecutionStages)def.GetInt("ExecutionStages"));
        }

        private IBehavior CreateLoopBehavior(BehaviorDefinition def)
        {
            IBehavior child = null;
            if (def.GetObject("Child") is BehaviorDefinition childDef)
                child = Create(childDef);

            return new LoopBehavior(child,
                _stateStore, def.GetString("ConditionKey"),
                def.GetInt("MaxIterations"),
                def.GetBool("IsBlocking"),
                (ExecutionStages)def.GetInt("ExecutionStages"));
        }

        private IBehavior CreateRetryBehavior(BehaviorDefinition def)
        {
            IBehavior child = null;
            if (def.GetObject("Child") is BehaviorDefinition childDef)
                child = Create(childDef);

            return new RetryBehavior(child,
                def.GetInt("MaxRetries"),
                def.GetFloat("RetryDelay"),
                def.GetBool("IsBlocking"),
                (ExecutionStages)def.GetInt("ExecutionStages"));
        }
    }
}
