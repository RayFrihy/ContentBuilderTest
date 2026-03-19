using System.Collections.Generic;
using Newtonsoft.Json;

namespace ReactiveFlowEngine.Serialization.JsonModels
{
    public class JsonTypedList<T>
    {
        [JsonProperty("$values")]
        public List<T> Values { get; set; } = new List<T>();
    }
}
