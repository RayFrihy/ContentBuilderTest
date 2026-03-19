namespace ReactiveFlowEngine.Abstractions
{
    public interface ITimeBasedCondition : ICondition
    {
        float Duration { get; }
    }
}
