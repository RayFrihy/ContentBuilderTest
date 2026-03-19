using System;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.StepFlow
{
    public sealed class StepActiveCondition : IStepFlowCondition
    {
        private readonly IFlowEngine _flowEngine;
        private readonly string _stepId;
        private IDisposable _subscription;

        public StepActiveCondition(IFlowEngine flowEngine, string stepId)
        {
            _flowEngine = flowEngine ?? throw new ArgumentNullException(nameof(flowEngine));
            _stepId = stepId ?? throw new ArgumentNullException(nameof(stepId));
        }

        public Observable<bool> Evaluate()
        {
            return _flowEngine.CurrentStep
                .Select(step => step != null && string.Equals(step.Id, _stepId, StringComparison.Ordinal));
        }

        public void Reset() { }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
        }
    }
}
