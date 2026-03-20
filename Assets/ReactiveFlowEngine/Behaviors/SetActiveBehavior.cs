using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class SetActiveBehavior : IReversibleBehavior, IStateCaptureBehavior
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly string _targetGuid;
        private readonly bool _targetActiveState;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        private bool _wasActive;
        private bool _hasOriginalState;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public SetActiveBehavior(
            ISceneObjectResolver resolver,
            string targetGuid,
            bool targetActiveState,
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _resolver = resolver;
            _targetGuid = targetGuid;
            _targetActiveState = targetActiveState;
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public UniTask ExecuteAsync(CancellationToken ct)
        {
            if (_resolver == null)
            {
                Debug.LogWarning($"[RFE] SetActiveBehavior: SceneObjectResolver is null, skipping.");
                return UniTask.CompletedTask;
            }

            var target = _resolver.Resolve(_targetGuid);
            if (target == null)
            {
                Debug.LogWarning($"[RFE] SetActiveBehavior: Target object '{_targetGuid}' not found.");
                return UniTask.CompletedTask;
            }

            _wasActive = target.gameObject.activeSelf;
            _hasOriginalState = true;

            target.gameObject.SetActive(_targetActiveState);
            return UniTask.CompletedTask;
        }

        public UniTask UndoAsync(CancellationToken ct)
        {
            if (_resolver == null || !_hasOriginalState) return UniTask.CompletedTask;

            var target = _resolver.Resolve(_targetGuid);
            if (target == null) return UniTask.CompletedTask;

            target.gameObject.SetActive(_wasActive);
            return UniTask.CompletedTask;
        }

        public Dictionary<string, object> CaptureState()
        {
            return new Dictionary<string, object>
            {
                ["WasActive"] = _wasActive,
                ["HasOriginalState"] = _hasOriginalState,
                ["TargetGuid"] = _targetGuid,
                ["TargetActiveState"] = _targetActiveState
            };
        }
    }
}
