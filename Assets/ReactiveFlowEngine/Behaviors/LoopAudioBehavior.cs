using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class LoopAudioBehavior : IReversibleBehavior, IStateCaptureBehavior
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly string _audioSourceGuid;
        private readonly string _clipPath;
        private readonly float _volume;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        private AudioSource _source;
        private bool _originalLoop;
        private AudioClip _originalClip;
        private float _originalVolume;
        private bool _hasOriginalState;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public LoopAudioBehavior(
            ISceneObjectResolver resolver,
            string audioSourceGuid,
            string clipPath = null,
            float volume = 1f,
            bool isBlocking = false,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _resolver = resolver;
            _audioSourceGuid = audioSourceGuid;
            _clipPath = clipPath;
            _volume = volume;
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public UniTask ExecuteAsync(CancellationToken ct)
        {
            if (_resolver == null)
            {
                UnityEngine.Debug.LogWarning($"[RFE] LoopAudioBehavior: SceneObjectResolver is null, skipping.");
                return UniTask.CompletedTask;
            }

            var target = _resolver.Resolve(_audioSourceGuid);
            if (target == null)
            {
                UnityEngine.Debug.LogWarning($"[RFE] LoopAudioBehavior: Audio source object '{_audioSourceGuid}' not found.");
                return UniTask.CompletedTask;
            }

            _source = target.GetComponent<AudioSource>();
            if (_source == null) return UniTask.CompletedTask;

            _originalLoop = _source.loop;
            _originalClip = _source.clip;
            _originalVolume = _source.volume;
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
            _source.loop = true;
            _source.Play();

            return UniTask.CompletedTask;
        }

        public UniTask UndoAsync(CancellationToken ct)
        {
            if (_source == null || !_hasOriginalState) return UniTask.CompletedTask;

            _source.Stop();
            _source.loop = _originalLoop;
            _source.clip = _originalClip;
            _source.volume = _originalVolume;

            return UniTask.CompletedTask;
        }

        public Dictionary<string, object> CaptureState()
        {
            return new Dictionary<string, object>
            {
                ["AudioSourceGuid"] = _audioSourceGuid,
                ["OriginalLoop"] = _originalLoop,
                ["OriginalVolume"] = _originalVolume,
                ["HasOriginalState"] = _hasOriginalState
            };
        }
    }
}
