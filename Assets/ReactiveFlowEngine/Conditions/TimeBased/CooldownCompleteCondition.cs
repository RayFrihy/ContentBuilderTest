using System;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.TimeBased
{
    public sealed class CooldownCompleteCondition : ITimeBasedCondition
    {
        private readonly IEventBus _eventBus;
        private readonly string _cooldownId;
        private readonly float _cooldownDuration;

        public float Duration => _cooldownDuration;

        public CooldownCompleteCondition(IEventBus eventBus, string cooldownId, float cooldownDuration)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _cooldownId = cooldownId ?? throw new ArgumentNullException(nameof(cooldownId));
            _cooldownDuration = cooldownDuration;
        }

        public Observable<bool> Evaluate()
        {
            return _eventBus.On("CooldownStarted")
                .Where(payload => IsMatchingCooldown(payload))
                .SelectMany(_ =>
                    Observable.Timer(TimeSpan.FromSeconds(_cooldownDuration))
                        .Select(__ => true)
                        .Prepend(false)
                )
                .Prepend(true);
        }

        public void Reset() { }

        public void Dispose() { }

        private bool IsMatchingCooldown(object payload)
        {
            if (payload is string cooldownId)
                return string.Equals(cooldownId, _cooldownId, StringComparison.Ordinal);
            return false;
        }
    }
}
