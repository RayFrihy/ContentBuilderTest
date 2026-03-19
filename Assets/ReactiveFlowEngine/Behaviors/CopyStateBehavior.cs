using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class CopyStateBehavior : IReversibleBehavior, IStateCaptureBehavior
    {
        private readonly IStateStore _stateStore;
        private readonly string _sourceKey;
        private readonly string _destinationKey;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        private object _previousDestValue;
        private bool _hadPreviousDestValue;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public CopyStateBehavior(
            IStateStore stateStore,
            string sourceKey,
            string destinationKey,
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _stateStore = stateStore;
            _sourceKey = sourceKey;
            _destinationKey = destinationKey;
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public async UniTask ExecuteAsync(CancellationToken ct)
        {
            _previousDestValue = _stateStore.GetGlobalState(_destinationKey);
            _hadPreviousDestValue = _stateStore.HasGlobalState(_destinationKey);
            _stateStore.SetGlobalState(_destinationKey, _stateStore.GetGlobalState(_sourceKey));
            await UniTask.CompletedTask;
        }

        public async UniTask UndoAsync(CancellationToken ct)
        {
            if (_hadPreviousDestValue)
            {
                _stateStore.SetGlobalState(_destinationKey, _previousDestValue);
            }
            else
            {
                _stateStore.RemoveGlobalState(_destinationKey);
            }
            await UniTask.CompletedTask;
        }

        public Dictionary<string, object> CaptureState()
        {
            return new Dictionary<string, object>
            {
                ["SourceKey"] = _sourceKey,
                ["DestinationKey"] = _destinationKey,
                ["PreviousDestValue"] = _previousDestValue,
                ["HadPreviousDestValue"] = _hadPreviousDestValue
            };
        }
    }
}
