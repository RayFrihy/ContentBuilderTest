using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class SkipStepBehavior : IBehavior
    {
        private readonly INavigationService _navigationService;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public SkipStepBehavior(
            INavigationService navigationService,
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _navigationService = navigationService;
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public async UniTask ExecuteAsync(CancellationToken ct)
        {
            await _navigationService.NextStepAsync(ct);
        }
    }
}
