using System.Collections.Generic;

namespace ReactiveFlowEngine.Abstractions
{
    public interface IStateCaptureBehavior : IBehavior
    {
        Dictionary<string, object> CaptureState();
    }
}
