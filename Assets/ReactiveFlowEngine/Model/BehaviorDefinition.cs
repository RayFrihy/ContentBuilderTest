using System;
using System.Collections.Generic;
using UnityEngine;

namespace ReactiveFlowEngine.Model
{
    public sealed class BehaviorDefinition
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

        public ChapterModel GetChapter(string key)
        {
            if (Parameters.TryGetValue(key, out var value))
                return value as ChapterModel;
            return null;
        }

        public AnimationCurve GetAnimationCurve(string key)
        {
            if (Parameters.TryGetValue(key, out var value))
            {
                if (value is AnimationCurve curve) return curve;
                if (value is AnimationCurveData data) return data.ToAnimationCurve();
            }
            return AnimationCurve.Linear(0f, 0f, 1f, 1f);
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
                return Convert.ToInt32(value);
            }
            return 0;
        }

        public Vector3 GetVector3(string key)
        {
            if (Parameters.TryGetValue(key, out var value))
            {
                if (value is Vector3 v) return v;
                if (value is Dictionary<string, object> dict)
                {
                    return new Vector3(
                        dict.ContainsKey("x") ? Convert.ToSingle(dict["x"]) : 0f,
                        dict.ContainsKey("y") ? Convert.ToSingle(dict["y"]) : 0f,
                        dict.ContainsKey("z") ? Convert.ToSingle(dict["z"]) : 0f);
                }
            }
            return Vector3.zero;
        }

        public Quaternion GetQuaternion(string key)
        {
            if (Parameters.TryGetValue(key, out var value))
            {
                if (value is Quaternion q) return q;
                if (value is Dictionary<string, object> dict)
                {
                    return new Quaternion(
                        dict.ContainsKey("x") ? Convert.ToSingle(dict["x"]) : 0f,
                        dict.ContainsKey("y") ? Convert.ToSingle(dict["y"]) : 0f,
                        dict.ContainsKey("z") ? Convert.ToSingle(dict["z"]) : 0f,
                        dict.ContainsKey("w") ? Convert.ToSingle(dict["w"]) : 1f);
                }
            }
            return Quaternion.identity;
        }

        public Color GetColor(string key)
        {
            if (Parameters.TryGetValue(key, out var value))
            {
                if (value is Color c) return c;
                if (value is string hex && ColorUtility.TryParseHtmlString(hex, out var parsed))
                    return parsed;
            }
            return Color.white;
        }

        public List<BehaviorDefinition> GetBehaviorDefinitionList(string key)
        {
            if (Parameters.TryGetValue(key, out var value))
                return value as List<BehaviorDefinition>;
            return new List<BehaviorDefinition>();
        }

        public object GetObject(string key)
        {
            Parameters.TryGetValue(key, out var value);
            return value;
        }
    }
}
