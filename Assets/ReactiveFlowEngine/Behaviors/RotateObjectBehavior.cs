using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class RotateObjectBehavior : IReversibleBehavior, IStateCaptureBehavior
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly string _targetGuid;
        private readonly Vector3 _targetEulerAngles;
        private readonly float _duration;
        private readonly AnimationCurve _curve;
        private readonly bool _useLocalRotation;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        private Quaternion _originalRotation;
        private bool _hasOriginalState;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public RotateObjectBehavior(
            ISceneObjectResolver resolver,
            string targetGuid,
            Vector3 targetEulerAngles,
            float duration,
            AnimationCurve curve,
            bool useLocalRotation = false,
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _resolver = resolver;
            _targetGuid = targetGuid;
            _targetEulerAngles = targetEulerAngles;
            _duration = duration;
            _curve = curve ?? AnimationCurve.Linear(0f, 0f, 1f, 1f);
            _useLocalRotation = useLocalRotation;
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public async UniTask ExecuteAsync(CancellationToken ct)
        {
            if (_resolver == null) return;

            var target = _resolver.Resolve(_targetGuid);
            if (target == null) return;

            _originalRotation = _useLocalRotation ? target.localRotation : target.rotation;
            _hasOriginalState = true;

            var startRot = _useLocalRotation ? target.localRotation : target.rotation;
            var endRot = Quaternion.Euler(_targetEulerAngles);
            float elapsed = 0f;

            while (elapsed < _duration)
            {
                ct.ThrowIfCancellationRequested();

                elapsed += Time.deltaTime;
                float t = _curve.Evaluate(Mathf.Clamp01(elapsed / _duration));

                if (_useLocalRotation)
                    target.localRotation = Quaternion.Slerp(startRot, endRot, t);
                else
                    target.rotation = Quaternion.Slerp(startRot, endRot, t);

                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            if (_useLocalRotation)
                target.localRotation = endRot;
            else
                target.rotation = endRot;
        }

        public async UniTask UndoAsync(CancellationToken ct)
        {
            if (_resolver == null || !_hasOriginalState) return;

            var target = _resolver.Resolve(_targetGuid);
            if (target == null) return;

            if (_useLocalRotation)
                target.localRotation = _originalRotation;
            else
                target.rotation = _originalRotation;

            await UniTask.CompletedTask;
        }

        public Dictionary<string, object> CaptureState()
        {
            return new Dictionary<string, object>
            {
                ["OriginalRotation"] = _originalRotation,
                ["HasOriginalState"] = _hasOriginalState,
                ["TargetGuid"] = _targetGuid,
                ["Duration"] = _duration
            };
        }
    }
}
