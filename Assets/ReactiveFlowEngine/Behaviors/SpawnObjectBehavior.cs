using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class SpawnObjectBehavior : IReversibleBehavior, IStateCaptureBehavior
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly string _prefabGuid;
        private readonly Vector3 _position;
        private readonly Quaternion _rotation;
        private readonly string _parentGuid;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        private GameObject _spawnedObject;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public SpawnObjectBehavior(
            ISceneObjectResolver resolver,
            string prefabGuid,
            Vector3 position,
            Quaternion rotation,
            string parentGuid = null,
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _resolver = resolver;
            _prefabGuid = prefabGuid;
            _position = position;
            _rotation = rotation;
            _parentGuid = parentGuid;
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public async UniTask ExecuteAsync(CancellationToken ct)
        {
            if (_resolver == null) return;

            var template = _resolver.Resolve(_prefabGuid);
            if (template == null) return;

            _spawnedObject = Object.Instantiate(template.gameObject, _position, _rotation);

            if (!string.IsNullOrEmpty(_parentGuid))
            {
                var parent = _resolver.Resolve(_parentGuid);
                if (parent != null)
                {
                    _spawnedObject.transform.SetParent(parent, true);
                }
            }

            await UniTask.CompletedTask;
        }

        public async UniTask UndoAsync(CancellationToken ct)
        {
            if (_spawnedObject != null)
            {
                Object.Destroy(_spawnedObject);
                _spawnedObject = null;
            }

            await UniTask.CompletedTask;
        }

        public Dictionary<string, object> CaptureState()
        {
            return new Dictionary<string, object>
            {
                ["PrefabGuid"] = _prefabGuid,
                ["Position"] = _position,
                ["Rotation"] = _rotation,
                ["SpawnedObject"] = _spawnedObject
            };
        }
    }
}
