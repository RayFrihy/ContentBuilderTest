using System;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using ReactiveFlowEngine.Serialization.JsonModels;

namespace ReactiveFlowEngine.Serialization
{
    public class RfeTypeBinder : ISerializationBinder
    {
        private readonly Dictionary<string, Type> _typeMap = new Dictionary<string, Type>
        {
            // === Structural types ===
            ["Serializer_ProcessWrapper"] = typeof(JsonProcessWrapper),
            ["Process"] = typeof(JsonProcess),
            ["ProcessData"] = typeof(JsonProcessData),
            ["ProcessMetadata"] = typeof(JsonProcessMetadata),
            ["Chapter"] = typeof(JsonChapter),
            ["ChapterData"] = typeof(JsonChapterData),
            ["ChapterMetadata"] = typeof(JsonChapterMetadata),
            ["Step"] = typeof(JsonStep),
            ["StepRef"] = typeof(JsonStepRef),
            ["StepData"] = typeof(JsonStepData),
            ["StepMetadata"] = typeof(JsonStepMetadata),
            ["Transition"] = typeof(JsonTransition),
            ["TransitionData"] = typeof(JsonTransitionData),
            ["TransitionCollection"] = typeof(JsonTransitionCollection),
            ["TransitionCollectionData"] = typeof(JsonTransitionCollectionData),
            ["BehaviorCollection"] = typeof(JsonBehaviorCollection),
            ["BehaviorCollectionData"] = typeof(JsonBehaviorCollectionData),
            ["SingleSceneObjectReference"] = typeof(JsonSceneObjRef),
            ["Metadata"] = typeof(JsonMetadata),
            ["MetadataValuesDictionary"] = typeof(JsonMetadataValuesDictionary),
            ["ConditionDictionary"] = typeof(JsonConditionDictionary),
            ["ListOfAttributeData"] = typeof(JsonListOfAttributeData),
            ["FoldableAttribute"] = typeof(JsonFoldableAttribute),
            ["HelpAttribute"] = typeof(JsonHelpAttribute),
            ["MenuAttribute"] = typeof(JsonMenuAttribute),
            ["ExtendableListAttribute_Wrapper"] = typeof(JsonExtendableListAttributeWrapper),
            ["ReorderableElementMetadata"] = typeof(JsonReorderableElementMetadata),
            ["ViewTransform"] = typeof(JsonViewTransform),

            // === Specific behavior/condition DTOs (keep these for exact mapping) ===
            ["TimeoutCondition"] = typeof(JsonTimeoutCondition),
            ["TimeoutConditionData"] = typeof(JsonTimeoutConditionData),
            ["MoveObjectBehavior"] = typeof(JsonMoveObjectBehavior),
            ["MoveObjectBehaviorData"] = typeof(JsonMoveObjectBehaviorData),
            ["ExecuteChapterBehavior"] = typeof(JsonExecuteChapterBehavior),
            ["ExecuteChapterBehaviorData"] = typeof(JsonExecuteChapterBehaviorData),

            // === Generic behavior types (all map to JsonGenericBehavior) ===
            ["DelayBehavior"] = typeof(JsonGenericBehavior),
            ["RotateObjectBehavior"] = typeof(JsonGenericBehavior),
            ["ScaleObjectBehavior"] = typeof(JsonGenericBehavior),
            ["SetTransformBehavior"] = typeof(JsonGenericBehavior),
            ["TeleportObjectBehavior"] = typeof(JsonGenericBehavior),
            ["EnableObjectBehavior"] = typeof(JsonGenericBehavior),
            ["DisableObjectBehavior"] = typeof(JsonGenericBehavior),
            ["DestroyObjectBehavior"] = typeof(JsonGenericBehavior),
            ["SpawnObjectBehavior"] = typeof(JsonGenericBehavior),
            ["AttachObjectBehavior"] = typeof(JsonGenericBehavior),
            ["DetachObjectBehavior"] = typeof(JsonGenericBehavior),
            ["SetParentBehavior"] = typeof(JsonGenericBehavior),
            ["HighlightObjectBehavior"] = typeof(JsonGenericBehavior),
            ["UnhighlightObjectBehavior"] = typeof(JsonGenericBehavior),
            ["ChangeMaterialBehavior"] = typeof(JsonGenericBehavior),
            ["FadeObjectBehavior"] = typeof(JsonGenericBehavior),
            ["ShowObjectBehavior"] = typeof(JsonGenericBehavior),
            ["HideObjectBehavior"] = typeof(JsonGenericBehavior),
            ["PlayAnimationBehavior"] = typeof(JsonGenericBehavior),
            ["StopAnimationBehavior"] = typeof(JsonGenericBehavior),
            ["SetAnimationStateBehavior"] = typeof(JsonGenericBehavior),
            ["TriggerParticleEffectBehavior"] = typeof(JsonGenericBehavior),
            ["PlayAudioBehavior"] = typeof(JsonGenericBehavior),
            ["StopAudioBehavior"] = typeof(JsonGenericBehavior),
            ["PauseAudioBehavior"] = typeof(JsonGenericBehavior),
            ["ResumeAudioBehavior"] = typeof(JsonGenericBehavior),
            ["LoopAudioBehavior"] = typeof(JsonGenericBehavior),
            ["PlaySpatialAudioBehavior"] = typeof(JsonGenericBehavior),
            ["TextToSpeechBehavior"] = typeof(JsonGenericBehavior),
            ["WaitUntilConditionBehavior"] = typeof(JsonGenericBehavior),
            ["TimeoutBehavior"] = typeof(JsonGenericBehavior),
            ["StartTimerBehavior"] = typeof(JsonGenericBehavior),
            ["StopTimerBehavior"] = typeof(JsonGenericBehavior),
            ["ResetTimerBehavior"] = typeof(JsonGenericBehavior),
            ["SetStateBehavior"] = typeof(JsonGenericBehavior),
            ["UpdateStateBehavior"] = typeof(JsonGenericBehavior),
            ["IncrementStateBehavior"] = typeof(JsonGenericBehavior),
            ["DecrementStateBehavior"] = typeof(JsonGenericBehavior),
            ["ToggleStateBehavior"] = typeof(JsonGenericBehavior),
            ["ClearStateBehavior"] = typeof(JsonGenericBehavior),
            ["CopyStateBehavior"] = typeof(JsonGenericBehavior),
            ["SaveStateBehavior"] = typeof(JsonGenericBehavior),
            ["LoadStateBehavior"] = typeof(JsonGenericBehavior),
            ["ForceTransitionBehavior"] = typeof(JsonGenericBehavior),
            ["CancelTransitionBehavior"] = typeof(JsonGenericBehavior),
            ["RestartStepBehavior"] = typeof(JsonGenericBehavior),
            ["SkipStepBehavior"] = typeof(JsonGenericBehavior),
            ["BranchBehavior"] = typeof(JsonGenericBehavior),
            ["EndProcessBehavior"] = typeof(JsonGenericBehavior),
            ["BehaviorSequence"] = typeof(JsonGenericBehavior),
            ["ParallelBehavior"] = typeof(JsonGenericBehavior),
            ["ConditionalBehavior"] = typeof(JsonGenericBehavior),
            ["RepeatBehavior"] = typeof(JsonGenericBehavior),
            ["LoopBehavior"] = typeof(JsonGenericBehavior),
            ["RetryBehavior"] = typeof(JsonGenericBehavior),
            ["LoadSceneBehavior"] = typeof(JsonGenericBehavior),
            ["UnloadSceneBehavior"] = typeof(JsonGenericBehavior),
            ["EnablePhysicsBehavior"] = typeof(JsonGenericBehavior),
            ["DisablePhysicsBehavior"] = typeof(JsonGenericBehavior),
            ["TriggerEventBehavior"] = typeof(JsonGenericBehavior),
            ["SendMessageBehavior"] = typeof(JsonGenericBehavior),
            ["RaycastBehavior"] = typeof(JsonGenericBehavior),

            // === Generic condition types (all map to JsonGenericCondition) ===
            ["CompositeAndCondition"] = typeof(JsonGenericCondition),
            ["CompositeOrCondition"] = typeof(JsonGenericCondition),
            ["CompositeNotCondition"] = typeof(JsonGenericCondition),
            ["CompositeCondition"] = typeof(JsonGenericCondition),
            ["XorCondition"] = typeof(JsonGenericCondition),
            ["WeightedCondition"] = typeof(JsonGenericCondition),
            ["PriorityCondition"] = typeof(JsonGenericCondition),
            ["ObjectGrabbedCondition"] = typeof(JsonGenericCondition),
            ["ObjectReleasedCondition"] = typeof(JsonGenericCondition),
            ["ObjectTouchedCondition"] = typeof(JsonGenericCondition),
            ["ObjectUsedCondition"] = typeof(JsonGenericCondition),
            ["ObjectHoveredCondition"] = typeof(JsonGenericCondition),
            ["ObjectSelectedCondition"] = typeof(JsonGenericCondition),
            ["ObjectDeselectedCondition"] = typeof(JsonGenericCondition),
            ["ButtonPressedCondition"] = typeof(JsonGenericCondition),
            ["ButtonReleasedCondition"] = typeof(JsonGenericCondition),
            ["InputActionTriggeredCondition"] = typeof(JsonGenericCondition),
            ["GesturePerformedCondition"] = typeof(JsonGenericCondition),
            ["ObjectInZoneCondition"] = typeof(JsonGenericCondition),
            ["ObjectExitedZoneCondition"] = typeof(JsonGenericCondition),
            ["ObjectNearCondition"] = typeof(JsonGenericCondition),
            ["ObjectFarCondition"] = typeof(JsonGenericCondition),
            ["ObjectAlignedCondition"] = typeof(JsonGenericCondition),
            ["ObjectRotationCondition"] = typeof(JsonGenericCondition),
            ["ObjectPositionCondition"] = typeof(JsonGenericCondition),
            ["ObjectFacingCondition"] = typeof(JsonGenericCondition),
            ["ObjectInsideBoundsCondition"] = typeof(JsonGenericCondition),
            ["ObjectOutsideBoundsCondition"] = typeof(JsonGenericCondition),
            ["DistanceThresholdCondition"] = typeof(JsonGenericCondition),
            ["DelayElapsedCondition"] = typeof(JsonGenericCondition),
            ["TimerRunningCondition"] = typeof(JsonGenericCondition),
            ["CooldownCompleteCondition"] = typeof(JsonGenericCondition),
            ["ElapsedTimeCondition"] = typeof(JsonGenericCondition),
            ["BooleanStateCondition"] = typeof(JsonGenericCondition),
            ["IntegerStateCondition"] = typeof(JsonGenericCondition),
            ["FloatStateCondition"] = typeof(JsonGenericCondition),
            ["StringStateCondition"] = typeof(JsonGenericCondition),
            ["StateEqualsCondition"] = typeof(JsonGenericCondition),
            ["StateChangedCondition"] = typeof(JsonGenericCondition),
            ["StateExistsCondition"] = typeof(JsonGenericCondition),
            ["StateNotExistsCondition"] = typeof(JsonGenericCondition),
            ["VariableComparisonCondition"] = typeof(JsonGenericCondition),
            ["ObjectPropertyCondition"] = typeof(JsonGenericCondition),
            ["FlagSetCondition"] = typeof(JsonGenericCondition),
            ["StepCompletedCondition"] = typeof(JsonGenericCondition),
            ["StepActiveCondition"] = typeof(JsonGenericCondition),
            ["PreviousStepCondition"] = typeof(JsonGenericCondition),
            ["NextStepAvailableCondition"] = typeof(JsonGenericCondition),
            ["TransitionAvailableCondition"] = typeof(JsonGenericCondition),
            ["ProcessStartedCondition"] = typeof(JsonGenericCondition),
            ["ProcessCompletedCondition"] = typeof(JsonGenericCondition),
            ["SceneLoadedCondition"] = typeof(JsonGenericCondition),
            ["ObjectExistsCondition"] = typeof(JsonGenericCondition),
            ["ObjectDestroyedCondition"] = typeof(JsonGenericCondition),
            ["PhysicsCollisionCondition"] = typeof(JsonGenericCondition),
            ["TriggerEnterCondition"] = typeof(JsonGenericCondition),
            ["TriggerExitCondition"] = typeof(JsonGenericCondition),
            ["RaycastHitCondition"] = typeof(JsonGenericCondition),
            ["LayerMaskCondition"] = typeof(JsonGenericCondition),

            // === List types ===
            ["StepList"] = typeof(List<object>),
            ["ChapterList"] = typeof(List<object>),
            ["TransitionList"] = typeof(List<object>),
            ["BehaviorList"] = typeof(List<object>),
            ["ConditionList"] = typeof(List<object>),
            ["GuidList"] = typeof(List<object>),
            ["LockablePropertyRefList"] = typeof(List<object>),
            ["ChildAttributes"] = typeof(List<object>),
            ["ChildMetadata"] = typeof(List<object>),
            ["GuidDictionary"] = typeof(Dictionary<string, object>),
            ["StepRefList"] = typeof(List<object>),
        };

