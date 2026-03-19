using Newtonsoft.Json;

namespace ReactiveFlowEngine.Serialization.JsonModels
{
    public class JsonStep
    {
        [JsonProperty("StepMetadata")]
        public JsonStepMetadata StepMetadata { get; set; }

        [JsonProperty("Data")]
        public JsonStepData Data { get; set; }
    }

    public class JsonStepRef
    {
        [JsonProperty("StepMetadata")]
        public JsonStepMetadata StepMetadata { get; set; }

        [JsonProperty("LifeCycle")]
        public object LifeCycle { get; set; }

        [JsonProperty("Parent")]
        public object Parent { get; set; }
    }

    public class JsonStepMetadata
    {
        [JsonProperty("Position")]
        public JsonVector2 Position { get; set; }

        [JsonProperty("StepType")]
        public string StepType { get; set; }

        [JsonProperty("Guid")]
        public string Guid { get; set; }
    }

    public class JsonVector2
    {
        [JsonProperty("x")]
        public float X { get; set; }

        [JsonProperty("y")]
        public float Y { get; set; }
    }

    public class JsonStepData
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("Behaviors")]
        public JsonBehaviorCollection Behaviors { get; set; }

        [JsonProperty("Transitions")]
        public JsonTransitionCollection Transitions { get; set; }
    }
}
