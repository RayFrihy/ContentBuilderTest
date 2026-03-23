using System;
using System.Collections.Generic;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Tests.TestDoubles
{
    public class MockStepGuidanceService : IStepGuidanceService
    {
        private readonly ReactiveProperty<IReadOnlyList<string>> _targetObjectIds
            = new ReactiveProperty<IReadOnlyList<string>>(Array.Empty<string>() as IReadOnlyList<string>);

        public ReadOnlyReactiveProperty<IReadOnlyList<string>> CurrentTargetObjectIds => _targetObjectIds;
        public int EnableCount { get; private set; }
        public int DisableCount { get; private set; }

        public void Enable() => EnableCount++;
        public void Disable() => DisableCount++;
        public void SetTargets(IReadOnlyList<string> ids) => _targetObjectIds.Value = ids;
        public void Dispose() => _targetObjectIds.Dispose();
    }
}
