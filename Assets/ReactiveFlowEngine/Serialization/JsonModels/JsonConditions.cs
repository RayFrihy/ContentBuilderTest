using Newtonsoft.Json;

namespace ReactiveFlowEngine.Serialization.JsonModels
{
    public class JsonTimeoutCondition
    {
        [JsonProperty("Data")]
        public JsonTimeoutConditionData Data { get; set; }
    }

    public class JsonTimeoutConditionData
    {
        [JsonProperty("Timeout")]
        public float Timeout { get; set; }
    }
}
