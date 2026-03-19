using System.Collections.Generic;

namespace ReactiveFlowEngine.Abstractions
{
    public interface IStep
    {
        string Id { get; }
        string Name { get; }
        StepType Type { get; }
        IReadOnlyList<IBehavior> Behaviors { get; }
        IReadOnlyList<ITransition> Transitions { get; }
        IChapter SubChapter { get; }
    }
}
