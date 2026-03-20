using System;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.StepFlow
{
    public sealed class PreviousStepCondition : ICondition
    {
        private readonly IHistoryService _historyService;
        private readonly string _expectedPreviousStepId;

        public PreviousStepCondition(IHistoryService historyService, string expectedPreviousStepId)
        {
            _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
            _expectedPreviousStepId = expectedPreviousStepId ?? throw new ArgumentNullException(nameof(expectedPreviousStepId));
        }

        public Observable<bool> Evaluate()
        {
            return Observable.EveryUpdate()
                .Select(_ => IsPreviousStep())
                .DistinctUntilChanged();
        }

        public void Reset() { }

        public void Dispose() { }

        private bool IsPreviousStep()
        {
            var entries = _historyService.GetAll();
            if (entries.Count == 0)
                return false;

            var lastEntry = entries[entries.Count - 1];
            return string.Equals(lastEntry.StepId, _expectedPreviousStepId, StringComparison.Ordinal);
        }
    }
}
