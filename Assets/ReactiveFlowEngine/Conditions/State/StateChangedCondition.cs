using System;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.State
{
    public sealed class StateChangedCondition : IStateCondition
    {
        private readonly IStateStore _stateStore;
        private readonly string _stateKey;
        private object _previousValue;
        private bool _initialized;
        private IDisposable _subscription;

        public string StateKey => _stateKey;

        public StateChangedCondition(IStateStore stateStore, string stateKey)
        {
            _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
            _stateKey = stateKey ?? throw new ArgumentNullException(nameof(stateKey));
        }

        public Observable<bool> Evaluate()
        {
            return Observable.EveryUpdate()
                .Select(_ => HasChanged())
                .DistinctUntilChanged();
        }

        public void Reset()
        {
            _previousValue = null;
            _initialized = false;
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
        }

        private bool HasChanged()
        {
            var currentValue = _stateStore.GetGlobalState(_stateKey);

            if (!_initialized)
            {
                _previousValue = currentValue;
                _initialized = true;
                return false;
            }

            bool changed = !Equals(_previousValue, currentValue);
            _previousValue = currentValue;
            return changed;
        }
    }
}
