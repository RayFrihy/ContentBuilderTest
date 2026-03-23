using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using ReactiveFlowEngine.Serialization.JsonModels;
using UnityEngine;

namespace ReactiveFlowEngine.Serialization
{
    public class ModelBuilder
    {
        private readonly Conditions.ConditionFactory _conditionFactory;
        private readonly Behaviors.BehaviorFactory _behaviorFactory;

        // ConditionFactory and BehaviorFactory are in the DI layer but we reference them by their concrete types
        // injected via VContainer
        public ModelBuilder()
        {
            _conditionFactory = null;
            _behaviorFactory = null;
        }

        public ModelBuilder(Conditions.ConditionFactory conditionFactory, Behaviors.BehaviorFactory behaviorFactory)
        {
            _conditionFactory = conditionFactory;
            _behaviorFactory = behaviorFactory;
        }

        public ProcessModel Build(JsonProcessWrapper wrapper)
        {
            if (wrapper?.Process == null)
                throw new InvalidOperationException("JSON process wrapper or Process is null");

            // Step 1: Build master step dictionary from flat steps list
            var stepLookup = new Dictionary<string, StepModel>();
            if (wrapper.Steps != null)
            {
                foreach (var jsonStepObj in wrapper.Steps)
                {
                    var jsonStep = jsonStepObj as JsonStep;
                    if (jsonStep?.StepMetadata?.Guid == null) continue;

                    var stepModel = BuildStep(jsonStep);
                    stepLookup[jsonStep.StepMetadata.Guid] = stepModel;
                }
            }

            // Step 2: Build sub-chapters (from wrapper.SubChapters)
            var subChapterLookup = new Dictionary<string, ChapterModel>();
            if (wrapper.SubChapters != null)
            {
                foreach (var jsonChapterObj in wrapper.SubChapters)
                {
                    var jsonChapter = jsonChapterObj as JsonChapter;
                    if (jsonChapter == null) continue;

                    var chapterModel = BuildChapter(jsonChapter, stepLookup);
                    if (chapterModel != null && chapterModel.Id != null)
                        subChapterLookup[chapterModel.Id] = chapterModel;
                }
            }

            // Step 3: Build main chapters
            var processData = wrapper.Process.Data;
            var process = new ProcessModel
            {
                Id = wrapper.Process.ProcessMetadata?.Guid ?? System.Guid.NewGuid().ToString(),
                Name = processData?.Name ?? "Unnamed Process"
            };

            if (processData?.Chapters != null)
            {
                foreach (var jsonChapterObj in processData.Chapters)
                {
                    var jsonChapter = jsonChapterObj as JsonChapter;
                    if (jsonChapter == null) continue;

                    var chapterModel = BuildChapter(jsonChapter, stepLookup);
                    if (chapterModel != null)
                        process.ChapterModels.Add(chapterModel);
                }
            }

            // Resolve FirstChapter
            if (processData?.FirstChapter != null)
            {
                var firstChapterGuid = (processData.FirstChapter as JsonChapter)?.ChapterMetadata?.Guid;
                if (firstChapterGuid != null)
                    process.FirstChapterModel = process.ChapterModels.Find(c => c.Id == firstChapterGuid);
            }
            if (process.FirstChapterModel == null && process.ChapterModels.Count > 0)
                process.FirstChapterModel = process.ChapterModels[0];

            // Step 4: Link transitions TargetStep references
            foreach (var step in stepLookup.Values)
            {
                foreach (var transition in step.TransitionModels)
                {
                    // TargetStepModel was set with a temporary StepModel holding only the GUID
                    // Now resolve to the real step
                    if (transition.TargetStepModel != null && !string.IsNullOrEmpty(transition.TargetStepModel.Id))
                    {
                        if (stepLookup.TryGetValue(transition.TargetStepModel.Id, out var resolvedStep))
                            transition.TargetStepModel = resolvedStep;
                    }
                }
            }

            // Step 5: Link step groups to their sub-chapters and update ExecuteChapterBehavior instances
            foreach (var step in stepLookup.Values)
            {
                if (step.Type != StepType.StepGroup)
                    continue;

                // Find the matching sub-chapter for each ExecuteChapterBehavior using its stored GUID
                ChapterModel linkedSubChapter = null;
                foreach (var behavior in step.BehaviorList)
                {
                    if (behavior is Behaviors.ExecuteChapterBehavior execBehavior)
                    {
                        var guid = execBehavior.SubChapterGuid;
                        if (guid != null && subChapterLookup.TryGetValue(guid, out var matched))
                        {
                            linkedSubChapter = matched;
                        }
                    }
                }

                step.SubChapterModel = linkedSubChapter;

                // Now rebuild the ExecuteChapterBehavior with the resolved sub-chapter
                if (linkedSubChapter != null)
                {
                    for (int i = 0; i < step.BehaviorList.Count; i++)
                    {
                        if (step.BehaviorList[i] is Behaviors.ExecuteChapterBehavior oldExec)
                        {
                            step.BehaviorList[i] = new Behaviors.ExecuteChapterBehavior(
                                linkedSubChapter, null, oldExec.SubChapterGuid);
                        }
                    }
                }
            }

            return process;
        }

