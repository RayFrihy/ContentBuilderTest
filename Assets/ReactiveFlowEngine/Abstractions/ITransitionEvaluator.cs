using System.Collections.Generic;
using R3;

namespace ReactiveFlowEngine.Abstractions
{
    public interface ITransitionEvaluator
    {
        Observable<ITransition> Evaluate(IReadOnlyList<ITransition> transitions);
    }
}
