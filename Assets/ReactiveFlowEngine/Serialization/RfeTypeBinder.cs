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
            ["TimeoutCondition"] = typeof(JsonTimeoutCondition),
            ["TimeoutConditionData"] = typeof(JsonTimeoutConditionData),
            ["MoveObjectBehavior"] = typeof(JsonMoveObjectBehavior),
            ["MoveObjectBehaviorData"] = typeof(JsonMoveObjectBehaviorData),
            ["ExecuteChapterBehavior"] = typeof(JsonExecuteChapterBehavior),
            ["ExecuteChapterBehaviorData"] = typeof(JsonExecuteChapterBehaviorData),
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
            // List types: Newtonsoft natively handles $values for collection types
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

        private readonly Dictionary<Type, string> _reverseMap;

        public RfeTypeBinder()
        {
            _reverseMap = new Dictionary<Type, string>();
            foreach (var kvp in _typeMap)
            {
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
    }
}
