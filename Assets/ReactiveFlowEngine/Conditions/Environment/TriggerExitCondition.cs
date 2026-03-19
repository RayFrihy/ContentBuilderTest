using System;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.Environment
{
    public sealed class TriggerExitCondition : IEnvironmentCondition
    {
        private readonly IEventBus _eventBus;
        private readonly string _triggerObjectId;
        private readonly string _exitingObjectId;
        private IDisposable _subscription;

        public TriggerExitCondition(IEventBus eventBus, string triggerObjectId, string exitingObjectId)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _triggerObjectId = triggerObjectId ?? throw new ArgumentNullException(nameof(triggerObjectId));
            _exitingObjectId = exitingObjectId ?? throw new ArgumentNullException(nameof(exitingObjectId));
        }

        public Observable<bool> Evaluate()
        {
            return _eventBus.On("TriggerExit")
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
                       string.Equals(data.OtherObjectId, _exitingObjectId, StringComparison.Ordinal);
            }
            return false;
        }
    }
}
