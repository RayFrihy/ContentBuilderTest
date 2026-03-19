using System.Collections.Generic;
using Newtonsoft.Json;

namespace ReactiveFlowEngine.Serialization.JsonModels
{
    public class JsonTransition
    {
        [JsonProperty("Data")]
        public JsonTransitionData Data { get; set; }
    }

    public class JsonTransitionData
    {
        [JsonProperty("Conditions")]
        public List<object> Conditions { get; set; }

        [JsonProperty("TargetStep")]
        public JsonStepRef TargetStep { get; set; }
    }

    public class JsonTransitionCollection
    {
        [JsonProperty("Data")]
        public JsonTransitionCollectionData Data { get; set; }
    }

    public class JsonTransitionCollectionData
    {
        [JsonProperty("Transitions")]
        public List<object> Transitions { get; set; }
    }
}