        private ChapterModel BuildChapter(JsonChapter jsonChapter, Dictionary<string, StepModel> stepLookup)
        {
            if (jsonChapter == null) return null;

            var chapter = new ChapterModel
            {
                Id = jsonChapter.ChapterMetadata?.Guid ?? System.Guid.NewGuid().ToString(),
                Name = jsonChapter.Data?.Name ?? "Unnamed Chapter"
            };

            // Resolve steps from chapter's step list (items can be JsonStep or JsonStepRef via $ref)
            if (jsonChapter.Data?.Steps != null)
            {
                foreach (var stepObj in jsonChapter.Data.Steps)
                {
                    var guid = ExtractStepGuid(stepObj);

                    if (guid != null && stepLookup.TryGetValue(guid, out var stepModel))
                        chapter.StepModels.Add(stepModel);
                }
            }

            // Resolve FirstStep (can be JsonStep or JsonStepRef via $ref)
            if (jsonChapter.Data?.FirstStep != null)
            {
                var firstStepGuid = ExtractStepGuid(jsonChapter.Data.FirstStep);
                if (firstStepGuid != null && stepLookup.TryGetValue(firstStepGuid, out var firstStep))
                    chapter.FirstStepModel = firstStep;
            }
            if (chapter.FirstStepModel == null && chapter.StepModels.Count > 0)
                chapter.FirstStepModel = chapter.StepModels[0];

            return chapter;
        }

        private StepModel BuildStep(JsonStep jsonStep)
        {
            var step = new StepModel
            {
                Id = jsonStep.StepMetadata.Guid,
                Name = jsonStep.Data?.Name ?? "Unnamed Step",
                Type = ParseStepType(jsonStep.StepMetadata.StepType)
            };

            // Build behaviors
            if (jsonStep.Data?.Behaviors?.Data?.Behaviors != null)
            {
                foreach (var behaviorObj in jsonStep.Data.Behaviors.Data.Behaviors)
                {
                    var behaviorDef = BuildBehaviorDefinition(behaviorObj);
                    if (behaviorDef != null)
                    {
                        IBehavior behavior = null;
                        if (_behaviorFactory != null)
                            behavior = _behaviorFactory.Create(behaviorDef);

                        if (behavior != null)
                            step.BehaviorList.Add(behavior);
                    }
                }
            }

            // Build transitions
            if (jsonStep.Data?.Transitions?.Data?.Transitions != null)
            {
                int priority = 0;
                foreach (var transObj in jsonStep.Data.Transitions.Data.Transitions)
                {
                    var jsonTransition = transObj as JsonTransition;
                    if (jsonTransition == null) continue;

                    var transition = BuildTransition(jsonTransition, priority);
                    step.TransitionModels.Add(transition);
                    priority++;
                }
            }

            return step;
        }

        private TransitionModel BuildTransition(JsonTransition jsonTransition, int priority)
        {
            var transition = new TransitionModel { Priority = priority };

            // Build conditions
            if (jsonTransition.Data?.Conditions != null)
            {
                foreach (var condObj in jsonTransition.Data.Conditions)
                {
                    var condDef = BuildConditionDefinition(condObj);
                    if (condDef != null)
                    {
                        ICondition condition = null;
                        if (_conditionFactory != null)
                            condition = _conditionFactory.Create(condDef);

                        if (condition != null)
                            transition.ConditionList.Add(condition);
                    }
                }
            }

            // Resolve TargetStep: store a temporary StepModel with just the GUID, linked in Build()
            if (jsonTransition.Data?.TargetStep?.StepMetadata?.Guid != null)
            {
                transition.TargetStepModel = new StepModel
                {
                    Id = jsonTransition.Data.TargetStep.StepMetadata.Guid
                };
            }

            return transition;
        }

