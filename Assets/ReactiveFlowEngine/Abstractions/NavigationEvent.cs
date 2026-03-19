using System;

namespace ReactiveFlowEngine.Abstractions
{
    public sealed class NavigationEvent
    {
        public NavigationType Type { get; }
        public string FromStepId { get; }
        public string ToStepId { get; }
        public DateTimeOffset Timestamp { get; }

        public NavigationEvent(NavigationType type, string fromStepId, string toStepId, DateTimeOffset timestamp)
        {
            Type = type;
            FromStepId = fromStepId;
            ToStepId = toStepId;
            Timestamp = timestamp;
        }
    }
}
