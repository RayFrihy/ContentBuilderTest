using System;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.Environment
{
    public sealed class TriggerEnterCondition : IEnvironmentCondition
    {
        private readonly IEventBus _eventBus;
        private readonly string _triggerObjectId;
        private readonly string _enteringObjectId;
        private IDisposable _subscription;

        public TriggerEnterCondition(IEventBus eventBus, string triggerObjectId, string enteringObjectId)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _triggerObjectId = triggerObjectId ?? throw new ArgumentNullException(nameof(triggerObjectId));
            _enteringObjectId = enteringObjectId ?? throw new ArgumentNullException(nameof(enteringObjectId));
        }

        public Observable<bool> Evaluate()
        {
            return _eventBus.On("TriggerEnter")
                .Select(payload => IsMatchingTrigger(payload))
                .Prepend(false);
        }

        public void Reset() { }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
        }

        private bool IsMatchingTrigger(object payload)
        {
            if (payload is TriggerEventData data)
            {
                return string.Equals(data.TriggerObjectId, _triggerObjectId, StringComparison.Ordinal) &&
                       string.Equals(data.OtherObjectId, _enteringObjectId, StringComparison.Ordinal);
            }
            return false;
        }
    }

    public sealed class TriggerEventData
    {
        public string TriggerObjectId { get; }
        public string OtherObjectId { get; }

        public TriggerEventData(string triggerObjectId, string otherObjectId)
        {
            TriggerObjectId = triggerObjectId;
            OtherObjectId = otherObjectId;
        }
    }
}
