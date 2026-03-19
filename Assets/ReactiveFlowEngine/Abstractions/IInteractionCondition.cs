using R3;

namespace ReactiveFlowEngine.Abstractions
{
    public interface IInteractionCondition : ICondition
    {
        string TargetObjectId { get; }
    }
}
