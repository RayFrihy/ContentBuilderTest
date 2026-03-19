using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class FadeObjectBehavior : IReversibleBehavior, IStateCaptureBehavior
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly string _targetGuid;
        private readonly float _targetAlpha;
        private readonly float _duration;
        private readonly AnimationCurve _curve;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        private List<(Renderer renderer, Color originalColor)> _originalStates;
        private bool _hasOriginalState;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public FadeObjectBehavior(
            ISceneObjectResolver resolver,
            string targetGuid,
            float targetAlpha,
            float duration,
            AnimationCurve curve = null,
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _resolver = resolver;
            _targetGuid = targetGuid;
            _targetAlpha = targetAlpha;
            _duration = duration;
            _curve = curve ?? AnimationCurve.Linear(0f, 0f, 1f, 1f);
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public async UniTask ExecuteAsync(CancellationToken ct)
        {
            if (_resolver == null) return;

            var target = _resolver.Resolve(_targetGuid);
            if (target == null) return;

            var renderers = target.GetComponentsInChildren<Renderer>();
            _originalStates = new List<(Renderer, Color)>(renderers.Length);

            foreach (var renderer in renderers)
            {
                if (renderer.material != null)
                {
                    _originalStates.Add((renderer, renderer.material.color));
                }
            }

            _hasOriginalState = true;

            var startAlphas = new float[_originalStates.Count];
            for (int i = 0; i < _originalStates.Count; i++)
            {
                startAlphas[i] = _originalStates[i].originalColor.a;
            }

            float elapsed = 0f;

            while (elapsed < _duration)
            {
                ct.ThrowIfCancellationRequested();

                elapsed += Time.deltaTime;
                float t = _curve.Evaluate(Mathf.Clamp01(elapsed / _duration));

                for (int i = 0; i < _originalStates.Count; i++)
                {
                    var (renderer, _) = _originalStates[i];
                    if (renderer == null || renderer.material == null) continue;

                    var color = renderer.material.color;
                    color.a = Mathf.Lerp(startAlphas[i], _targetAlpha, t);
                    renderer.material.color = color;
                }

                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            for (int i = 0; i < _originalStates.Count; i++)
            {
                var (renderer, _) = _originalStates[i];
                if (renderer == null || renderer.material == null) continue;

                var color = renderer.material.color;
                color.a = _targetAlpha;
                renderer.material.color = color;
            }
        }

        public async UniTask UndoAsync(CancellationToken ct)
        {
            if (_resolver == null || !_hasOriginalState) return;

            foreach (var (renderer, originalColor) in _originalStates)
            {
                ct.ThrowIfCancellationRequested();

                if (renderer == null || renderer.material == null) continue;
                renderer.material.color = originalColor;
            }

            await UniTask.CompletedTask;
        }

        public Dictionary<string, object> CaptureState()
        {
            return new Dictionary<string, object>
            {
                ["HasOriginalState"] = _hasOriginalState,
                ["TargetGuid"] = _targetGuid,
                ["TargetAlpha"] = _targetAlpha,
                ["Duration"] = _duration
            };
        }
    }
}
