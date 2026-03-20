using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class ForceTransitionBehavior : IBehavior
    {
        private readonly IStepRunner _stepRunner;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public ForceTransitionBehavior(
            IStepRunner stepRunner,
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _stepRunner = stepRunner;
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public UniTask ExecuteAsync(CancellationToken ct)
        {
            _stepRunner.CancelCurrentStep();
            return UniTask.CompletedTask;
        }
    }
}