        // Store type name → string name mapping for generic types
        // so we can recover the original type name after deserialization
        private static readonly HashSet<string> _behaviorTypeNames = new HashSet<string>
        {
            "DelayBehavior", "RotateObjectBehavior", "ScaleObjectBehavior", "SetTransformBehavior",
            "TeleportObjectBehavior", "EnableObjectBehavior", "DisableObjectBehavior", "DestroyObjectBehavior",
            "SpawnObjectBehavior", "AttachObjectBehavior", "DetachObjectBehavior", "SetParentBehavior",
            "HighlightObjectBehavior", "UnhighlightObjectBehavior", "ChangeMaterialBehavior", "FadeObjectBehavior",
            "ShowObjectBehavior", "HideObjectBehavior", "PlayAnimationBehavior", "StopAnimationBehavior",
            "SetAnimationStateBehavior", "TriggerParticleEffectBehavior", "PlayAudioBehavior", "StopAudioBehavior",
            "PauseAudioBehavior", "ResumeAudioBehavior", "LoopAudioBehavior", "PlaySpatialAudioBehavior",
            "TextToSpeechBehavior", "WaitUntilConditionBehavior", "TimeoutBehavior", "StartTimerBehavior",
            "StopTimerBehavior", "ResetTimerBehavior", "SetStateBehavior", "UpdateStateBehavior",
            "IncrementStateBehavior", "DecrementStateBehavior", "ToggleStateBehavior", "ClearStateBehavior",
            "CopyStateBehavior", "SaveStateBehavior", "LoadStateBehavior", "ForceTransitionBehavior",
            "CancelTransitionBehavior", "RestartStepBehavior", "SkipStepBehavior", "BranchBehavior",
            "EndProcessBehavior", "BehaviorSequence", "ParallelBehavior", "ConditionalBehavior",
            "RepeatBehavior", "LoopBehavior", "RetryBehavior", "LoadSceneBehavior", "UnloadSceneBehavior",
            "EnablePhysicsBehavior", "DisablePhysicsBehavior", "TriggerEventBehavior", "SendMessageBehavior",
            "RaycastBehavior"
        };