        private BehaviorDefinition BuildBehaviorDefinition(object behaviorObj)
        {
            if (behaviorObj is JsonMoveObjectBehavior moveBehavior && moveBehavior.Data != null)
            {
                var def = new BehaviorDefinition { TypeName = "MoveObjectBehavior" };
                var data = moveBehavior.Data;

                // Extract target object GUID
                if (data.TargetObject?.Guids != null && data.TargetObject.Guids.Count > 0)
                    def.Parameters["TargetObject"] = data.TargetObject.Guids[0]?.ToString();

                // Extract final position GUID
                if (data.FinalPosition?.Guids != null && data.FinalPosition.Guids.Count > 0)
                    def.Parameters["FinalPosition"] = data.FinalPosition.Guids[0]?.ToString();

                def.Parameters["Duration"] = data.Duration;
                def.Parameters["IsBlocking"] = data.IsBlocking;
                def.Parameters["ExecutionStages"] = data.ExecutionStages;

                // Build AnimationCurve
                if (data.AnimationCurve != null)
                {
                    var curveData = new AnimationCurveData
                    {
                        PreWrapMode = (UnityEngine.WrapMode)data.AnimationCurve.PreWrapMode,
                        PostWrapMode = (UnityEngine.WrapMode)data.AnimationCurve.PostWrapMode
                    };
                    if (data.AnimationCurve.Keys != null)
                    {
                        foreach (var k in data.AnimationCurve.Keys)
                        {
                            curveData.Keys.Add(new KeyframeData
                            {
                                Time = k.Time,
                                Value = k.Value,
                                InTangent = k.InTangent,
                                OutTangent = k.OutTangent,
                                InWeight = k.InWeight,
                                OutWeight = k.OutWeight,
                                WeightedMode = k.WeightedMode
                            });
                        }
                    }
                    def.Parameters["AnimationCurve"] = curveData;
                }

                return def;
            }
            else if (behaviorObj is JsonExecuteChapterBehavior execChapter && execChapter.Data != null)
            {
                var def = new BehaviorDefinition { TypeName = "ExecuteChapterBehavior" };
                // The chapter reference will be resolved later during model linking
                // Store the JsonChapter reference for now
                if (execChapter.Data.Chapter != null)
                {
                    var chapterGuid = execChapter.Data.Chapter.ChapterMetadata?.Guid;
                    def.Parameters["ChapterGuid"] = chapterGuid;
                }
                return def;
            }
            else if (behaviorObj is JsonGenericBehavior generic && generic.Data != null)
            {
                // Generic fallback for all other behavior types
                // TypeDiscriminator comes from $type (may be consumed by Newtonsoft),
                // TypeName comes from __type (VR Builder format)
                var typeName = !string.IsNullOrEmpty(generic.TypeDiscriminator)
                    ? generic.TypeDiscriminator
                    : generic.TypeName;
                if (string.IsNullOrEmpty(typeName))
                {
                    Debug.LogWarning("[RFE] GenericBehavior has no TypeDiscriminator or __type, skipping.");
                    return null;
                }

                var def = new BehaviorDefinition { TypeName = typeName };
                foreach (var kvp in generic.Data)
                {
                    def.Parameters[kvp.Key] = ResolveJsonValue(kvp.Value);
                }
                return def;
            }

            return null;
        }

        private ConditionDefinition BuildConditionDefinition(object condObj)
        {
            if (condObj is JsonTimeoutCondition timeout && timeout.Data != null)
            {
                return new ConditionDefinition
                {
                    TypeName = "TimeoutCondition",
                    Parameters = new Dictionary<string, object>
                    {
                        ["Timeout"] = timeout.Data.Timeout
                    }
                };
            }
            else if (condObj is JsonGenericCondition generic && generic.Data != null)
            {
                // Generic fallback for all other condition types
                var typeName = !string.IsNullOrEmpty(generic.TypeDiscriminator)
                    ? generic.TypeDiscriminator
                    : generic.TypeName;
                if (string.IsNullOrEmpty(typeName))
                {
                    Debug.LogWarning("[RFE] GenericCondition has no TypeDiscriminator or __type, skipping.");
                    return null;
                }

                var def = new ConditionDefinition { TypeName = typeName };
                foreach (var kvp in generic.Data)
                {
                    def.Parameters[kvp.Key] = ResolveJsonValue(kvp.Value);
                }
                return def;
            }

            return null;
        }

