using System;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.TimeBased
{
    public sealed class DelayElapsedCondition : ITimeBasedCondition
    {
        private readonly float _delay;
        private IDisposable _subscription;

        public float Duration => _delay;

        public DelayElapsedCondition(float delay)
        {
            _delay = delay;
        }

        public Observable<bool> Evaluate()
        {
            return Observable.Timer(TimeSpan.FromSeconds(_delay))
                .Select(_ => true)
                .Prepend(false);
        }

        public void Reset() { }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
        }
    }
}
