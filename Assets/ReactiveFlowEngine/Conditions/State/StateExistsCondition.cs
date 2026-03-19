using System;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.State
{
    public sealed class StateExistsCondition : IStateCondition
    {
        private readonly IStateStore _stateStore;
        private readonly string _stateKey;
        private IDisposable _subscription;

        public string StateKey => _stateKey;

        public StateExistsCondition(IStateStore stateStore, string stateKey)
        {
            _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
            _stateKey = stateKey ?? throw new ArgumentNullException(nameof(stateKey));
        }

        public Observable<bool> Evaluate()
        {
            return Observable.EveryUpdate()
                .Select(_ => _stateStore.HasGlobalState(_stateKey))
                .DistinctUntilChanged();
        }

        public void Reset() { }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
        }
    }
}
