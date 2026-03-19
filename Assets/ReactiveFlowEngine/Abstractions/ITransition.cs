using System.Collections.Generic;

namespace ReactiveFlowEngine.Abstractions
{
    public interface ITransition
    {
        int Priority { get; }
        IReadOnlyList<ICondition> Conditions { get; }
        IStep TargetStep { get; }
        bool IsUnconditional { get; }
    }
}
