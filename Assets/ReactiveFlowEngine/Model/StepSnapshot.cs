using System;
using System.Collections.Generic;

namespace ReactiveFlowEngine.Model
{
    public sealed class StepSnapshot
    {
        public string StepId { get; set; }
        public string ChapterId { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public Dictionary<string, object> State { get; set; } = new Dictionary<string, object>();
        public List<BehaviorSnapshot> BehaviorStates { get; set; } = new List<BehaviorSnapshot>();
    }

    public sealed class BehaviorSnapshot
    {
        public string BehaviorType { get; set; }
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
    }
}
