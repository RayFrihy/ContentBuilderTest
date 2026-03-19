using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class TeleportObjectBehavior : IReversibleBehavior, IStateCaptureBehavior
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly string _targetGuid;
        private readonly string _destinationGuid;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        private Vector3 _originalPosition;
        private Quaternion _originalRotation;
        private bool _hasOriginalState;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public TeleportObjectBehavior(
            ISceneObjectResolver resolver,
            string targetGuid,
            string destinationGuid,
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _resolver = resolver;
            _targetGuid = targetGuid;
            _destinationGuid = destinationGuid;
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public async UniTask ExecuteAsync(CancellationToken ct)
        {
            if (_resolver == null) return;

            var target = _resolver.Resolve(_targetGuid);
            var destination = _resolver.Resolve(_destinationGuid);
            if (target == null || destination == null) return;

            _originalPosition = target.position;
            _originalRotation = target.rotation;
            _hasOriginalState = true;

            target.position = destination.position;
            target.rotation = destination.rotation;

            await UniTask.CompletedTask;
        }

        public async UniTask UndoAsync(CancellationToken ct)
        {
            if (_resolver == null || !_hasOriginalState) return;

            var target = _resolver.Resolve(_targetGuid);
            if (target == null) return;

            target.position = _originalPosition;
            target.rotation = _originalRotation;
            await UniTask.CompletedTask;
        }

        public Dictionary<string, object> CaptureState()
        {
            return new Dictionary<string, object>
            {
                ["OriginalPosition"] = _originalPosition,
                ["OriginalRotation"] = _originalRotation,
                ["HasOriginalState"] = _hasOriginalState,
                ["TargetGuid"] = _targetGuid,
                ["DestinationGuid"] = _destinationGuid
            };
        }
    }
}
