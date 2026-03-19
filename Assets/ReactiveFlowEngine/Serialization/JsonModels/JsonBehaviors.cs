using System.Collections.Generic;
using Newtonsoft.Json;

namespace ReactiveFlowEngine.Serialization.JsonModels
{
    public class JsonBehaviorCollection
    {
        [JsonProperty("Data")]
        public JsonBehaviorCollectionData Data { get; set; }
    }

    public class JsonBehaviorCollectionData
    {
        [JsonProperty("Behaviors")]
        public List<object> Behaviors { get; set; }
    }

    public class JsonMoveObjectBehavior
    {
        [JsonProperty("Data")]
        public JsonMoveObjectBehaviorData Data { get; set; }
    }

    public class JsonMoveObjectBehaviorData
    {
        [JsonProperty("ExecutionStages")]
        public int ExecutionStages { get; set; }

        [JsonProperty("TargetObject")]
        public JsonSceneObjRef TargetObject { get; set; }

        [JsonProperty("FinalPosition")]
        public JsonSceneObjRef FinalPosition { get; set; }

        [JsonProperty("Duration")]
        public float Duration { get; set; }

        [JsonProperty("AnimationCurve")]
        public JsonAnimationCurve AnimationCurve { get; set; }

        [JsonProperty("IsBlocking")]
        public bool IsBlocking { get; set; }
    }

    public class JsonExecuteChapterBehavior
    {
        [JsonProperty("Data")]
        public JsonExecuteChapterBehaviorData Data { get; set; }
    }

    public class JsonExecuteChapterBehaviorData
    {
        [JsonProperty("Chapter")]
        public JsonChapter Chapter { get; set; }
    }

    public class JsonSceneObjRef
    {
        [JsonProperty("guids")]
        public List<object> Guids { get; set; }
    }

    public class JsonAnimationCurve
    {
        [JsonProperty("Keys")]
        public JsonKeyframe[] Keys { get; set; }

        [JsonProperty("PreWrapMode")]
        public int PreWrapMode { get; set; }

        [JsonProperty("PostWrapMode")]
        public int PostWrapMode { get; set; }
    }

    public class JsonKeyframe
    {
        [JsonProperty("Time")]
        public float Time { get; set; }

        [JsonProperty("Value")]
        public float Value { get; set; }

        [JsonProperty("InTangent")]
        public float InTangent { get; set; }

        [JsonProperty("OutTangent")]
        public float OutTangent { get; set; }

        [JsonProperty("InWeight")]
        public float InWeight { get; set; }

        [JsonProperty("OutWeight")]
        public float OutWeight { get; set; }

        [JsonProperty("WeightedMode")]
        public int WeightedMode { get; set; }
    }
}
