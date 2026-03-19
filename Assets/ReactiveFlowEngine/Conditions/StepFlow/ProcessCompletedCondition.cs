using System;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.StepFlow
{
    public sealed class ProcessCompletedCondition : IStepFlowCondition
    {
        private readonly IFlowEngine _flowEngine;
        private IDisposable _subscription;

        public ProcessCompletedCondition(IFlowEngine flowEngine)
        {
            _flowEngine = flowEngine ?? throw new ArgumentNullException(nameof(flowEngine));
        }

        public Observable<bool> Evaluate()
        {
            return _flowEngine.State
                .Select(state => state == EngineState.Completed);
        }

        public void Reset() { }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
        }
    }
}
