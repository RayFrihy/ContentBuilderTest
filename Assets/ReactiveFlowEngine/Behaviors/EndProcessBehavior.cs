using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class EndProcessBehavior : IBehavior
    {
        private readonly IFlowEngine _flowEngine;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public EndProcessBehavior(
            IFlowEngine flowEngine,
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _flowEngine = flowEngine;
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public async UniTask ExecuteAsync(CancellationToken ct)
        {
            await _flowEngine.StopAsync();
        }
    }
}
