using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class PlayAudioBehavior : IReversibleBehavior
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly string _audioSourceGuid;
        private readonly string _clipPath;
        private readonly bool _waitForCompletion;
        private readonly float _volume;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        private AudioSource _source;
        private AudioClip _originalClip;
        private float _originalVolume;
        private bool _wasPlaying;
        private bool _hasOriginalState;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public PlayAudioBehavior(
            ISceneObjectResolver resolver,
            string audioSourceGuid,
            string clipPath,
            bool waitForCompletion = false,
            float volume = 1f,
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _resolver = resolver;
            _audioSourceGuid = audioSourceGuid;
            _clipPath = clipPath;
            _waitForCompletion = waitForCompletion;
            _volume = volume;
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

            _originalClip = _source.clip;
            _originalVolume = _source.volume;
            _wasPlaying = _source.isPlaying;
            _hasOriginalState = true;

            if (_clipPath != null)
            {
                var clip = Resources.Load<AudioClip>(_clipPath);
                if (clip != null)
                {
                    _source.clip = clip;
                }
            }

            _source.volume = _volume;
            _source.Play();

            if (_waitForCompletion)
            {
                while (_source != null && _source.isPlaying)
                {
                    ct.ThrowIfCancellationRequested();
                    await UniTask.Yield(ct);
                }
            }
        }

        public async UniTask UndoAsync(CancellationToken ct)
        {
            if (_source == null || !_hasOriginalState) return;

            _source.Stop();
            _source.clip = _originalClip;
            _source.volume = _originalVolume;

            await UniTask.CompletedTask;
        }
    }
}
