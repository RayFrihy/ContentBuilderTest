using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class UnhighlightObjectBehavior : IReversibleBehavior, IStateCaptureBehavior
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly string _targetGuid;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        private List<(Renderer renderer, MaterialPropertyBlock originalBlock)> _originalStates;
        private bool _hasOriginalState;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public UnhighlightObjectBehavior(
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
                UnityEngine.Debug.LogWarning($"[RFE] UnhighlightObjectBehavior: SceneObjectResolver is null, skipping.");
                return UniTask.CompletedTask;
            }

            var target = _resolver.Resolve(_targetGuid);
            if (target == null)
            {
                UnityEngine.Debug.LogWarning($"[RFE] UnhighlightObjectBehavior: Target object '{_targetGuid}' not found.");
                return UniTask.CompletedTask;
            }

            var renderers = target.GetComponentsInChildren<Renderer>();
            _originalStates = new List<(Renderer, MaterialPropertyBlock)>(renderers.Length);

            foreach (var renderer in renderers)
            {
                ct.ThrowIfCancellationRequested();

                var originalBlock = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(originalBlock);
                _originalStates.Add((renderer, originalBlock));

                var emptyBlock = new MaterialPropertyBlock();
                renderer.SetPropertyBlock(emptyBlock);

                foreach (var material in renderer.materials)
                {
                    material.DisableKeyword("_EMISSION");
                }
            }

            _hasOriginalState = true;
            return UniTask.CompletedTask;
        }

        public UniTask UndoAsync(CancellationToken ct)
        {
            if (_resolver == null || !_hasOriginalState) return UniTask.CompletedTask;

            foreach (var (renderer, originalBlock) in _originalStates)
            {
                ct.ThrowIfCancellationRequested();

                if (renderer == null) continue;
                renderer.SetPropertyBlock(originalBlock);
            }

            return UniTask.CompletedTask;
        }

        public Dictionary<string, object> CaptureState()
        {
            return new Dictionary<string, object>
            {
                ["TargetGuid"] = _targetGuid,
                ["HasOriginalState"] = _hasOriginalState
            };
        }
    }
}
