using System;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.StepFlow
{
    public sealed class TransitionAvailableCondition : ICondition
    {
        private readonly IFlowEngine _flowEngine;
        private readonly string _targetStepId;

        public TransitionAvailableCondition(IFlowEngine flowEngine, string targetStepId)
        {
            _flowEngine = flowEngine ?? throw new ArgumentNullException(nameof(flowEngine));
            _targetStepId = targetStepId ?? throw new ArgumentNullException(nameof(targetStepId));
        }

        public Observable<bool> Evaluate()
        {
            return _flowEngine.CurrentStep
                .Select(step => HasTransitionToTarget(step));
        }

        public void Reset() { }

        public void Dispose() { }

        private bool HasTransitionToTarget(IStep currentStep)
        {
            if (currentStep?.Transitions == null)
                return false;

            for (int i = 0; i < currentStep.Transitions.Count; i++)
            {
                var transition = currentStep.Transitions[i];
                if (transition?.TargetStep != null &&
                    string.Equals(transition.TargetStep.Id, _targetStepId, StringComparison.Ordinal))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
