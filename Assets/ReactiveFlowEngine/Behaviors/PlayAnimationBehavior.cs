using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class PlayAnimationBehavior : IBehavior
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly string _targetGuid;
        private readonly string _stateName;
        private readonly int _layer;
        private readonly bool _waitForCompletion;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public PlayAnimationBehavior(
            ISceneObjectResolver resolver,
            string targetGuid,
            string stateName,
            int layer = 0,
            bool waitForCompletion = false,
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _resolver = resolver;
            _targetGuid = targetGuid;
            _stateName = stateName;
            _layer = layer;
            _waitForCompletion = waitForCompletion;
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public async UniTask ExecuteAsync(CancellationToken ct)
        {
            if (_resolver == null)
            {
                UnityEngine.Debug.LogWarning($"[RFE] PlayAnimationBehavior: SceneObjectResolver is null, skipping.");
                return;
            }

            var target = _resolver.Resolve(_targetGuid);
            if (target == null)
            {
                UnityEngine.Debug.LogWarning($"[RFE] PlayAnimationBehavior: Target object '{_targetGuid}' not found.");
                return;
            }

            var animator = target.GetComponent<Animator>();
            if (animator == null) return;

            animator.Play(_stateName, _layer);

            if (_waitForCompletion)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, ct);

                while (animator.GetCurrentAnimatorStateInfo(_layer).normalizedTime < 1f)
                {
                    ct.ThrowIfCancellationRequested();
                    await UniTask.Yield(PlayerLoopTiming.Update, ct);
                }
            }
        }
    }
}
