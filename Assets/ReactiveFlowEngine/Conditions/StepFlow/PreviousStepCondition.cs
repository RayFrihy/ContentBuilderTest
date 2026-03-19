using System;
using System.Collections.Generic;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.StepFlow
{
    public sealed class PreviousStepCondition : IStepFlowCondition
    {
        private readonly IStateStore _stateStore;
        private readonly string _expectedPreviousStepId;
        private IDisposable _subscription;

        public PreviousStepCondition(IStateStore stateStore, string expectedPreviousStepId)
        {
            _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
            _expectedPreviousStepId = expectedPreviousStepId ?? throw new ArgumentNullException(nameof(expectedPreviousStepId));
        }

        public Observable<bool> Evaluate()
        {
            return Observable.EveryUpdate()
                .Select(_ => IsPreviousStep())
                .DistinctUntilChanged();
        }

        public void Reset() { }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
        }

        private bool IsPreviousStep()
        {
            var history = _stateStore.GetHistory();
            if (history.Count == 0)
                return false;

            var lastStepId = history[history.Count - 1];
            return string.Equals(lastStepId, _expectedPreviousStepId, StringComparison.Ordinal);
        }
    }
}
