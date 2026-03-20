using System.Collections.Generic;
using UnityEngine;

namespace ReactiveFlowEngine.Model
{
    public sealed class BehaviorDefinition : DefinitionBase
    {
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

        public Vector3 GetVector3(string key)
        {
            if (Parameters.TryGetValue(key, out var value))
            {
                if (value is Vector3 v) return v;
                if (value is Dictionary<string, object> dict)
                {
                    return new Vector3(
                        dict.ContainsKey("x") ? System.Convert.ToSingle(dict["x"]) : 0f,
                        dict.ContainsKey("y") ? System.Convert.ToSingle(dict["y"]) : 0f,
                        dict.ContainsKey("z") ? System.Convert.ToSingle(dict["z"]) : 0f);
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
                        dict.ContainsKey("x") ? System.Convert.ToSingle(dict["x"]) : 0f,
                        dict.ContainsKey("y") ? System.Convert.ToSingle(dict["y"]) : 0f,
                        dict.ContainsKey("z") ? System.Convert.ToSingle(dict["z"]) : 0f,
                        dict.ContainsKey("w") ? System.Convert.ToSingle(dict["w"]) : 1f);
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
    }
}
