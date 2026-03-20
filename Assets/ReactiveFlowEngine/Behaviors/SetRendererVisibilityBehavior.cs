using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class SetRendererVisibilityBehavior : IReversibleBehavior, IStateCaptureBehavior
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly string _targetGuid;
        private readonly bool _targetVisibility;
        private readonly bool _includeChildren;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        private Dictionary<Renderer, bool> _originalEnabledStates;
        private bool _hasOriginalState;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public SetRendererVisibilityBehavior(
            ISceneObjectResolver resolver,
            string targetGuid,
            bool targetVisibility,
            bool includeChildren = true,
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _resolver = resolver;
            _targetGuid = targetGuid;
            _targetVisibility = targetVisibility;
            _includeChildren = includeChildren;
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public UniTask ExecuteAsync(CancellationToken ct)
        {
            if (_resolver == null)
            {
                Debug.LogWarning($"[RFE] SetRendererVisibilityBehavior: SceneObjectResolver is null, skipping.");
                return UniTask.CompletedTask;
            }

            var target = _resolver.Resolve(_targetGuid);
            if (target == null)
            {
                Debug.LogWarning($"[RFE] SetRendererVisibilityBehavior: Target object '{_targetGuid}' not found.");
                return UniTask.CompletedTask;
            }

            Renderer[] renderers;
            if (_includeChildren)
            {
                renderers = target.GetComponentsInChildren<Renderer>(true);
            }
            else
            {
                var renderer = target.GetComponent<Renderer>();
                renderers = renderer != null ? new[] { renderer } : new Renderer[0];
            }

            _originalEnabledStates = new Dictionary<Renderer, bool>(renderers.Length);

            foreach (var renderer in renderers)
            {
                ct.ThrowIfCancellationRequested();

                _originalEnabledStates[renderer] = renderer.enabled;
                renderer.enabled = _targetVisibility;
            }

            _hasOriginalState = true;
            return UniTask.CompletedTask;
        }

        public UniTask UndoAsync(CancellationToken ct)
        {
            if (_resolver == null || !_hasOriginalState) return UniTask.CompletedTask;

            foreach (var kvp in _originalEnabledStates)
            {
                ct.ThrowIfCancellationRequested();

                if (kvp.Key == null) continue;
                kvp.Key.enabled = kvp.Value;
            }

            return UniTask.CompletedTask;
        }

        public Dictionary<string, object> CaptureState()
        {
            return new Dictionary<string, object>
            {
                ["HasOriginalState"] = _hasOriginalState,
                ["TargetGuid"] = _targetGuid,
                ["TargetVisibility"] = _targetVisibility,
                ["IncludeChildren"] = _includeChildren
            };
        }
    }
}
