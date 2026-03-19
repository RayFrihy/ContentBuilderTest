using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class SetTransformBehavior : IReversibleBehavior, IStateCaptureBehavior
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly string _targetGuid;
        private readonly Vector3? _position;
        private readonly Quaternion? _rotation;
        private readonly Vector3? _scale;
        private readonly bool _useLocal;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        private Vector3 _originalPosition;
        private Quaternion _originalRotation;
        private Vector3 _originalScale;
        private bool _hasOriginalState;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public SetTransformBehavior(
            ISceneObjectResolver resolver,
            string targetGuid,
            Vector3? position,
            Quaternion? rotation,
            Vector3? scale,
            bool useLocal = false,
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _resolver = resolver;
            _targetGuid = targetGuid;
            _position = position;
            _rotation = rotation;
            _scale = scale;
            _useLocal = useLocal;
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public async UniTask ExecuteAsync(CancellationToken ct)
        {
            if (_resolver == null) return;

            var target = _resolver.Resolve(_targetGuid);
            if (target == null) return;

            _originalPosition = _useLocal ? target.localPosition : target.position;
            _originalRotation = _useLocal ? target.localRotation : target.rotation;
            _originalScale = target.localScale;
            _hasOriginalState = true;

            if (_position.HasValue)
            {
                if (_useLocal)
                    target.localPosition = _position.Value;
                else
                    target.position = _position.Value;
            }

            if (_rotation.HasValue)
            {
                if (_useLocal)
                    target.localRotation = _rotation.Value;
                else
                    target.rotation = _rotation.Value;
            }

            if (_scale.HasValue)
            {
                target.localScale = _scale.Value;
            }

            await UniTask.CompletedTask;
        }

        public async UniTask UndoAsync(CancellationToken ct)
        {
            if (_resolver == null || !_hasOriginalState) return;

            var target = _resolver.Resolve(_targetGuid);
            if (target == null) return;

            if (_useLocal)
            {
                target.localPosition = _originalPosition;
                target.localRotation = _originalRotation;
            }
            else
            {
                target.position = _originalPosition;
                target.rotation = _originalRotation;
            }

            target.localScale = _originalScale;
            await UniTask.CompletedTask;
        }

        public Dictionary<string, object> CaptureState()
        {
            return new Dictionary<string, object>
            {
                ["OriginalPosition"] = _originalPosition,
                ["OriginalRotation"] = _originalRotation,
                ["OriginalScale"] = _originalScale,
                ["HasOriginalState"] = _hasOriginalState,
                ["TargetGuid"] = _targetGuid
            };
        }
    }
}
