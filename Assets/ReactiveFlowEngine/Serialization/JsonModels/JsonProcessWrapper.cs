using System.Collections.Generic;
using Newtonsoft.Json;

namespace ReactiveFlowEngine.Serialization.JsonModels
{
    public class JsonProcessWrapper
    {
        [JsonProperty("SubChapters")]
        public List<object> SubChapters { get; set; }

        [JsonProperty("Steps")]
        public List<object> Steps { get; set; }

        [JsonProperty("Process")]
        public JsonProcess Process { get; set; }
    }
}
