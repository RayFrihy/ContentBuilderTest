using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class PauseAudioBehavior : IReversibleBehavior, IStateCaptureBehavior
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly string _audioSourceGuid;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        private AudioSource _source;
        private bool _wasPlaying;
        private bool _hasOriginalState;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public PauseAudioBehavior(
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

        public async UniTask ExecuteAsync(CancellationToken ct)
        {
            if (_resolver == null) return;

            var target = _resolver.Resolve(_audioSourceGuid);
            if (target == null) return;

            _source = target.GetComponent<AudioSource>();
            if (_source == null) return;

            _wasPlaying = _source.isPlaying;
            _hasOriginalState = true;

            _source.Pause();

            await UniTask.CompletedTask;
        }

        public async UniTask UndoAsync(CancellationToken ct)
        {
            if (_source == null || !_hasOriginalState) return;

            if (_wasPlaying)
            {
                _source.UnPause();
            }

            await UniTask.CompletedTask;
        }

        public Dictionary<string, object> CaptureState()
        {
            return new Dictionary<string, object>
            {
                ["AudioSourceGuid"] = _audioSourceGuid,
                ["WasPlaying"] = _wasPlaying,
                ["HasOriginalState"] = _hasOriginalState
            };
        }
    }
}
