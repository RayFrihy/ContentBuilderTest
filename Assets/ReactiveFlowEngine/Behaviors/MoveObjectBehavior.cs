using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class MoveObjectBehavior : IReversibleBehavior, IStateCaptureBehavior
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly string _targetGuid;
        private readonly string _destinationGuid;
        private readonly float _duration;
        private readonly AnimationCurve _curve;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        private Vector3 _originalPosition;
        private Quaternion _originalRotation;
        private bool _hasOriginalState;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public MoveObjectBehavior(
            ISceneObjectResolver resolver,
            string targetGuid,
            string destinationGuid,
            float duration,
            AnimationCurve curve,
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _resolver = resolver;
            _targetGuid = targetGuid;
            _destinationGuid = destinationGuid;
            _duration = duration;
            _curve = curve ?? AnimationCurve.Linear(0f, 0f, 1f, 1f);
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public async UniTask ExecuteAsync(CancellationToken ct)
        {
            if (_resolver == null)
            {
                UnityEngine.Debug.LogWarning($"[RFE] MoveObjectBehavior: SceneObjectResolver is null, skipping.");
                return;
            }

            var target = _resolver.Resolve(_targetGuid);
            var destination = _resolver.Resolve(_destinationGuid);
            if (target == null || destination == null)
            {
                UnityEngine.Debug.LogWarning($"[RFE] MoveObjectBehavior: Target '{_targetGuid}' or destination '{_destinationGuid}' not found.");
                return;
            }

            _originalPosition = target.position;
            _originalRotation = target.rotation;
            _hasOriginalState = true;

            var startPos = target.position;
            var endPos = destination.position;
            var startRot = target.rotation;
            var endRot = destination.rotation;
            float elapsed = 0f;

            while (elapsed < _duration)
            {
                ct.ThrowIfCancellationRequested();

                elapsed += Time.deltaTime;
                float t = _curve.Evaluate(Mathf.Clamp01(elapsed / _duration));
                target.position = Vector3.Lerp(startPos, endPos, t);
                target.rotation = Quaternion.Slerp(startRot, endRot, t);

                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            target.position = endPos;
            target.rotation = endRot;
        }

        public UniTask UndoAsync(CancellationToken ct)
        {
            if (_resolver == null || !_hasOriginalState) return UniTask.CompletedTask;

            var target = _resolver.Resolve(_targetGuid);
            if (target == null) return UniTask.CompletedTask;

            target.position = _originalPosition;
            target.rotation = _originalRotation;
            return UniTask.CompletedTask;
        }

        public Dictionary<string, object> CaptureState()
        {
            return new Dictionary<string, object>
            {
                ["OriginalPosition"] = _originalPosition,
                ["OriginalRotation"] = _originalRotation,
                ["HasOriginalState"] = _hasOriginalState,
                ["TargetGuid"] = _targetGuid,
                ["DestinationGuid"] = _destinationGuid,
                ["Duration"] = _duration
            };
        }
    }
}
