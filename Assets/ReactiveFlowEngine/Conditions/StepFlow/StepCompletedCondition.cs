using System;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.StepFlow
{
    public sealed class StepCompletedCondition : IStepFlowCondition
    {
        private readonly IStateStore _stateStore;
        private readonly string _stepId;
        private IDisposable _subscription;

        public StepCompletedCondition(IStateStore stateStore, string stepId)
        {
            _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
            _stepId = stepId ?? throw new ArgumentNullException(nameof(stepId));
        }

        public Observable<bool> Evaluate()
        {
            return Observable.EveryUpdate()
                .Select(_ => IsStepCompleted())
                .DistinctUntilChanged();
        }

        public void Reset() { }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
        }

        private bool IsStepCompleted()
        {
            var history = _stateStore.GetHistory();
            for (int i = 0; i < history.Count; i++)
            {
                if (string.Equals(history[i], _stepId, StringComparison.Ordinal))
                    return true;
            }
            return false;
        }
    }
}
