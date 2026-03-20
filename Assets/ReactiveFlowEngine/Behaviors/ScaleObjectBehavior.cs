using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class ScaleObjectBehavior : IReversibleBehavior, IStateCaptureBehavior
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly string _targetGuid;
        private readonly Vector3 _targetScale;
        private readonly float _duration;
        private readonly AnimationCurve _curve;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        private Vector3 _originalScale;
        private bool _hasOriginalState;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public ScaleObjectBehavior(
            ISceneObjectResolver resolver,
            string targetGuid,
            Vector3 targetScale,
            float duration,
            AnimationCurve curve,
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _resolver = resolver;
            _targetGuid = targetGuid;
            _targetScale = targetScale;
            _duration = duration;
            _curve = curve ?? AnimationCurve.Linear(0f, 0f, 1f, 1f);
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public async UniTask ExecuteAsync(CancellationToken ct)
        {
            if (_resolver == null)
            {
                UnityEngine.Debug.LogWarning($"[RFE] ScaleObjectBehavior: SceneObjectResolver is null, skipping.");
                return;
            }

            var target = _resolver.Resolve(_targetGuid);
            if (target == null)
            {
                UnityEngine.Debug.LogWarning($"[RFE] ScaleObjectBehavior: Target object '{_targetGuid}' not found.");
                return;
            }

            _originalScale = target.localScale;
            _hasOriginalState = true;

            var startScale = target.localScale;
            var endScale = _targetScale;
            float elapsed = 0f;

            while (elapsed < _duration)
            {
                ct.ThrowIfCancellationRequested();

                elapsed += Time.deltaTime;
                float t = _curve.Evaluate(Mathf.Clamp01(elapsed / _duration));
                target.localScale = Vector3.Lerp(startScale, endScale, t);

                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            target.localScale = endScale;
        }

        public UniTask UndoAsync(CancellationToken ct)
        {
            if (_resolver == null || !_hasOriginalState) return UniTask.CompletedTask;

            var target = _resolver.Resolve(_targetGuid);
            if (target == null) return UniTask.CompletedTask;

            target.localScale = _originalScale;
            return UniTask.CompletedTask;
        }

        public Dictionary<string, object> CaptureState()
        {
            return new Dictionary<string, object>
            {
                ["OriginalScale"] = _originalScale,
                ["HasOriginalState"] = _hasOriginalState,
                ["TargetGuid"] = _targetGuid,
                ["Duration"] = _duration
            };
        }
    }
}
