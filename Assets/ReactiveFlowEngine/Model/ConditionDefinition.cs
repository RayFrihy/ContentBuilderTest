using System;
using System.Collections.Generic;
using System.Linq;

namespace ReactiveFlowEngine.Model
{
    public sealed class ConditionDefinition
    {
        public string TypeName { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
        public List<ConditionDefinition> Children { get; set; } = new List<ConditionDefinition>();

        public float GetFloat(string key)
        {
            if (Parameters.TryGetValue(key, out var value))
            {
                if (value is float f) return f;
                if (value is double d) return (float)d;
                if (value is int i) return i;
                if (value is long l) return l;
                return Convert.ToSingle(value);
            }
            return 0f;
        }

        public string GetString(string key)
        {
            if (Parameters.TryGetValue(key, out var value))
                return value?.ToString();
            return null;
        }

        public bool GetBool(string key)
        {
            if (Parameters.TryGetValue(key, out var value))
            {
                if (value is bool b) return b;
                return Convert.ToBoolean(value);
            }
            return false;
        }

        public int GetInt(string key)
        {
            if (Parameters.TryGetValue(key, out var value))
            {
                if (value is int i) return i;
                if (value is long l) return (int)l;
                if (value is float f) return (int)f;
                if (value is double d) return (int)d;
                return Convert.ToInt32(value);
            }
            return 0;
        }

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
