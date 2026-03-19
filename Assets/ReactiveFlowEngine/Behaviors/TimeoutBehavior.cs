using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class TimeoutBehavior : IBehavior
    {
        private readonly float _duration;
        private readonly string _warningMessage;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public TimeoutBehavior(
            float duration,
            string warningMessage = null,
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _duration = duration;
            _warningMessage = warningMessage;
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public async UniTask ExecuteAsync(CancellationToken ct)
        {
            if (_duration <= 0f) return;

            await UniTask.Delay(TimeSpan.FromSeconds(_duration), cancellationToken: ct);

            Debug.LogWarning($"[RFE] Timeout expired: {_warningMessage ?? "unnamed timeout"}");
        }
    }
}
