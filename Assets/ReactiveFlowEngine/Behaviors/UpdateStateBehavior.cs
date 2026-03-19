using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class UpdateStateBehavior : IReversibleBehavior, IStateCaptureBehavior
    {
        private readonly IStateStore _stateStore;
        private readonly string _key;
        private readonly object _value;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        private object _previousValue;
        private bool _hadPreviousValue;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public UpdateStateBehavior(
            IStateStore stateStore,
            string key,
            object value,
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _stateStore = stateStore;
            _key = key;
            _value = value;
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public async UniTask ExecuteAsync(CancellationToken ct)
        {
            _previousValue = _stateStore.GetGlobalState(_key);
            _hadPreviousValue = _stateStore.HasGlobalState(_key);

            if (!_stateStore.HasGlobalState(_key))
            {
                Debug.LogWarning($"[RFE] UpdateState: Key '{_key}' does not exist, creating it.");
            }

            _stateStore.SetGlobalState(_key, _value);
            await UniTask.CompletedTask;
        }

        public async UniTask UndoAsync(CancellationToken ct)
        {
            if (_hadPreviousValue)
            {
                _stateStore.SetGlobalState(_key, _previousValue);
            }
            else
            {
                _stateStore.RemoveGlobalState(_key);
            }
            await UniTask.CompletedTask;
        }

        public Dictionary<string, object> CaptureState()
        {
            return new Dictionary<string, object>
            {
                ["Key"] = _key,
                ["Value"] = _value,
                ["PreviousValue"] = _previousValue,
                ["HadPreviousValue"] = _hadPreviousValue
            };
        }
    }
}
