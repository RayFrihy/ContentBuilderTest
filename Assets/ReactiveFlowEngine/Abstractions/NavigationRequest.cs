namespace ReactiveFlowEngine.Abstractions
{
    public sealed class NavigationRequest
    {
        public NavigationType Type { get; }
        public string TargetStepId { get; }
        public string TargetChapterId { get; }

        public NavigationRequest(NavigationType type, string targetStepId = null, string targetChapterId = null)
        {
            Type = type;
            TargetStepId = targetStepId;
            TargetChapterId = targetChapterId;
        }
    }
}
