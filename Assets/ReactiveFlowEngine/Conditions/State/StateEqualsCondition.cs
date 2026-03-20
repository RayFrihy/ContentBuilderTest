using System;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.State
{
    public sealed class StateEqualsCondition : IStateCondition
    {
        private readonly IStateStore _stateStore;
        private readonly string _stateKey;
        private readonly object _expectedValue;

        public string StateKey => _stateKey;

        public StateEqualsCondition(IStateStore stateStore, string stateKey, object expectedValue)
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

        public void Dispose() { }

        private bool CheckState()
        {
            var value = _stateStore.GetGlobalState(_stateKey);
            if (value == null && _expectedValue == null)
                return true;
            if (value == null || _expectedValue == null)
                return false;
            return value.Equals(_expectedValue);
        }
    }
}
