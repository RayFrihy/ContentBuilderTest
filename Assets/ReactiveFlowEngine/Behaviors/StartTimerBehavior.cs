using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class StartTimerBehavior : IReversibleBehavior, IStateCaptureBehavior
    {
        private readonly IStateStore _stateStore;
        private readonly string _timerName;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        private bool _previousStartExists;
        private object _previousStart;
        private bool _previousRunningExists;
        private object _previousRunning;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public StartTimerBehavior(
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

            _previousStartExists = _stateStore.HasGlobalState(startKey);
            _previousStart = _previousStartExists ? _stateStore.GetGlobalState(startKey) : null;
            _previousRunningExists = _stateStore.HasGlobalState(runningKey);
            _previousRunning = _previousRunningExists ? _stateStore.GetGlobalState(runningKey) : null;

            _stateStore.SetGlobalState(startKey, Time.realtimeSinceStartup);
            _stateStore.SetGlobalState(runningKey, true);

            return UniTask.CompletedTask;
        }

        public UniTask UndoAsync(CancellationToken ct)
        {
            if (_stateStore == null) return UniTask.CompletedTask;

            var startKey = $"Timer_{_timerName}_Start";
            var runningKey = $"Timer_{_timerName}_Running";

            if (_previousStartExists)
                _stateStore.SetGlobalState(startKey, _previousStart);
            else
                _stateStore.RemoveGlobalState(startKey);

            if (_previousRunningExists)
                _stateStore.SetGlobalState(runningKey, _previousRunning);
            else
                _stateStore.RemoveGlobalState(runningKey);

            return UniTask.CompletedTask;
        }

        public Dictionary<string, object> CaptureState()
        {
            return new Dictionary<string, object>
            {
                ["TimerName"] = _timerName,
                ["PreviousStartExists"] = _previousStartExists,
                ["PreviousStart"] = _previousStart,
                ["PreviousRunningExists"] = _previousRunningExists,
                ["PreviousRunning"] = _previousRunning
            };
        }
    }
}
