using System.Collections.Generic;
using Newtonsoft.Json;

namespace ReactiveFlowEngine.Serialization.JsonModels
{
    public class JsonChapter
    {
        [JsonProperty("ChapterMetadata")]
        public JsonChapterMetadata ChapterMetadata { get; set; }

        [JsonProperty("Data")]
        public JsonChapterData Data { get; set; }
    }

    public class JsonChapterMetadata
    {
        [JsonProperty("Guid")]
        public string Guid { get; set; }
    }

    public class JsonChapterData
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        // object because $ref can resolve to JsonStep or JsonStepRef depending on context
        [JsonProperty("FirstStep")]
        public object FirstStep { get; set; }

        // List<object> because Newtonsoft handles $values natively for collection types,
        // and items can be JsonStep or JsonStepRef via $ref resolution
        [JsonProperty("Steps")]
        public List<object> Steps { get; set; }
    }
}
