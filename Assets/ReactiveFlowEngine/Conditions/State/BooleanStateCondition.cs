using System;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.State
{
    public sealed class BooleanStateCondition : IStateCondition
    {
        private readonly IStateStore _stateStore;
        private readonly string _stateKey;
        private readonly bool _expectedValue;
        private IDisposable _subscription;

        public string StateKey => _stateKey;

        public BooleanStateCondition(IStateStore stateStore, string stateKey, bool expectedValue)
        {
            _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
            _stateKey = stateKey ?? throw new ArgumentNullException(nameof(stateKey));
            _expectedValue = expectedValue;
        }

        public Observable<bool> Evaluate()
        {
            return Observable.EveryUpdate()
                .Select(_ => CheckState())
                .DistinctUntilChanged();
        }

        public void Reset() { }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
        }

        private bool CheckState()
        {
            var value = _stateStore.GetGlobalState(_stateKey);
            if (value is bool b)
                return b == _expectedValue;
            return false;
        }
    }
}
