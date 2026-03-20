using System;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.StepFlow
{
    public sealed class NextStepAvailableCondition : ICondition
    {
        private readonly IFlowEngine _flowEngine;

        public NextStepAvailableCondition(IFlowEngine flowEngine)
        {
            _flowEngine = flowEngine ?? throw new ArgumentNullException(nameof(flowEngine));
        }

        public Observable<bool> Evaluate()
        {
            return _flowEngine.CurrentStep
                .Select(step => HasNextStep(step));
        }

        public void Reset() { }

        public void Dispose() { }

        private bool HasNextStep(IStep currentStep)
        {
            if (currentStep == null)
                return false;

            return currentStep.Transitions != null && currentStep.Transitions.Count > 0;
        }
    }
}
