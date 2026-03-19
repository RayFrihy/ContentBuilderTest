using System;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.State
{
    public sealed class StringStateCondition : IStateCondition
    {
        private readonly IStateStore _stateStore;
        private readonly string _stateKey;
        private readonly string _expectedValue;
        private readonly bool _ignoreCase;
        private IDisposable _subscription;

        public string StateKey => _stateKey;

        public StringStateCondition(IStateStore stateStore, string stateKey, string expectedValue, bool ignoreCase = false)
        {
            _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
            _stateKey = stateKey ?? throw new ArgumentNullException(nameof(stateKey));
            _expectedValue = expectedValue;
            _ignoreCase = ignoreCase;
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
            var stringValue = value?.ToString();

            var comparison = _ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            return string.Equals(stringValue, _expectedValue, comparison);
        }
    }
}
