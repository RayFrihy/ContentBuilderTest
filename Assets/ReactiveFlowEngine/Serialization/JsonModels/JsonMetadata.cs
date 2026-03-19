using System.Collections.Generic;
using Newtonsoft.Json;

namespace ReactiveFlowEngine.Serialization.JsonModels
{
    // These are structural types in the JSON that we don't need in the domain model.
    // They exist only so Newtonsoft can deserialize without errors.

    public class JsonMetadata
    {
        [JsonProperty("values")]
        public object Values { get; set; }
    }

    public class JsonMetadataValuesDictionary : Dictionary<string, object> { }

    public class JsonConditionDictionary : Dictionary<string, object> { }

    public class JsonListOfAttributeData
    {
        [JsonProperty("ChildAttributes")]
        public JsonTypedList<object> ChildAttributes { get; set; }

        [JsonProperty("ChildMetadata")]
        public JsonTypedList<object> ChildMetadata { get; set; }
    }

    public class JsonFoldableAttribute
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("TypeId")]
        public string TypeId { get; set; }
    }

    public class JsonHelpAttribute
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("TypeId")]
        public string TypeId { get; set; }
    }

    public class JsonMenuAttribute
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("TypeId")]
        public string TypeId { get; set; }
    }

    public class JsonExtendableListAttributeWrapper
    {
        [JsonProperty("Type")]
        public string Type { get; set; }
    }

    public class JsonReorderableElementMetadata
    {
        [JsonProperty("MoveUp")]
        public bool MoveUp { get; set; }

        [JsonProperty("MoveDown")]
        public bool MoveDown { get; set; }

        [JsonProperty("IsFirst")]
        public bool IsFirst { get; set; }

        [JsonProperty("IsLast")]
        public bool IsLast { get; set; }
    }

    public class JsonViewTransform
    {
        [JsonProperty("Position")]
        public JsonVector3 Position { get; set; }

        [JsonProperty("Scale")]
        public JsonVector3 Scale { get; set; }
    }

    public class JsonVector3
    {
        [JsonProperty("x")]
        public float X { get; set; }

        [JsonProperty("y")]
        public float Y { get; set; }

        [JsonProperty("z")]
        public float Z { get; set; }
    }
}
