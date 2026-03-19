using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class BranchBehavior : IReversibleBehavior
    {
        private readonly IStateStore _stateStore;
        private readonly string _conditionKey;
        private readonly IBehavior _trueBranch;
        private readonly IBehavior _falseBranch;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        private bool _executedTrueBranch;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public BranchBehavior(
            IStateStore stateStore,
            string conditionKey,
            IBehavior trueBranch,
            IBehavior falseBranch = null,
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _stateStore = stateStore;
            _conditionKey = conditionKey;
            _trueBranch = trueBranch;
            _falseBranch = falseBranch;
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public async UniTask ExecuteAsync(CancellationToken ct)
        {
            var value = _stateStore.GetGlobalState(_conditionKey);
            bool condition = value is bool b ? b : value != null;
            _executedTrueBranch = condition;

            if (condition && _trueBranch != null)
                await _trueBranch.ExecuteAsync(ct);
            else if (!condition && _falseBranch != null)
                await _falseBranch.ExecuteAsync(ct);
        }

        public async UniTask UndoAsync(CancellationToken ct)
        {
            if (_executedTrueBranch && _trueBranch is IReversibleBehavior r)
                await r.UndoAsync(ct);
            else if (!_executedTrueBranch && _falseBranch is IReversibleBehavior r2)
                await r2.UndoAsync(ct);
        }
    }
}
