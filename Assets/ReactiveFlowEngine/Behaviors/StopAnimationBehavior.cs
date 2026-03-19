using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class StopAnimationBehavior : IReversibleBehavior, IStateCaptureBehavior
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly string _targetGuid;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        private bool _wasEnabled;
        private bool _hasOriginalState;

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
            _hasOriginalState = true;
            animator.enabled = false;

            await UniTask.CompletedTask;
        }

        public async UniTask UndoAsync(CancellationToken ct)
        {
            if (_resolver == null || !_hasOriginalState) return;

            var target = _resolver.Resolve(_targetGuid);
            if (target == null) return;

            var animator = target.GetComponent<Animator>();
            if (animator == null) return;

            animator.enabled = _wasEnabled;
            await UniTask.CompletedTask;
        }

        public Dictionary<string, object> CaptureState()
        {
            return new Dictionary<string, object>
            {
                ["TargetGuid"] = _targetGuid,
                ["WasEnabled"] = _wasEnabled,
                ["HasOriginalState"] = _hasOriginalState
            };
        }
    }
}
