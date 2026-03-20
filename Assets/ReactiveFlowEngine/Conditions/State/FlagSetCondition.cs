using System;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.State
{
    public sealed class FlagSetCondition : IStateCondition
    {
        private readonly IStateStore _stateStore;
        private readonly string _flagKey;

        public string StateKey => _flagKey;

        public FlagSetCondition(IStateStore stateStore, string flagKey)
        {
            _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
            _flagKey = flagKey ?? throw new ArgumentNullException(nameof(flagKey));
        }

        public Observable<bool> Evaluate()
        {
            return Observable.EveryUpdate()
                .Select(_ => IsFlagSet())
                .DistinctUntilChanged();
        }

        public void Reset() { }

        public void Dispose() { }

        private bool IsFlagSet()
        {
            var value = _stateStore.GetGlobalState(_flagKey);
            if (value is bool b)
                return b;
            return _stateStore.HasGlobalState(_flagKey) && value != null;
        }
    }
}
