using System.Collections.Generic;
using Newtonsoft.Json;

namespace ReactiveFlowEngine.Serialization.JsonModels
{
    public class JsonProcess
    {
        [JsonProperty("ProcessMetadata")]
        public JsonProcessMetadata ProcessMetadata { get; set; }

        [JsonProperty("Data")]
        public JsonProcessData Data { get; set; }
    }

    public class JsonProcessMetadata
    {
        [JsonProperty("Guid")]
        public string Guid { get; set; }
    }

    public class JsonProcessData
    {
        [JsonProperty("Chapters")]
        public List<object> Chapters { get; set; }

        [JsonProperty("FirstChapter")]
        public object FirstChapter { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }
    }
}
