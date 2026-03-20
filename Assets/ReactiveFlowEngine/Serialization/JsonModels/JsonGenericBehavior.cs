using System.Collections.Generic;
using Newtonsoft.Json;

namespace ReactiveFlowEngine.Serialization.JsonModels
{
    public class JsonGenericBehavior
    {
        [JsonProperty("$type")]
        public string TypeDiscriminator { get; set; }

        [JsonProperty("Data")]
        public Dictionary<string, object> Data { get; set; }
    }
}
