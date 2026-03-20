using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class IncrementStateBehavior : IReversibleBehavior, IStateCaptureBehavior
    {
        private readonly IStateStore _stateStore;
        private readonly string _key;
        private readonly float _amount;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        private float _previousFloatValue;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public IncrementStateBehavior(
            IStateStore stateStore,
            string key,
            float amount = 1f,
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _stateStore = stateStore;
            _key = key;
            _amount = amount;
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public UniTask ExecuteAsync(CancellationToken ct)
        {
            var current = _stateStore.GetGlobalState(_key);
            _previousFloatValue = current != null ? Convert.ToSingle(current) : 0f;
            float newValue = _previousFloatValue + _amount;
            _stateStore.SetGlobalState(_key, newValue);
            return UniTask.CompletedTask;
        }

        public UniTask UndoAsync(CancellationToken ct)
        {
            var current = _stateStore.GetGlobalState(_key);
            float currentValue = current != null ? Convert.ToSingle(current) : 0f;
            float restoredValue = currentValue - _amount;
            _stateStore.SetGlobalState(_key, restoredValue);
            return UniTask.CompletedTask;
        }

        public Dictionary<string, object> CaptureState()
        {
            return new Dictionary<string, object>
            {
                ["Key"] = _key,
                ["Amount"] = _amount,
                ["PreviousFloatValue"] = _previousFloatValue
            };
        }
    }
}
