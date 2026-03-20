using System;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.State
{
    public sealed class StateNotExistsCondition : IStateCondition
    {
        private readonly IStateStore _stateStore;
        private readonly string _stateKey;

        public string StateKey => _stateKey;

        public StateNotExistsCondition(IStateStore stateStore, string stateKey)
        {
            _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
            _stateKey = stateKey ?? throw new ArgumentNullException(nameof(stateKey));
        }

        public Observable<bool> Evaluate()
        {
            return Observable.EveryUpdate()
                .Select(_ => !_stateStore.HasGlobalState(_stateKey))
                .DistinctUntilChanged();
        }

        public void Reset() { }

        public void Dispose() { }
    }
}
