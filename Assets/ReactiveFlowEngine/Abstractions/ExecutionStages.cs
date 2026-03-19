using System;

namespace ReactiveFlowEngine.Abstractions
{
    [Flags]
    public enum ExecutionStages
    {
        None = 0,
        Activation = 1,
        Deactivation = 2
    }
}
