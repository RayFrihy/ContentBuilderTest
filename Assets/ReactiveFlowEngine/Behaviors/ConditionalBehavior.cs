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
    public class ConditionalBehavior : IReversibleBehavior
    {
        private readonly IStateStore _stateStore;
        private readonly string _conditionKey;
        private readonly IBehavior _child;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        private bool _didExecute;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public ConditionalBehavior(
            IStateStore stateStore,
            string conditionKey,
            IBehavior child,
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _stateStore = stateStore;
            _conditionKey = conditionKey;
            _child = child;
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public async UniTask ExecuteAsync(CancellationToken ct)
        {
            var value = _stateStore.GetGlobalState(_conditionKey);
            bool shouldRun = value is bool b ? b : value != null;

            if (shouldRun)
            {
                await _child.ExecuteAsync(ct);
                _didExecute = true;
            }
        }

        public async UniTask UndoAsync(CancellationToken ct)
        {
            if (_didExecute && _child is IReversibleBehavior r)
            {
                await r.UndoAsync(ct);
            }
        }
    }
}
