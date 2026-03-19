using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class WaitUntilConditionBehavior : IBehavior
    {
        private readonly ICondition _condition;
        private readonly float _timeout;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public WaitUntilConditionBehavior(
            ICondition condition,
            float timeout = 0f,
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _condition = condition;
            _timeout = timeout;
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public async UniTask ExecuteAsync(CancellationToken ct)
        {
            if (_condition == null) return;

            var conditionMet = false;
            IDisposable subscription = null;

            try
            {
                subscription = _condition.Evaluate().Subscribe(value =>
                {
                    conditionMet = value;
                });

                if (_timeout > 0f)
                {
                    var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    var waitTask = UniTask.WaitUntil(() => conditionMet || ct.IsCancellationRequested, cancellationToken: ct);
                    var timeoutTask = UniTask.Delay(TimeSpan.FromSeconds(_timeout), cancellationToken: ct);

                    var result = await UniTask.WhenAny(waitTask, timeoutTask);

                    timeoutCts.Dispose();
                }
                else
                {
                    await UniTask.WaitUntil(() => conditionMet || ct.IsCancellationRequested, cancellationToken: ct);
                }

                ct.ThrowIfCancellationRequested();
            }
            finally
            {
                subscription?.Dispose();
            }
        }
    }
}
