using System;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.Interaction
{
    public sealed class EventBusCondition : IInteractionCondition
    {
        private readonly IEventBus _eventBus;
        private readonly string _eventName;
        private readonly string _targetId;

        public string TargetObjectId => _targetId;

        public EventBusCondition(IEventBus eventBus, string eventName, string targetId)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _eventName = eventName ?? throw new ArgumentNullException(nameof(eventName));
            _targetId = targetId ?? throw new ArgumentNullException(nameof(targetId));
        }

        public Observable<bool> Evaluate()
        {
            return _eventBus.On(_eventName)
                .Select(payload => payload is string id &&
                        string.Equals(id, _targetId, StringComparison.Ordinal))
                .Prepend(false);
        }

        public void Reset() { }

        public void Dispose() { }
    }
}
