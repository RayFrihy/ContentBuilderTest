using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class StopTimerBehavior : IReversibleBehavior, IStateCaptureBehavior
    {
        private readonly IStateStore _stateStore;
        private readonly string _timerName;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        private bool _previousRunningExists;
        private object _previousRunning;
        private bool _previousElapsedExists;
        private object _previousElapsed;
        private bool _previousStartExists;
        private object _previousStart;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public StopTimerBehavior(
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

        public UniTask ExecuteAsync(CancellationToken ct)
        {
            if (_stateStore == null) return UniTask.CompletedTask;

            var startKey = $"Timer_{_timerName}_Start";
            var runningKey = $"Timer_{_timerName}_Running";
            var elapsedKey = $"Timer_{_timerName}_Elapsed";

            _previousRunningExists = _stateStore.HasGlobalState(runningKey);
            _previousRunning = _previousRunningExists ? _stateStore.GetGlobalState(runningKey) : null;
            _previousElapsedExists = _stateStore.HasGlobalState(elapsedKey);
            _previousElapsed = _previousElapsedExists ? _stateStore.GetGlobalState(elapsedKey) : null;
            _previousStartExists = _stateStore.HasGlobalState(startKey);
            _previousStart = _previousStartExists ? _stateStore.GetGlobalState(startKey) : null;

            if (_stateStore.HasGlobalState(startKey))
            {
                var startTime = (float)_stateStore.GetGlobalState(startKey);
                var elapsed = Time.realtimeSinceStartup - startTime;
                _stateStore.SetGlobalState(elapsedKey, elapsed);
            }

            _stateStore.SetGlobalState(runningKey, false);

            return UniTask.CompletedTask;
        }

        public UniTask UndoAsync(CancellationToken ct)
        {
            if (_stateStore == null) return UniTask.CompletedTask;

            var startKey = $"Timer_{_timerName}_Start";
            var runningKey = $"Timer_{_timerName}_Running";
            var elapsedKey = $"Timer_{_timerName}_Elapsed";

            if (_previousRunningExists)
                _stateStore.SetGlobalState(runningKey, _previousRunning);
            else
                _stateStore.RemoveGlobalState(runningKey);

            if (_previousElapsedExists)
                _stateStore.SetGlobalState(elapsedKey, _previousElapsed);
            else
                _stateStore.RemoveGlobalState(elapsedKey);

            if (_previousStartExists)
                _stateStore.SetGlobalState(startKey, _previousStart);
            else
                _stateStore.RemoveGlobalState(startKey);

            return UniTask.CompletedTask;
        }

        public Dictionary<string, object> CaptureState()
        {
            return new Dictionary<string, object>
            {
                ["TimerName"] = _timerName,
                ["PreviousRunningExists"] = _previousRunningExists,
                ["PreviousRunning"] = _previousRunning,
                ["PreviousElapsedExists"] = _previousElapsedExists,
                ["PreviousElapsed"] = _previousElapsed,
                ["PreviousStartExists"] = _previousStartExists,
                ["PreviousStart"] = _previousStart
            };
        }
    }
}
