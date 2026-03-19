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
    public class LoopBehavior : IBehavior
    {
        private readonly IBehavior _child;
        private readonly IStateStore _stateStore;
        private readonly string _conditionKey;
        private readonly int _maxIterations;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public LoopBehavior(
            IBehavior child,
            IStateStore stateStore = null,
            string conditionKey = null,
            int maxIterations = 1000,
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _child = child;
            _stateStore = stateStore;
            _conditionKey = conditionKey;
            _maxIterations = maxIterations;
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public async UniTask ExecuteAsync(CancellationToken ct)
        {
            int iterations = 0;

            while (iterations < _maxIterations)
            {
                ct.ThrowIfCancellationRequested();

                if (_stateStore != null && _conditionKey != null)
                {
                    var val = _stateStore.GetGlobalState(_conditionKey);
                    bool shouldContinue = val is bool b ? b : val != null;
                    if (!shouldContinue) break;
                }

                await _child.ExecuteAsync(ct);
                iterations++;
            }

            if (iterations >= _maxIterations)
            {
                Debug.LogWarning("[RFE] LoopBehavior hit max iterations");
            }
        }
    }
}
