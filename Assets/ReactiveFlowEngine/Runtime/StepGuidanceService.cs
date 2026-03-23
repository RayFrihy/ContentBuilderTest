using System;
using System.Collections.Generic;
using R3;
using UnityEngine;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Runtime
{
    public class StepGuidanceService : IStepGuidanceService
    {
        private readonly IFlowEngine _engine;
        private readonly ISceneObjectResolver _resolver;
        private readonly ReactiveProperty<IReadOnlyList<string>> _targetObjectIds;
        private readonly Color _guidanceColor = new Color(0.2f, 0.8f, 1f, 1f);
        private readonly float _guidanceIntensity = 1.5f;
        private readonly List<(Renderer renderer, MaterialPropertyBlock originalBlock)> _activeHighlights
            = new List<(Renderer, MaterialPropertyBlock)>();

        private IDisposable _stepSubscription;
        private bool _isEnabled;

        public ReadOnlyReactiveProperty<IReadOnlyList<string>> CurrentTargetObjectIds => _targetObjectIds;

        public StepGuidanceService(IFlowEngine engine, ISceneObjectResolver resolver)
        {
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            _targetObjectIds = new ReactiveProperty<IReadOnlyList<string>>(
                Array.Empty<string>() as IReadOnlyList<string>);
        }

        public void Enable()
        {
            if (_isEnabled) return;
            _isEnabled = true;
            _stepSubscription = _engine.CurrentStep.Subscribe(OnStepChanged);
        }

        public void Disable()
        {
            if (!_isEnabled) return;
            _isEnabled = false;
            _stepSubscription?.Dispose();
            _stepSubscription = null;
            ClearAllHighlights();
            _targetObjectIds.Value = Array.Empty<string>();
        }

        public void Dispose()
        {
            Disable();
            _targetObjectIds.Dispose();
        }

        private void OnStepChanged(IStep step)
        {
            ClearAllHighlights();

            if (step == null)
            {
                _targetObjectIds.Value = Array.Empty<string>();
                return;
            }

            var ids = ExtractTargetObjectIds(step);
            _targetObjectIds.Value = ids;

            foreach (var id in ids)
            {
                ApplyGuidanceHighlight(id);
            }
        }

        private List<string> ExtractTargetObjectIds(IStep step)
        {
            var targetIds = new HashSet<string>();

            if (step.Transitions == null) return new List<string>();

            foreach (var transition in step.Transitions)
            {
                if (transition.Conditions == null) continue;
                foreach (var condition in transition.Conditions)
                {
                    CollectTargetIds(condition, targetIds);
                }
            }

            return new List<string>(targetIds);
        }

        private static void CollectTargetIds(ICondition condition, HashSet<string> ids)
        {
            if (condition is IInteractionCondition ic && !string.IsNullOrEmpty(ic.TargetObjectId))
                ids.Add(ic.TargetObjectId);

            if (condition is ISpatialCondition sc && !string.IsNullOrEmpty(sc.TargetObjectId))
                ids.Add(sc.TargetObjectId);

            if (condition is ICompositeCondition cc && cc.Children != null)
            {
                foreach (var child in cc.Children)
                    CollectTargetIds(child, ids);
            }
        }

        private void ApplyGuidanceHighlight(string objectId)
        {
            var target = _resolver.Resolve(objectId);
            if (target == null)
            {
                Debug.LogWarning($"[RFE] StepGuidanceService: Target object '{objectId}' not found, skipping highlight.");
                return;
            }

            var renderers = target.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                var originalBlock = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(originalBlock);
                _activeHighlights.Add((renderer, originalBlock));

                var highlightBlock = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(highlightBlock);
                highlightBlock.SetColor("_EmissionColor", _guidanceColor * _guidanceIntensity);
                renderer.SetPropertyBlock(highlightBlock);

                foreach (var material in renderer.materials)
                {
                    material.EnableKeyword("_EMISSION");
                }
            }
        }

        private void ClearAllHighlights()
        {
            foreach (var (renderer, originalBlock) in _activeHighlights)
            {
                if (renderer == null) continue;
                renderer.SetPropertyBlock(originalBlock);
            }
            _activeHighlights.Clear();
        }
    }
}
