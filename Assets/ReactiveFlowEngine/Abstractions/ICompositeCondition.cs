using System.Collections.Generic;

namespace ReactiveFlowEngine.Abstractions
{
    public interface ICompositeCondition : ICondition
    {
        IReadOnlyList<ICondition> Children { get; }
    }
}
