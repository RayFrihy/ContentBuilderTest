using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class SetParentBehavior : IReversibleBehavior, IStateCaptureBehavior
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly string _childGuid;
        private readonly string _parentGuid;
        private readonly bool _worldPositionStays;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        private Transform _originalParent;
        private bool _hasOriginalState;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public SetParentBehavior(
            ISceneObjectResolver resolver,
            string childGuid,
            string parentGuid,
            bool worldPositionStays = true,
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _resolver = resolver;
            _childGuid = childGuid;
            _parentGuid = parentGuid;
            _worldPositionStays = worldPositionStays;
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public UniTask ExecuteAsync(CancellationToken ct)
        {
            if (_resolver == null)
            {
                UnityEngine.Debug.LogWarning($"[RFE] SetParentBehavior: SceneObjectResolver is null, skipping.");
                return UniTask.CompletedTask;
            }

            var child = _resolver.Resolve(_childGuid);
            var parent = _resolver.Resolve(_parentGuid);
            if (child == null || parent == null)
            {
                UnityEngine.Debug.LogWarning($"[RFE] SetParentBehavior: Child '{_childGuid}' or parent '{_parentGuid}' not found.");
                return UniTask.CompletedTask;
            }

            _originalParent = child.parent;
            _hasOriginalState = true;

            child.SetParent(parent, _worldPositionStays);
            return UniTask.CompletedTask;
        }

        public UniTask UndoAsync(CancellationToken ct)
        {
            if (_resolver == null || !_hasOriginalState) return UniTask.CompletedTask;

            var child = _resolver.Resolve(_childGuid);
            if (child == null) return UniTask.CompletedTask;

            child.SetParent(_originalParent, _worldPositionStays);
            return UniTask.CompletedTask;
        }

        public Dictionary<string, object> CaptureState()
        {
            return new Dictionary<string, object>
            {
                ["ChildGuid"] = _childGuid,
                ["ParentGuid"] = _parentGuid,
                ["OriginalParent"] = _originalParent,
                ["HasOriginalState"] = _hasOriginalState
            };
        }
    }
}
