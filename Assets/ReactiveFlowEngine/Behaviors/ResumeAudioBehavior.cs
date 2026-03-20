using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class ResumeAudioBehavior : IReversibleBehavior, IStateCaptureBehavior
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly string _audioSourceGuid;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        private AudioSource _source;
        private bool _wasPaused;
        private bool _hasOriginalState;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public ResumeAudioBehavior(
            ISceneObjectResolver resolver,
            string audioSourceGuid,
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _resolver = resolver;
            _audioSourceGuid = audioSourceGuid;
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public UniTask ExecuteAsync(CancellationToken ct)
        {
            if (_resolver == null)
            {
                UnityEngine.Debug.LogWarning($"[RFE] ResumeAudioBehavior: SceneObjectResolver is null, skipping.");
                return UniTask.CompletedTask;
            }

            var target = _resolver.Resolve(_audioSourceGuid);
            if (target == null)
            {
                UnityEngine.Debug.LogWarning($"[RFE] ResumeAudioBehavior: Audio source object '{_audioSourceGuid}' not found.");
                return UniTask.CompletedTask;
            }

            _source = target.GetComponent<AudioSource>();
            if (_source == null) return UniTask.CompletedTask;

            _wasPaused = !_source.isPlaying;
            _hasOriginalState = true;

            _source.UnPause();

            return UniTask.CompletedTask;
        }

        public UniTask UndoAsync(CancellationToken ct)
        {
            if (_source == null || !_hasOriginalState) return UniTask.CompletedTask;

            if (_wasPaused)
            {
                _source.Pause();
            }

            return UniTask.CompletedTask;
        }

        public Dictionary<string, object> CaptureState()
        {
            return new Dictionary<string, object>
            {
                ["AudioSourceGuid"] = _audioSourceGuid,
                ["WasPaused"] = _wasPaused,
                ["HasOriginalState"] = _hasOriginalState
            };
        }
    }
}
