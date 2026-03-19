using System;
using System.Collections.Generic;

namespace ReactiveFlowEngine.Model
{
    public sealed class ConditionDefinition
    {
        public string TypeName { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

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
    }
}