        /// <summary>
        /// Recursively resolves Newtonsoft JObject/JArray types into plain .NET types,
        /// and extracts scene object GUIDs from nested { "guids": [...] } structures.
        /// </summary>
        private static object ResolveJsonValue(object value)
        {
            if (value is JObject jObj)
            {
                // Check if this is a scene object reference (has "guids" property)
                if (jObj.TryGetValue("guids", StringComparison.OrdinalIgnoreCase, out var guidsToken))
                {
                    var guids = guidsToken as JArray;
                    if (guids != null && guids.Count > 0)
                        return guids[0]?.ToString();
                }

                // Convert JObject to Dictionary
                var dict = new Dictionary<string, object>();
                foreach (var prop in jObj.Properties())
                {
                    dict[prop.Name] = ResolveJsonValue(prop.Value);
                }
                return dict;
            }
            else if (value is JArray jArr)
            {
                var list = new List<object>(jArr.Count);
                foreach (var item in jArr)
                    list.Add(ResolveJsonValue(item));
                return list;
            }
            else if (value is JValue jVal)
            {
                return jVal.Value;
            }
            // Handle pre-resolved Dictionary<string, object> (from type-binder mapped types)
            else if (value is IDictionary<string, object> dict2)
            {
                return ExtractFirstGuidOrResolve(dict2);
            }

            return value;
        }

        /// <summary>
        /// Extracts the first GUID from a VR Builder reference structure
        /// (e.g., { "guids": [guid] } or { "Targets": [guid] }) or recursively resolves dictionary values.
        /// </summary>
        private static object ExtractFirstGuidOrResolve(IDictionary<string, object> dict)
        {
            // Check for "guids" key (MultipleSceneObjectReference pattern)
            if (dict.TryGetValue("guids", out var guidsVal))
            {
                var guid = ExtractFirstGuidFromList(guidsVal);
                if (guid != null) return guid;
            }

            // Check for "Targets" key containing a list of GUIDs (MultipleGrabbablePropertyReference pattern)
            if (dict.TryGetValue("Targets", out var targetsVal))
            {
                var guid = ExtractFirstGuidFromList(targetsVal);
                if (guid != null) return guid;
            }

            // Recursively resolve values
            var resolved = new Dictionary<string, object>();
            foreach (var kvp in dict)
                resolved[kvp.Key] = ResolveJsonValue(kvp.Value);
            return resolved;
        }

        private static string ExtractFirstGuidFromList(object listObj)
        {
            if (listObj is IList<object> list && list.Count > 0)
                return list[0]?.ToString();
            if (listObj is JArray jArr && jArr.Count > 0)
                return jArr[0]?.ToString();
            return null;
        }

        private StepType ParseStepType(string stepType)
        {
            if (string.IsNullOrEmpty(stepType)) return StepType.Default;
            return stepType.ToLowerInvariant() switch
            {
                "stepgroup" => StepType.StepGroup,
                _ => StepType.Default
            };
        }

        public ValidationResult Validate(ProcessModel process, JsonProcessWrapper wrapper)
        {
            var result = new ValidationResult();

            if (process == null)
            {
                result.AddError("Process model is null");
                return result;
            }

            // Check chapters exist
            if (process.ChapterModels.Count == 0)
                result.AddError("Process has no chapters");

            if (process.FirstChapterModel == null)
                result.AddError("Process has no first chapter");

            // Check each chapter has steps and a first step
            foreach (var chapter in process.ChapterModels)
            {
                if (chapter.StepModels.Count == 0)
                    result.AddWarning($"Chapter '{chapter.Name}' has no steps");

                if (chapter.FirstStepModel == null)
                    result.AddError($"Chapter '{chapter.Name}' has no first step");

                // Validate each step
                foreach (var step in chapter.StepModels)
                {
                    ValidateStep(step, result);
                }
            }

            // Collect scene object GUIDs from behaviors
            CollectSceneObjectGuids(process, result);

            return result;
        }

        private void ValidateStep(StepModel step, ValidationResult result)
        {
            if (string.IsNullOrEmpty(step.Id))
                result.AddError($"Step '{step.Name}' has no ID");

            // Check transitions
            foreach (var transition in step.TransitionModels)
            {
                if (transition.TargetStepModel != null && string.IsNullOrEmpty(transition.TargetStepModel.Name)
                    && string.IsNullOrEmpty(transition.TargetStepModel.Id))
                {
                    result.AddError($"Step '{step.Name}' has a transition with an unresolved target step");
                }
            }
        }

        private void CollectSceneObjectGuids(ProcessModel process, ValidationResult result)
        {
            // Scene object GUIDs are resolved at runtime by SceneObjectResolver
        }

        /// <summary>
        /// Extracts a step GUID from an object that may be JsonStep or JsonStepRef
        /// (due to Newtonsoft $ref resolving to either type).
        /// </summary>
        private static string ExtractStepGuid(object stepObj)
        {
            if (stepObj is JsonStep step)
                return step.StepMetadata?.Guid;
            if (stepObj is JsonStepRef stepRef)
                return stepRef.StepMetadata?.Guid;
            return null;
        }
    }
}
