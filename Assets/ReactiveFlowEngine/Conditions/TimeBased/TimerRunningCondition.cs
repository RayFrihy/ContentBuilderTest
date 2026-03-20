using System;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.TimeBased
{
    public sealed class TimerRunningCondition : ITimeBasedCondition
    {
        private readonly IEventBus _eventBus;
        private readonly string _timerId;

        public float Duration => 0f;

        public TimerRunningCondition(IEventBus eventBus, string timerId)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _timerId = timerId ?? throw new ArgumentNullException(nameof(timerId));
        }

        public Observable<bool> Evaluate()
        {
            var started = _eventBus.On("TimerStarted")
                .Where(payload => IsMatchingTimer(payload))
                .Select(_ => true);

            var stopped = _eventBus.On("TimerStopped")
                .Where(payload => IsMatchingTimer(payload))
                .Select(_ => false);

            return Observable.Merge(started, stopped)
                .Prepend(false);
        }

        public void Reset() { }

        public void Dispose() { }

        private bool IsMatchingTimer(object payload)
        {
            if (payload is string timerId)
                return string.Equals(timerId, _timerId, StringComparison.Ordinal);
            return false;
        }
    }
}
