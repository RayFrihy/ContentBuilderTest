using System;
using System.Collections.Generic;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.State
{
    /// <summary>
    /// Serializes and deserializes StepSnapshot objects for persistence.
    /// Uses JSON format via Unity's JsonUtility for simple types and manual serialization for complex types.
    /// </summary>
    public static class SnapshotSerializer
    {
        public static string Serialize(StepSnapshot snapshot)
        {
            if (snapshot == null) return null;

            var wrapper = new SerializableSnapshot
            {
                StepId = snapshot.StepId,
                ChapterId = snapshot.ChapterId,
                TimestampTicks = snapshot.Timestamp.UtcTicks,
                BehaviorStates = new List<SerializableBehaviorState>()
            };

            // Serialize simple state values
            var stateEntries = new List<SerializableStateEntry>();
            foreach (var kvp in snapshot.State)
            {
                stateEntries.Add(new SerializableStateEntry
                {
                    Key = kvp.Key,
                    ValueJson = SerializeValue(kvp.Value),
                    ValueType = kvp.Value?.GetType().FullName ?? "null"
                });
            }
            wrapper.StateEntries = stateEntries;

            foreach (var bs in snapshot.BehaviorStates)
            {
                var sbs = new SerializableBehaviorState
                {
                    BehaviorType = bs.BehaviorType,
                    DataEntries = new List<SerializableStateEntry>()
                };
                foreach (var kvp in bs.Data)
                {
                    sbs.DataEntries.Add(new SerializableStateEntry
                    {
                        Key = kvp.Key,
                        ValueJson = SerializeValue(kvp.Value),
                        ValueType = kvp.Value?.GetType().FullName ?? "null"
                    });
                }
                wrapper.BehaviorStates.Add(sbs);
            }

            return JsonUtility.ToJson(wrapper);
        }

        public static StepSnapshot Deserialize(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;

            var wrapper = JsonUtility.FromJson<SerializableSnapshot>(json);
            if (wrapper == null) return null;

            var snapshot = new StepSnapshot
            {
                StepId = wrapper.StepId,
                ChapterId = wrapper.ChapterId,
                Timestamp = new DateTimeOffset(wrapper.TimestampTicks, TimeSpan.Zero),
                State = new Dictionary<string, object>(),
                BehaviorStates = new List<BehaviorSnapshot>()
            };

            if (wrapper.StateEntries != null)
            {
                foreach (var entry in wrapper.StateEntries)
                {
                    snapshot.State[entry.Key] = DeserializeValue(entry.ValueJson, entry.ValueType);
                }
            }

            if (wrapper.BehaviorStates != null)
            {
                foreach (var sbs in wrapper.BehaviorStates)
                {
                    var bs = new BehaviorSnapshot
                    {
                        BehaviorType = sbs.BehaviorType,
                        Data = new Dictionary<string, object>()
                    };
                    if (sbs.DataEntries != null)
                    {
                        foreach (var entry in sbs.DataEntries)
                        {
                            bs.Data[entry.Key] = DeserializeValue(entry.ValueJson, entry.ValueType);
                        }
                    }
                    snapshot.BehaviorStates.Add(bs);
                }
            }

            return snapshot;
        }

        private static string SerializeValue(object value)
        {
            if (value == null) return "null";
            if (value is Vector3 v3) return $"{v3.x},{v3.y},{v3.z}";
            if (value is Quaternion q) return $"{q.x},{q.y},{q.z},{q.w}";
            if (value is bool b) return b.ToString();
            if (value is float f) return f.ToString("R");
            if (value is double d) return d.ToString("R");
            if (value is int i) return i.ToString();
            return value.ToString();
        }

        private static object DeserializeValue(string json, string typeName)
        {
            if (json == "null" || typeName == "null") return null;

            if (typeName == typeof(Vector3).FullName)
            {
                var parts = json.Split(',');
                if (parts.Length == 3)
                    return new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
            }
            else if (typeName == typeof(Quaternion).FullName)
            {
                var parts = json.Split(',');
                if (parts.Length == 4)
                    return new Quaternion(float.Parse(parts[0]), float.Parse(parts[1]),
                        float.Parse(parts[2]), float.Parse(parts[3]));
            }
            else if (typeName == typeof(bool).FullName)
            {
                return bool.Parse(json);
            }
            else if (typeName == typeof(float).FullName)
            {
                return float.Parse(json);
            }
            else if (typeName == typeof(int).FullName)
            {
                return int.Parse(json);
            }
            else if (typeName == typeof(double).FullName)
            {
                return double.Parse(json);
            }

            return json;
        }

        [Serializable]
        private class SerializableSnapshot
        {
            public string StepId;
            public string ChapterId;
            public long TimestampTicks;
            public List<SerializableStateEntry> StateEntries;
            public List<SerializableBehaviorState> BehaviorStates;
        }

        [Serializable]
        private class SerializableStateEntry
        {
            public string Key;
            public string ValueJson;
            public string ValueType;
        }

        [Serializable]
        private class SerializableBehaviorState
        {
            public string BehaviorType;
            public List<SerializableStateEntry> DataEntries;
        }
    }
}
