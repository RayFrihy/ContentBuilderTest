using System;
using System.Collections.Generic;

namespace ReactiveFlowEngine.Model
{
    public abstract class DefinitionBase
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

        public object GetObject(string key)
        {
            Parameters.TryGetValue(key, out var value);
            return value;
        }
    }
}
