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

        public UniTask ExecuteAsync(CancellationToken ct)
        {
            if (_resolver == null)
            {
                UnityEngine.Debug.LogWarning($"[RFE] StopAnimationBehavior: SceneObjectResolver is null, skipping.");
                return UniTask.CompletedTask;
            }

            var target = _resolver.Resolve(_targetGuid);
            if (target == null)
            {
                UnityEngine.Debug.LogWarning($"[RFE] StopAnimationBehavior: Target object '{_targetGuid}' not found.");
                return UniTask.CompletedTask;
            }

            var animator = target.GetComponent<Animator>();
            if (animator == null) return UniTask.CompletedTask;

            _wasEnabled = animator.enabled;
            _hasOriginalState = true;
            animator.enabled = false;

            return UniTask.CompletedTask;
        }

        public UniTask UndoAsync(CancellationToken ct)
        {
            if (_resolver == null || !_hasOriginalState) return UniTask.CompletedTask;

            var target = _resolver.Resolve(_targetGuid);
            if (target == null) return UniTask.CompletedTask;

            var animator = target.GetComponent<Animator>();
            if (animator == null) return UniTask.CompletedTask;

            animator.enabled = _wasEnabled;
            return UniTask.CompletedTask;
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
