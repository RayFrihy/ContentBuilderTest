using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ReactiveFlowEngine.Behaviors
{
    public class DisablePhysicsBehavior : IReversibleBehavior
    {
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        private SimulationMode _previousMode;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public DisablePhysicsBehavior(
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public async UniTask ExecuteAsync(CancellationToken ct)
        {
            _previousMode = Physics.simulationMode;
            Physics.simulationMode = SimulationMode.Script;
            await UniTask.CompletedTask;
        }

        public async UniTask UndoAsync(CancellationToken ct)
        {
            Physics.simulationMode = _previousMode;
            await UniTask.CompletedTask;
        }
    }
}
