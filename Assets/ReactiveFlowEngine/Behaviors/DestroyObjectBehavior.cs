using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class DestroyObjectBehavior : IReversibleBehavior, IStateCaptureBehavior
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly string _targetGuid;
        private readonly float _delay;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        private Vector3 _lastKnownPosition;
        private Quaternion _lastKnownRotation;
        private Vector3 _lastKnownScale;
        private bool _hasCapturedState;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public DestroyObjectBehavior(
            ISceneObjectResolver resolver,
            string targetGuid,
            float delay = 0f,
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _resolver = resolver;
            _targetGuid = targetGuid;
            _delay = delay;
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public async UniTask ExecuteAsync(CancellationToken ct)
        {
            if (_resolver == null)
            {
                UnityEngine.Debug.LogWarning($"[RFE] DestroyObjectBehavior: SceneObjectResolver is null, skipping.");
                return;
            }

            var target = _resolver.Resolve(_targetGuid);
            if (target == null)
            {
                UnityEngine.Debug.LogWarning($"[RFE] DestroyObjectBehavior: Target object '{_targetGuid}' not found.");
                return;
            }

            _lastKnownPosition = target.position;
            _lastKnownRotation = target.rotation;
            _lastKnownScale = target.localScale;
            _hasCapturedState = true;

            Object.Destroy(target.gameObject, _delay);

            if (_delay > 0f)
            {
                await UniTask.Delay(
                    System.TimeSpan.FromSeconds(_delay),
                    cancellationToken: ct);
            }
        }

        public UniTask UndoAsync(CancellationToken ct)
        {
            if (!_hasCapturedState) return UniTask.CompletedTask;

            Debug.LogWarning($"[RFE] DestroyObjectBehavior: Cannot undo destruction of '{_targetGuid}'. Object state was captured at position {_lastKnownPosition}.");
            return UniTask.CompletedTask;
        }

        public Dictionary<string, object> CaptureState()
        {
            return new Dictionary<string, object>
            {
                ["TargetGuid"] = _targetGuid,
                ["LastKnownPosition"] = _lastKnownPosition,
                ["LastKnownRotation"] = _lastKnownRotation,
                ["LastKnownScale"] = _lastKnownScale,
                ["HasCapturedState"] = _hasCapturedState
            };
        }
    }
}
