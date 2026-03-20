using System;
using System.Collections.Generic;
using System.Linq;

namespace ReactiveFlowEngine.Model
{
    public sealed class ConditionDefinition : DefinitionBase
    {
        public List<ConditionDefinition> Children { get; set; } = new List<ConditionDefinition>();

        public string[] GetStringArray(string key)
        {
            if (Parameters.TryGetValue(key, out var value))
            {
                if (value is string[] arr) return arr;
                if (value is List<object> list) return list.Select(o => o?.ToString()).ToArray();
                if (value is object[] objArr) return objArr.Select(o => o?.ToString()).ToArray();
            }
            return Array.Empty<string>();
        }

        public T GetEnum<T>(string key) where T : struct, Enum
        {
            var str = GetString(key);
            if (str != null && Enum.TryParse<T>(str, true, out var result))
                return result;
            return default;
        }
    }
}
