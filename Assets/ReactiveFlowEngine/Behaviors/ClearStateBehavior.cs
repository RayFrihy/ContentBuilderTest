using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class ClearStateBehavior : IReversibleBehavior, IStateCaptureBehavior
    {
        private readonly IStateStore _stateStore;
        private readonly string _key;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        private Dictionary<string, object> _stateSnapshot;
        private object _previousValue;
        private bool _hadPreviousValue;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public ClearStateBehavior(
            IStateStore stateStore,
            string key = null,
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
            if (_key == null)
            {
                _stateSnapshot = _stateStore.GetAllGlobalState();
                _stateStore.SetAllGlobalState(new Dictionary<string, object>());
            }
            else
            {
                _previousValue = _stateStore.GetGlobalState(_key);
                _hadPreviousValue = _stateStore.HasGlobalState(_key);
                _stateStore.RemoveGlobalState(_key);
            }
            return UniTask.CompletedTask;
        }

        public UniTask UndoAsync(CancellationToken ct)
        {
            if (_key == null)
            {
                if (_stateSnapshot != null)
                {
                    _stateStore.SetAllGlobalState(_stateSnapshot);
                }
            }
            else
            {
                if (_hadPreviousValue)
                {
                    _stateStore.SetGlobalState(_key, _previousValue);
                }
            }
            return UniTask.CompletedTask;
        }

        public Dictionary<string, object> CaptureState()
        {
            return new Dictionary<string, object>
            {
                ["Key"] = _key,
                ["PreviousValue"] = _previousValue,
                ["HadPreviousValue"] = _hadPreviousValue,
                ["StateSnapshot"] = _stateSnapshot
            };
        }
    }
}
