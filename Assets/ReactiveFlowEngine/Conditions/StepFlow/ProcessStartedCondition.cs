using System;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.StepFlow
{
    public sealed class ProcessStartedCondition : ICondition
    {
        private readonly IFlowEngine _flowEngine;

        public ProcessStartedCondition(IFlowEngine flowEngine)
        {
            _flowEngine = flowEngine ?? throw new ArgumentNullException(nameof(flowEngine));
        }

        public Observable<bool> Evaluate()
        {
            return _flowEngine.State
                .Select(state => state != EngineState.Idle);
        }

        public void Reset() { }

        public void Dispose() { }
    }
}
