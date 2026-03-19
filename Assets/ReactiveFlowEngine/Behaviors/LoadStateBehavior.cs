using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class LoadStateBehavior : IReversibleBehavior, IStateCaptureBehavior
    {
        private readonly IStateStore _stateStore;
        private readonly string _snapshotKey;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        private Dictionary<string, object> _preLoadState;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public LoadStateBehavior(
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
            _preLoadState = _stateStore.GetAllGlobalState();
            var snapshot = _stateStore.GetGlobalState(_snapshotKey) as Dictionary<string, object>;
            if (snapshot != null)
            {
                _stateStore.SetAllGlobalState(snapshot);
            }
            else
            {
                Debug.LogWarning($"[RFE] LoadState: Snapshot '{_snapshotKey}' not found.");
            }
            await UniTask.CompletedTask;
        }

        public async UniTask UndoAsync(CancellationToken ct)
        {
            if (_preLoadState != null)
            {
                _stateStore.SetAllGlobalState(_preLoadState);
            }
            await UniTask.CompletedTask;
        }

        public Dictionary<string, object> CaptureState()
        {
            return new Dictionary<string, object>
            {
                ["SnapshotKey"] = _snapshotKey,
                ["PreLoadState"] = _preLoadState
            };
        }
    }
}
