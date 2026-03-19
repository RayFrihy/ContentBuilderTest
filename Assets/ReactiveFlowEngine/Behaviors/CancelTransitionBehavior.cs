using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class CancelTransitionBehavior : IBehavior
    {
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public CancelTransitionBehavior(
            bool isBlocking = false,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public async UniTask ExecuteAsync(CancellationToken ct)
        {
            await UniTask.CompletedTask;
        }
    }
}
