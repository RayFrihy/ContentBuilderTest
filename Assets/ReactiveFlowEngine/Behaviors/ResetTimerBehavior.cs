using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class ResetTimerBehavior : IReversibleBehavior, IStateCaptureBehavior
    {
        private readonly IStateStore _stateStore;
        private readonly string _timerName;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        private bool _previousStartExists;
        private object _previousStart;
        private bool _previousElapsedExists;
        private object _previousElapsed;
        private bool _previousRunningExists;
        private object _previousRunning;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public ResetTimerBehavior(
            IStateStore stateStore,
            string timerName,
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _stateStore = stateStore;
            _timerName = timerName;
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public async UniTask ExecuteAsync(CancellationToken ct)
        {
            if (_stateStore == null) return;

            var startKey = $"Timer_{_timerName}_Start";
            var elapsedKey = $"Timer_{_timerName}_Elapsed";
            var runningKey = $"Timer_{_timerName}_Running";

            _previousStartExists = _stateStore.HasGlobalState(startKey);
            _previousStart = _previousStartExists ? _stateStore.GetGlobalState(startKey) : null;
            _previousElapsedExists = _stateStore.HasGlobalState(elapsedKey);
            _previousElapsed = _previousElapsedExists ? _stateStore.GetGlobalState(elapsedKey) : null;
            _previousRunningExists = _stateStore.HasGlobalState(runningKey);
            _previousRunning = _previousRunningExists ? _stateStore.GetGlobalState(runningKey) : null;

            _stateStore.SetGlobalState(startKey, Time.realtimeSinceStartup);

            if (_stateStore.HasGlobalState(elapsedKey))
            {
                _stateStore.RemoveGlobalState(elapsedKey);
            }

            await UniTask.CompletedTask;
        }

        public async UniTask UndoAsync(CancellationToken ct)
        {
            if (_stateStore == null) return;

            var startKey = $"Timer_{_timerName}_Start";
            var elapsedKey = $"Timer_{_timerName}_Elapsed";
            var runningKey = $"Timer_{_timerName}_Running";

            if (_previousStartExists)
                _stateStore.SetGlobalState(startKey, _previousStart);
            else
                _stateStore.RemoveGlobalState(startKey);

            if (_previousElapsedExists)
                _stateStore.SetGlobalState(elapsedKey, _previousElapsed);
            else
                _stateStore.RemoveGlobalState(elapsedKey);

            if (_previousRunningExists)
                _stateStore.SetGlobalState(runningKey, _previousRunning);
            else
                _stateStore.RemoveGlobalState(runningKey);

            await UniTask.CompletedTask;
        }

        public Dictionary<string, object> CaptureState()
        {
            return new Dictionary<string, object>
            {
                ["TimerName"] = _timerName,
                ["PreviousStartExists"] = _previousStartExists,
                ["PreviousStart"] = _previousStart,
                ["PreviousElapsedExists"] = _previousElapsedExists,
                ["PreviousElapsed"] = _previousElapsed,
                ["PreviousRunningExists"] = _previousRunningExists,
                ["PreviousRunning"] = _previousRunning
            };
        }
    }
}
