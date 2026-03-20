using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ReactiveFlowEngine.Behaviors
{
    public class RaycastBehavior : IReversibleBehavior, IStateCaptureBehavior
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly IStateStore _stateStore;
        private readonly string _originGuid;
        private readonly Vector3 _direction;
        private readonly float _maxDistance;
        private readonly string _resultStateKey;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        private object _previousStateValue;
        private bool _hadPreviousState;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public RaycastBehavior(
            ISceneObjectResolver resolver,
            IStateStore stateStore,
            string originGuid,
            Vector3 direction,
            float maxDistance = 0f,
            string resultStateKey = "RaycastResult",
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _resolver = resolver;
            _stateStore = stateStore;
            _originGuid = originGuid;
            _direction = direction;
            _maxDistance = maxDistance;
            _resultStateKey = resultStateKey;
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public UniTask ExecuteAsync(CancellationToken ct)
        {
            var origin = _resolver.Resolve(_originGuid);
            if (origin == null) return UniTask.CompletedTask;

            _previousStateValue = _stateStore.GetGlobalState(_resultStateKey);
            _hadPreviousState = _stateStore.HasGlobalState(_resultStateKey);

            var dist = _maxDistance <= 0f ? Mathf.Infinity : _maxDistance;

            bool didHit = Physics.Raycast(origin.position, _direction, out RaycastHit hit, dist);

            var result = new Dictionary<string, object>
            {
                ["Hit"] = didHit,
                ["Point"] = hit.point,
                ["Normal"] = hit.normal,
                ["Distance"] = hit.distance,
                ["ColliderName"] = hit.collider?.name
            };

            _stateStore.SetGlobalState(_resultStateKey, result);

            return UniTask.CompletedTask;
        }

        public UniTask UndoAsync(CancellationToken ct)
        {
            if (_hadPreviousState)
            {
                _stateStore.SetGlobalState(_resultStateKey, _previousStateValue);
            }
            else
            {
                _stateStore.RemoveGlobalState(_resultStateKey);
            }
            return UniTask.CompletedTask;
        }

        public Dictionary<string, object> CaptureState()
        {
            return new Dictionary<string, object>
            {
                ["OriginGuid"] = _originGuid,
                ["Direction"] = _direction,
                ["ResultStateKey"] = _resultStateKey
            };
        }
    }
}
