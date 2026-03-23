using System;
using System.Collections.Generic;
using R3;

namespace ReactiveFlowEngine.Abstractions
{
    public interface IStepGuidanceService : IDisposable
    {
        ReadOnlyReactiveProperty<IReadOnlyList<string>> CurrentTargetObjectIds { get; }
        void Enable();
        void Disable();
    }
}
