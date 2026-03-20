using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class TriggerParticleEffectBehavior : IReversibleBehavior, IStateCaptureBehavior
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly string _targetGuid;
        private readonly bool _play;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        private bool _wasPlaying;
        private bool _hasOriginalState;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public TriggerParticleEffectBehavior(
            ISceneObjectResolver resolver,
            string targetGuid,
            bool play = true,
            bool isBlocking = false,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _resolver = resolver;
            _targetGuid = targetGuid;
            _play = play;
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public UniTask ExecuteAsync(CancellationToken ct)
        {
            if (_resolver == null)
            {
                UnityEngine.Debug.LogWarning($"[RFE] TriggerParticleEffectBehavior: SceneObjectResolver is null, skipping.");
                return UniTask.CompletedTask;
            }

            var target = _resolver.Resolve(_targetGuid);
            if (target == null)
            {
                UnityEngine.Debug.LogWarning($"[RFE] TriggerParticleEffectBehavior: Target object '{_targetGuid}' not found.");
                return UniTask.CompletedTask;
            }

            var ps = target.GetComponent<ParticleSystem>();
            if (ps == null) return UniTask.CompletedTask;

            _wasPlaying = ps.isPlaying;
            _hasOriginalState = true;

            if (_play)
            {
                ps.Play();
            }
            else
            {
                ps.Stop();
            }

            return UniTask.CompletedTask;
        }

        public UniTask UndoAsync(CancellationToken ct)
        {
            if (_resolver == null || !_hasOriginalState) return UniTask.CompletedTask;

            var target = _resolver.Resolve(_targetGuid);
            if (target == null) return UniTask.CompletedTask;

            var ps = target.GetComponent<ParticleSystem>();
            if (ps == null) return UniTask.CompletedTask;

            if (_play && !_wasPlaying)
            {
                ps.Stop();
            }
            else if (!_play && _wasPlaying)
            {
                ps.Play();
            }

            return UniTask.CompletedTask;
        }

        public Dictionary<string, object> CaptureState()
        {
            return new Dictionary<string, object>
            {
                ["TargetGuid"] = _targetGuid,
                ["Play"] = _play,
                ["WasPlaying"] = _wasPlaying,
                ["HasOriginalState"] = _hasOriginalState
            };
        }
    }
}
