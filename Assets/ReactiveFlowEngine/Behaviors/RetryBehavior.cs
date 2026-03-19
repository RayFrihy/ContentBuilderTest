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
    public class RetryBehavior : IBehavior
    {
        private readonly IBehavior _child;
        private readonly int _maxRetries;
        private readonly float _retryDelay;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public RetryBehavior(
            IBehavior child,
            int maxRetries = 3,
            float retryDelay = 0f,
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _child = child;
            _maxRetries = maxRetries;
            _retryDelay = retryDelay;
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public async UniTask ExecuteAsync(CancellationToken ct)
        {
            int attempts = 0;

            while (attempts <= _maxRetries)
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    await _child.ExecuteAsync(ct);
                    return;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    attempts++;

                    if (attempts > _maxRetries)
                    {
                        Debug.LogError($"[RFE] RetryBehavior: All {_maxRetries} retries failed. Last error: {ex.Message}");
                        throw;
                    }

                    Debug.LogWarning($"[RFE] RetryBehavior: Attempt {attempts} failed: {ex.Message}. Retrying...");

                    if (_retryDelay > 0f)
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(_retryDelay), cancellationToken: ct);
                    }
                }
            }
        }
    }
}
