using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class DetachObjectBehavior : IReversibleBehavior, IStateCaptureBehavior
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly string _childGuid;
        private readonly bool _worldPositionStays;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        private Transform _originalParent;
        private bool _hasOriginalState;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public DetachObjectBehavior(
            ISceneObjectResolver resolver,
            string childGuid,
            bool worldPositionStays = true,
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _resolver = resolver;
            _childGuid = childGuid;
            _worldPositionStays = worldPositionStays;
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public async UniTask ExecuteAsync(CancellationToken ct)
        {
            if (_resolver == null) return;

            var child = _resolver.Resolve(_childGuid);
            if (child == null) return;

            _originalParent = child.parent;
            _hasOriginalState = true;

            child.SetParent(null, _worldPositionStays);
            await UniTask.CompletedTask;
        }

        public async UniTask UndoAsync(CancellationToken ct)
        {
            if (_resolver == null || !_hasOriginalState) return;

            var child = _resolver.Resolve(_childGuid);
            if (child == null) return;

            child.SetParent(_originalParent, _worldPositionStays);
            await UniTask.CompletedTask;
        }

        public Dictionary<string, object> CaptureState()
        {
            return new Dictionary<string, object>
            {
                ["ChildGuid"] = _childGuid,
                ["OriginalParent"] = _originalParent,
                ["HasOriginalState"] = _hasOriginalState
            };
        }
    }
}
