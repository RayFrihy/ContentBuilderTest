using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class ChangeMaterialBehavior : IReversibleBehavior, IStateCaptureBehavior
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly string _targetGuid;
        private readonly string _materialPath;
        private readonly int _materialIndex;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        private Material _originalMaterial;
        private bool _hasOriginalState;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public ChangeMaterialBehavior(
            ISceneObjectResolver resolver,
            string targetGuid,
            string materialPath,
            int materialIndex = 0,
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _resolver = resolver;
            _targetGuid = targetGuid;
            _materialPath = materialPath;
            _materialIndex = materialIndex;
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public UniTask ExecuteAsync(CancellationToken ct)
        {
            if (_resolver == null)
            {
                UnityEngine.Debug.LogWarning($"[RFE] ChangeMaterialBehavior: SceneObjectResolver is null, skipping.");
                return UniTask.CompletedTask;
            }

            var target = _resolver.Resolve(_targetGuid);
            if (target == null)
            {
                UnityEngine.Debug.LogWarning($"[RFE] ChangeMaterialBehavior: Target object '{_targetGuid}' not found.");
                return UniTask.CompletedTask;
            }

            var renderer = target.GetComponent<Renderer>();
            if (renderer == null) return UniTask.CompletedTask;

            var materials = renderer.materials;
            if (_materialIndex < 0 || _materialIndex >= materials.Length) return UniTask.CompletedTask;

            _originalMaterial = materials[_materialIndex];
            _hasOriginalState = true;

            var newMaterial = Resources.Load<Material>(_materialPath);
            if (newMaterial == null) return UniTask.CompletedTask;

            var updatedMaterials = new Material[materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                updatedMaterials[i] = materials[i];
            }

            updatedMaterials[_materialIndex] = newMaterial;
            renderer.materials = updatedMaterials;

            return UniTask.CompletedTask;
        }

        public UniTask UndoAsync(CancellationToken ct)
        {
            if (_resolver == null || !_hasOriginalState) return UniTask.CompletedTask;

            var target = _resolver.Resolve(_targetGuid);
            if (target == null) return UniTask.CompletedTask;

            var renderer = target.GetComponent<Renderer>();
            if (renderer == null) return UniTask.CompletedTask;

            var materials = renderer.materials;
            if (_materialIndex < 0 || _materialIndex >= materials.Length) return UniTask.CompletedTask;

            var updatedMaterials = new Material[materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                updatedMaterials[i] = materials[i];
            }

            updatedMaterials[_materialIndex] = _originalMaterial;
            renderer.materials = updatedMaterials;

            return UniTask.CompletedTask;
        }

        public Dictionary<string, object> CaptureState()
        {
            return new Dictionary<string, object>
            {
                ["OriginalMaterial"] = _originalMaterial,
                ["HasOriginalState"] = _hasOriginalState,
                ["TargetGuid"] = _targetGuid,
                ["MaterialPath"] = _materialPath,
                ["MaterialIndex"] = _materialIndex
            };
        }
    }
}
