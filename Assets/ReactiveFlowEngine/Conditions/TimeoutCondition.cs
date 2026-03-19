using System;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions
{
    public class TimeoutCondition : ICondition
    {
        private readonly float _timeout;
        private IDisposable _subscription;

        public TimeoutCondition(float timeout)
        {
            _timeout = timeout;
        }

        public Observable<bool> Evaluate()
        {
            return Observable.Timer(TimeSpan.FromSeconds(_timeout))
                .Select(_ => true)
                .Prepend(false);
        }

        public void Reset()
        {
            // Stateless - timer restarts on new subscription
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
        }
    }
}
