namespace ReactiveFlowEngine.Abstractions
{
    public enum StepStatus
    {
        Idle,
        Entering,
        Executing,
        Evaluating,
        Exiting,
        Completed,
        Cancelled
    }
}
