using System;
using System.Collections.Generic;
using NUnit.Framework;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Behaviors;
using ReactiveFlowEngine.Model;
using ReactiveFlowEngine.Tests.TestDoubles;
using UnityEngine;

namespace ReactiveFlowEngine.Tests
{
    [TestFixture]
    public class BehaviorFactoryTests
    {
        private BehaviorFactory _factory;
        private MockSceneObjectResolver _resolver;
        private MockStateStore _stateStore;
        private MockStepRunner _stepRunner;
        private MockEventBus _eventBus;
        private MockNavigationService _navigationService;
        private MockFlowEngine _flowEngine;

        [SetUp]
        public void SetUp()
        {
            _resolver = new MockSceneObjectResolver();
            _stateStore = new MockStateStore();
            _stepRunner = new MockStepRunner();
            _eventBus = new MockEventBus();
            _navigationService = new MockNavigationService();
            _flowEngine = new MockFlowEngine();

            // Create a minimal condition factory for behaviors that need it
            var condFactory = new Conditions.ConditionFactory(_resolver, _eventBus, _stateStore, _flowEngine);

            _factory = new BehaviorFactory(
                _resolver, _stateStore, _navigationService, _flowEngine, _stepRunner, _eventBus, condFactory);
        }

        [Test]
        public void Create_NullDefinition_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _factory.Create(null));
        }

        [Test]
        public void Create_UnknownType_ThrowsArgumentException()
        {
            var def = new BehaviorDefinition { TypeName = "NonExistentBehavior" };
            Assert.Throws<ArgumentException>(() => _factory.Create(def));
        }

        [Test]
        public void Create_DelayBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "DelayBehavior" };
            def.Parameters["Duration"] = 2.5f;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<DelayBehavior>(behavior);
        }

        [Test]
        public void Create_EnableObjectBehavior_ReturnsSetActiveBehavior()
        {
            var def = new BehaviorDefinition { TypeName = "EnableObjectBehavior" };
            def.Parameters["TargetObject"] = "guid-123";
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<SetActiveBehavior>(behavior);
        }

        [Test]
        public void Create_DisableObjectBehavior_ReturnsSetActiveBehavior()
        {
            var def = new BehaviorDefinition { TypeName = "DisableObjectBehavior" };
            def.Parameters["TargetObject"] = "guid-123";
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<SetActiveBehavior>(behavior);
        }

        [Test]
        public void Create_ShowObjectBehavior_ReturnsSetRendererVisibilityBehavior()
        {
            var def = new BehaviorDefinition { TypeName = "ShowObjectBehavior" };
            def.Parameters["TargetObject"] = "guid-123";
            def.Parameters["IncludeChildren"] = true;
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<SetRendererVisibilityBehavior>(behavior);
        }

        [Test]
        public void Create_HideObjectBehavior_ReturnsSetRendererVisibilityBehavior()
        {
            var def = new BehaviorDefinition { TypeName = "HideObjectBehavior" };
            def.Parameters["TargetObject"] = "guid-123";
            def.Parameters["IncludeChildren"] = true;
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<SetRendererVisibilityBehavior>(behavior);
        }

        [Test]
        public void Create_SetStateBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "SetStateBehavior" };
            def.Parameters["Key"] = "testKey";
            def.Parameters["Value"] = "testValue";
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<SetStateBehavior>(behavior);
        }

        [Test]
        public void Create_TriggerEventBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "TriggerEventBehavior" };
            def.Parameters["EventName"] = "TestEvent";
            def.Parameters["IsBlocking"] = false;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<TriggerEventBehavior>(behavior);
        }

        [Test]
        public void Create_LoadSceneBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "LoadSceneBehavior" };
            def.Parameters["SceneName"] = "TestScene";
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<LoadSceneBehavior>(behavior);
        }

        // === Object Control ===

        [Test]
        public void Create_MoveObjectBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "MoveObjectBehavior" };
            def.Parameters["TargetObject"] = "guid-123";
            def.Parameters["FinalPosition"] = "guid-456";
            def.Parameters["Duration"] = 1.0f;
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<MoveObjectBehavior>(behavior);
        }

        [Test]
        public void Create_RotateObjectBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "RotateObjectBehavior" };
            def.Parameters["TargetObject"] = "guid-123";
            def.Parameters["TargetRotation"] = new Vector3(0f, 90f, 0f);
            def.Parameters["Duration"] = 1.0f;
            def.Parameters["UseLocalRotation"] = false;
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<RotateObjectBehavior>(behavior);
        }

        [Test]
        public void Create_ScaleObjectBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "ScaleObjectBehavior" };
            def.Parameters["TargetObject"] = "guid-123";
            def.Parameters["TargetScale"] = new Vector3(2f, 2f, 2f);
            def.Parameters["Duration"] = 1.0f;
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<ScaleObjectBehavior>(behavior);
        }

        [Test]
        public void Create_SetTransformBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "SetTransformBehavior" };
            def.Parameters["TargetObject"] = "guid-123";
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<SetTransformBehavior>(behavior);
        }

        [Test]
        public void Create_TeleportObjectBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "TeleportObjectBehavior" };
            def.Parameters["TargetObject"] = "guid-123";
            def.Parameters["DestinationObject"] = "guid-456";
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<TeleportObjectBehavior>(behavior);
        }

        [Test]
        public void Create_DestroyObjectBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "DestroyObjectBehavior" };
            def.Parameters["TargetObject"] = "guid-123";
            def.Parameters["Delay"] = 0f;
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<DestroyObjectBehavior>(behavior);
        }

        [Test]
        public void Create_SpawnObjectBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "SpawnObjectBehavior" };
            def.Parameters["PrefabObject"] = "guid-123";
            def.Parameters["Position"] = new Vector3(0f, 0f, 0f);
            def.Parameters["Rotation"] = Quaternion.identity;
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<SpawnObjectBehavior>(behavior);
        }

        [Test]
        public void Create_AttachObjectBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "AttachObjectBehavior" };
            def.Parameters["ChildObject"] = "guid-123";
            def.Parameters["ParentObject"] = "guid-456";
            def.Parameters["WorldPositionStays"] = true;
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<AttachObjectBehavior>(behavior);
        }

        [Test]
        public void Create_DetachObjectBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "DetachObjectBehavior" };
            def.Parameters["ChildObject"] = "guid-123";
            def.Parameters["WorldPositionStays"] = true;
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<DetachObjectBehavior>(behavior);
        }

        [Test]
        public void Create_SetParentBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "SetParentBehavior" };
            def.Parameters["ChildObject"] = "guid-123";
            def.Parameters["ParentObject"] = "guid-456";
            def.Parameters["WorldPositionStays"] = true;
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<SetParentBehavior>(behavior);
        }

        // === Visual ===

        [Test]
        public void Create_HighlightObjectBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "HighlightObjectBehavior" };
            def.Parameters["TargetObject"] = "guid-123";
            def.Parameters["HighlightColor"] = Color.yellow;
            def.Parameters["Intensity"] = 1.5f;
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<HighlightObjectBehavior>(behavior);
        }

        [Test]
        public void Create_UnhighlightObjectBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "UnhighlightObjectBehavior" };
            def.Parameters["TargetObject"] = "guid-123";
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<UnhighlightObjectBehavior>(behavior);
        }

        [Test]
        public void Create_ChangeMaterialBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "ChangeMaterialBehavior" };
            def.Parameters["TargetObject"] = "guid-123";
            def.Parameters["MaterialPath"] = "Materials/TestMat";
            def.Parameters["MaterialIndex"] = 0;
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<ChangeMaterialBehavior>(behavior);
        }

        [Test]
        public void Create_FadeObjectBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "FadeObjectBehavior" };
            def.Parameters["TargetObject"] = "guid-123";
            def.Parameters["TargetAlpha"] = 0.5f;
            def.Parameters["Duration"] = 1.0f;
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<FadeObjectBehavior>(behavior);
        }

        [Test]
        public void Create_PlayAnimationBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "PlayAnimationBehavior" };
            def.Parameters["TargetObject"] = "guid-123";
            def.Parameters["StateName"] = "Idle";
            def.Parameters["Layer"] = 0;
            def.Parameters["WaitForCompletion"] = false;
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<PlayAnimationBehavior>(behavior);
        }

        [Test]
        public void Create_StopAnimationBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "StopAnimationBehavior" };
            def.Parameters["TargetObject"] = "guid-123";
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<StopAnimationBehavior>(behavior);
        }

        [Test]
        public void Create_SetAnimationStateBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "SetAnimationStateBehavior" };
            def.Parameters["TargetObject"] = "guid-123";
            def.Parameters["ParameterName"] = "Speed";
            def.Parameters["ParameterValue"] = 1.0f;
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<SetAnimationStateBehavior>(behavior);
        }

        [Test]
        public void Create_TriggerParticleEffectBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "TriggerParticleEffectBehavior" };
            def.Parameters["TargetObject"] = "guid-123";
            def.Parameters["Play"] = true;
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<TriggerParticleEffectBehavior>(behavior);
        }

        // === Audio ===

        [Test]
        public void Create_PlayAudioBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "PlayAudioBehavior" };
            def.Parameters["AudioSource"] = "guid-123";
            def.Parameters["ClipPath"] = "Audio/clip.wav";
            def.Parameters["WaitForCompletion"] = false;
            def.Parameters["Volume"] = 1.0f;
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<PlayAudioBehavior>(behavior);
        }

        [Test]
        public void Create_StopAudioBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "StopAudioBehavior" };
            def.Parameters["AudioSource"] = "guid-123";
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<StopAudioBehavior>(behavior);
        }

        [Test]
        public void Create_PauseAudioBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "PauseAudioBehavior" };
            def.Parameters["AudioSource"] = "guid-123";
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<PauseAudioBehavior>(behavior);
        }

        [Test]
        public void Create_ResumeAudioBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "ResumeAudioBehavior" };
            def.Parameters["AudioSource"] = "guid-123";
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<ResumeAudioBehavior>(behavior);
        }

        [Test]
        public void Create_LoopAudioBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "LoopAudioBehavior" };
            def.Parameters["AudioSource"] = "guid-123";
            def.Parameters["ClipPath"] = "Audio/loop.wav";
            def.Parameters["Volume"] = 0.8f;
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<LoopAudioBehavior>(behavior);
        }

        [Test]
        public void Create_PlaySpatialAudioBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "PlaySpatialAudioBehavior" };
            def.Parameters["AudioSource"] = "guid-123";
            def.Parameters["ClipPath"] = "Audio/spatial.wav";
            def.Parameters["PositionObject"] = "guid-456";
            def.Parameters["Volume"] = 1.0f;
            def.Parameters["WaitForCompletion"] = false;
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<PlaySpatialAudioBehavior>(behavior);
        }

        [Test]
        public void Create_TextToSpeechBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "TextToSpeechBehavior" };
            def.Parameters["Text"] = "Hello world";
            def.Parameters["Language"] = "en-US";
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<TextToSpeechBehavior>(behavior);
        }

        // === Flow/Timing ===

        [Test]
        public void Create_WaitUntilConditionBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "WaitUntilConditionBehavior" };
            def.Parameters["Timeout"] = 10.0f;
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<WaitUntilConditionBehavior>(behavior);
        }

        [Test]
        public void Create_TimeoutBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "TimeoutBehavior" };
            def.Parameters["Duration"] = 5.0f;
            def.Parameters["WarningMessage"] = "Timed out!";
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<TimeoutBehavior>(behavior);
        }

        [Test]
        public void Create_StartTimerBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "StartTimerBehavior" };
            def.Parameters["TimerName"] = "timer1";
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<StartTimerBehavior>(behavior);
        }

        [Test]
        public void Create_StopTimerBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "StopTimerBehavior" };
            def.Parameters["TimerName"] = "timer1";
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<StopTimerBehavior>(behavior);
        }

        [Test]
        public void Create_ResetTimerBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "ResetTimerBehavior" };
            def.Parameters["TimerName"] = "timer1";
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<ResetTimerBehavior>(behavior);
        }

        // === State/Data ===

        [Test]
        public void Create_UpdateStateBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "UpdateStateBehavior" };
            def.Parameters["Key"] = "testKey";
            def.Parameters["Value"] = "newValue";
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<UpdateStateBehavior>(behavior);
        }

        [Test]
        public void Create_IncrementStateBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "IncrementStateBehavior" };
            def.Parameters["Key"] = "counter";
            def.Parameters["Amount"] = 1.0f;
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<IncrementStateBehavior>(behavior);
        }

        [Test]
        public void Create_DecrementStateBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "DecrementStateBehavior" };
            def.Parameters["Key"] = "counter";
            def.Parameters["Amount"] = 1.0f;
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<DecrementStateBehavior>(behavior);
        }

        [Test]
        public void Create_ToggleStateBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "ToggleStateBehavior" };
            def.Parameters["Key"] = "flag";
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<ToggleStateBehavior>(behavior);
        }

        [Test]
        public void Create_ClearStateBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "ClearStateBehavior" };
            def.Parameters["Key"] = "testKey";
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<ClearStateBehavior>(behavior);
        }

        [Test]
        public void Create_CopyStateBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "CopyStateBehavior" };
            def.Parameters["SourceKey"] = "srcKey";
            def.Parameters["DestinationKey"] = "dstKey";
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<CopyStateBehavior>(behavior);
        }

        [Test]
        public void Create_SaveStateBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "SaveStateBehavior" };
            def.Parameters["SnapshotKey"] = "snap1";
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<SaveStateBehavior>(behavior);
        }

        [Test]
        public void Create_LoadStateBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "LoadStateBehavior" };
            def.Parameters["SnapshotKey"] = "snap1";
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<LoadStateBehavior>(behavior);
        }

        // === Flow Control ===

        [Test]
        public void Create_ForceTransitionBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "ForceTransitionBehavior" };
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<ForceTransitionBehavior>(behavior);
        }

        [Test]
        public void Create_CancelTransitionBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "CancelTransitionBehavior" };
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<CancelTransitionBehavior>(behavior);
        }

        [Test]
        public void Create_RestartStepBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "RestartStepBehavior" };
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<RestartStepBehavior>(behavior);
        }

        [Test]
        public void Create_SkipStepBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "SkipStepBehavior" };
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<SkipStepBehavior>(behavior);
        }

        [Test]
        public void Create_EndProcessBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "EndProcessBehavior" };
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<EndProcessBehavior>(behavior);
        }

        [Test]
        public void Create_BranchBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "BranchBehavior" };
            def.Parameters["ConditionKey"] = "someFlag";
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<BranchBehavior>(behavior);
        }

        // === Sequence/Composition ===

        [Test]
        public void Create_BehaviorSequence_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "BehaviorSequence" };
            def.Parameters["Children"] = new List<BehaviorDefinition>();
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<BehaviorSequence>(behavior);
        }

        [Test]
        public void Create_ParallelBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "ParallelBehavior" };
            def.Parameters["Children"] = new List<BehaviorDefinition>();
            def.Parameters["WaitForAll"] = true;
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<ParallelBehavior>(behavior);
        }

        [Test]
        public void Create_ConditionalBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "ConditionalBehavior" };
            def.Parameters["ConditionKey"] = "someFlag";
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<ConditionalBehavior>(behavior);
        }

        [Test]
        public void Create_RepeatBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "RepeatBehavior" };
            def.Parameters["Count"] = 3;
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<RepeatBehavior>(behavior);
        }

        [Test]
        public void Create_LoopBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "LoopBehavior" };
            def.Parameters["ConditionKey"] = "loopFlag";
            def.Parameters["MaxIterations"] = 10;
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<LoopBehavior>(behavior);
        }

        [Test]
        public void Create_RetryBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "RetryBehavior" };
            def.Parameters["MaxRetries"] = 3;
            def.Parameters["RetryDelay"] = 1.0f;
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<RetryBehavior>(behavior);
        }

        // === Environment ===

        [Test]
        public void Create_UnloadSceneBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "UnloadSceneBehavior" };
            def.Parameters["SceneName"] = "TestScene";
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<UnloadSceneBehavior>(behavior);
        }

        [Test]
        public void Create_EnablePhysicsBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "EnablePhysicsBehavior" };
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<EnablePhysicsBehavior>(behavior);
        }

        [Test]
        public void Create_DisablePhysicsBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "DisablePhysicsBehavior" };
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<DisablePhysicsBehavior>(behavior);
        }

        [Test]
        public void Create_SendMessageBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "SendMessageBehavior" };
            def.Parameters["TargetObject"] = "guid-123";
            def.Parameters["MethodName"] = "DoSomething";
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<SendMessageBehavior>(behavior);
        }

        [Test]
        public void Create_RaycastBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "RaycastBehavior" };
            def.Parameters["OriginObject"] = "guid-123";
            def.Parameters["Direction"] = new Vector3(0f, 0f, 1f);
            def.Parameters["MaxDistance"] = 100f;
            def.Parameters["ResultStateKey"] = "rayResult";
            def.Parameters["IsBlocking"] = true;
            def.Parameters["ExecutionStages"] = 1;

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<RaycastBehavior>(behavior);
        }

        [Test]
        public void Create_ExecuteChapterBehavior_ReturnsInstance()
        {
            var def = new BehaviorDefinition { TypeName = "ExecuteChapterBehavior" };
            def.Parameters["ChapterGuid"] = "chapter-guid-1";

            var behavior = _factory.Create(def);
            Assert.IsNotNull(behavior);
            Assert.IsInstanceOf<ExecuteChapterBehavior>(behavior);
        }
    }
}
