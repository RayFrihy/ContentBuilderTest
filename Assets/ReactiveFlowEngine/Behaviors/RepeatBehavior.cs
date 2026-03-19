using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ReactiveFlowEngine.Behaviors
{
    public class RepeatBehavior : IBehavior
    {
        private readonly IBehavior _child;
        private readonly int _count;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public RepeatBehavior(
            IBehavior child,
            int count,
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _child = child;
            _count = count;
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public async UniTask ExecuteAsync(CancellationToken ct)
        {
            for (int i = 0; i < _count; i++)
            {
                ct.ThrowIfCancellationRequested();
                await _child.ExecuteAsync(ct);
            }
        }
    }
}
