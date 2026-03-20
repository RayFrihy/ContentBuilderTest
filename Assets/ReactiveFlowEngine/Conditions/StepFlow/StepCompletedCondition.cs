using System;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.StepFlow
{
    public sealed class StepCompletedCondition : ICondition
    {
        private readonly IHistoryService _historyService;
        private readonly string _stepId;

        public StepCompletedCondition(IHistoryService historyService, string stepId)
        {
            _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
            _stepId = stepId ?? throw new ArgumentNullException(nameof(stepId));
        }

        public Observable<bool> Evaluate()
        {
            return Observable.EveryUpdate()
                .Select(_ => IsStepCompleted())
                .DistinctUntilChanged();
        }

        public void Reset() { }

        public void Dispose() { }

        private bool IsStepCompleted()
        {
            return _historyService.Contains(_stepId);
        }
    }
}