        public static bool IsBehaviorTypeName(string name) => _behaviorTypeNames.Contains(name);

        private readonly Dictionary<Type, string> _reverseMap;

        public RfeTypeBinder()
        {
            _reverseMap = new Dictionary<Type, string>();
            foreach (var kvp in _typeMap)
            {
                // Only add first mapping for each type (avoid overwriting with generic types)
                if (!_reverseMap.ContainsKey(kvp.Value))
                    _reverseMap[kvp.Value] = kvp.Key;
            }
        }

        public Type BindToType(string assemblyName, string typeName)
        {
            var cleanName = typeName.Trim().TrimEnd(',').Trim();
            if (_typeMap.TryGetValue(cleanName, out var type))
                return type;

            // Fallback: return null to let Newtonsoft handle it (will use object)
            return null;
        }

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            if (_reverseMap.TryGetValue(serializedType, out var name))
                typeName = name;
            else
                typeName = serializedType.Name;
        }

        /// <summary>
        /// Tries to extract the original type name from a deserialized generic object.
        /// For JsonGenericBehavior/JsonGenericCondition, the TypeDiscriminator may be null
        /// because Newtonsoft consumed the $type. This method provides the mapping from
        /// the binder's perspective.
        /// </summary>
        public string GetOriginalTypeName(string cleanName)
        {
            return _typeMap.ContainsKey(cleanName) ? cleanName : null;
        }
    }
}
