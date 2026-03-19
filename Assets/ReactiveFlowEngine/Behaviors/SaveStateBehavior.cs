using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class SaveStateBehavior : IReversibleBehavior, IStateCaptureBehavior
    {
        private readonly IStateStore _stateStore;
        private readonly string _snapshotKey;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public SaveStateBehavior(
            IStateStore stateStore,
            string snapshotKey,
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _stateStore = stateStore;
            _snapshotKey = snapshotKey;
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public async UniTask ExecuteAsync(CancellationToken ct)
        {
            var snapshot = _stateStore.GetAllGlobalState();
            _stateStore.SetGlobalState(_snapshotKey, snapshot);
            await UniTask.CompletedTask;
        }

        public async UniTask UndoAsync(CancellationToken ct)
        {
            _stateStore.RemoveGlobalState(_snapshotKey);
            await UniTask.CompletedTask;
        }

        public Dictionary<string, object> CaptureState()
        {
            return new Dictionary<string, object>
            {
                ["SnapshotKey"] = _snapshotKey
            };
        }
    }
}
