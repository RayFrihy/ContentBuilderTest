namespace ReactiveFlowEngine.Abstractions
{
    public interface IStateCondition : ICondition
    {
        string StateKey { get; }
    }
}
