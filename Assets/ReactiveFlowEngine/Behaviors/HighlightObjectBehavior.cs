using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class HighlightObjectBehavior : IReversibleBehavior, IStateCaptureBehavior
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly string _targetGuid;
        private readonly Color _highlightColor;
        private readonly float _intensity;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        private List<(Renderer renderer, MaterialPropertyBlock originalBlock)> _originalStates;
        private bool _hasOriginalState;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public HighlightObjectBehavior(
            ISceneObjectResolver resolver,
            string targetGuid,
            Color highlightColor,
            float intensity = 1f,
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _resolver = resolver;
            _targetGuid = targetGuid;
            _highlightColor = highlightColor;
            _intensity = intensity;
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public async UniTask ExecuteAsync(CancellationToken ct)
        {
            if (_resolver == null) return;

            var target = _resolver.Resolve(_targetGuid);
            if (target == null) return;

            var renderers = target.GetComponentsInChildren<Renderer>();
            _originalStates = new List<(Renderer, MaterialPropertyBlock)>(renderers.Length);

            foreach (var renderer in renderers)
            {
                ct.ThrowIfCancellationRequested();

                var originalBlock = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(originalBlock);
                _originalStates.Add((renderer, originalBlock));

                var highlightBlock = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(highlightBlock);
                highlightBlock.SetColor("_EmissionColor", _highlightColor * _intensity);
                renderer.SetPropertyBlock(highlightBlock);

                foreach (var material in renderer.materials)
                {
                    material.EnableKeyword("_EMISSION");
                }
            }

            _hasOriginalState = true;
            await UniTask.CompletedTask;
        }

        public async UniTask UndoAsync(CancellationToken ct)
        {
            if (_resolver == null || !_hasOriginalState) return;

            foreach (var (renderer, originalBlock) in _originalStates)
            {
                ct.ThrowIfCancellationRequested();

                if (renderer == null) continue;
                renderer.SetPropertyBlock(originalBlock);
            }

            await UniTask.CompletedTask;
        }

        public Dictionary<string, object> CaptureState()
        {
            return new Dictionary<string, object>
            {
                ["TargetGuid"] = _targetGuid,
                ["HighlightColor"] = _highlightColor,
                ["Intensity"] = _intensity,
                ["HasOriginalState"] = _hasOriginalState
            };
        }
    }
}
