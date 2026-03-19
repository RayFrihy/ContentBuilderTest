using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class EnableObjectBehavior : IReversibleBehavior, IStateCaptureBehavior
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly string _targetGuid;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        private bool _wasActive;
        private bool _hasOriginalState;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public EnableObjectBehavior(
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

        public async UniTask ExecuteAsync(CancellationToken ct)
        {
            if (_resolver == null) return;

            var target = _resolver.Resolve(_targetGuid);
            if (target == null) return;

            _wasActive = target.gameObject.activeSelf;
            _hasOriginalState = true;

            target.gameObject.SetActive(true);
            await UniTask.CompletedTask;
        }

        public async UniTask UndoAsync(CancellationToken ct)
        {
            if (_resolver == null || !_hasOriginalState) return;

            var target = _resolver.Resolve(_targetGuid);
            if (target == null) return;

            target.gameObject.SetActive(_wasActive);
            await UniTask.CompletedTask;
        }

        public Dictionary<string, object> CaptureState()
        {
            return new Dictionary<string, object>
            {
                ["WasActive"] = _wasActive,
                ["HasOriginalState"] = _hasOriginalState,
                ["TargetGuid"] = _targetGuid
            };
        }
    }
}
