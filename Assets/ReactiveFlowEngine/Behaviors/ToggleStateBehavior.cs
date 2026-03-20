using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class ToggleStateBehavior : IReversibleBehavior, IStateCaptureBehavior
    {
        private readonly IStateStore _stateStore;
        private readonly string _key;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        private bool _originalBoolValue;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public ToggleStateBehavior(
            IStateStore stateStore,
            string key,
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _stateStore = stateStore;
            _key = key;
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public UniTask ExecuteAsync(CancellationToken ct)
        {
            var current = _stateStore.GetGlobalState(_key);
            _originalBoolValue = current is bool b ? b : false;
            _stateStore.SetGlobalState(_key, !_originalBoolValue);
            return UniTask.CompletedTask;
        }

        public UniTask UndoAsync(CancellationToken ct)
        {
            _stateStore.SetGlobalState(_key, _originalBoolValue);
            return UniTask.CompletedTask;
        }

        public Dictionary<string, object> CaptureState()
        {
            return new Dictionary<string, object>
            {
                ["Key"] = _key,
                ["OriginalBoolValue"] = _originalBoolValue
            };
        }
    }
}
