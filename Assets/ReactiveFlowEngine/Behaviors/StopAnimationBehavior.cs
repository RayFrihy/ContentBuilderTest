using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class StopAnimationBehavior : IBehavior
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly string _targetGuid;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        private bool _wasEnabled;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public StopAnimationBehavior(
            ISceneObjectResolver resolver,
            string targetGuid,
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _resolver = resolver;
            _targetGuid = targetGuid;
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public async UniTask ExecuteAsync(CancellationToken ct)
        {
            if (_resolver == null) return;

            var target = _resolver.Resolve(_targetGuid);
            if (target == null) return;

            var animator = target.GetComponent<Animator>();
            if (animator == null) return;

            _wasEnabled = animator.enabled;
            animator.enabled = false;

            await UniTask.CompletedTask;
        }
    }
}
