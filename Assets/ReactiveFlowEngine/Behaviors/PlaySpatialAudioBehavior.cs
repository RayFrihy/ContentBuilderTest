using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class PlaySpatialAudioBehavior : IReversibleBehavior, IStateCaptureBehavior
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly string _audioSourceGuid;
        private readonly string _clipPath;
        private readonly string _positionGuid;
        private readonly float _volume;
        private readonly bool _waitForCompletion;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        private AudioSource _source;
        private float _originalSpatialBlend;
        private Vector3 _originalPosition;
        private bool _movedSource;
        private bool _hasOriginalState;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public PlaySpatialAudioBehavior(
            ISceneObjectResolver resolver,
            string audioSourceGuid,
            string clipPath,
            string positionGuid = null,
            float volume = 1f,
            bool waitForCompletion = false,
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _resolver = resolver;
            _audioSourceGuid = audioSourceGuid;
            _clipPath = clipPath;
            _positionGuid = positionGuid;
            _volume = volume;
            _waitForCompletion = waitForCompletion;
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

            _originalSpatialBlend = _source.spatialBlend;
            _originalPosition = target.position;
            _hasOriginalState = true;

            _source.spatialBlend = 1f;

            if (_positionGuid != null)
            {
                var positionTransform = _resolver.Resolve(_positionGuid);
                if (positionTransform != null)
                {
                    target.position = positionTransform.position;
                    _movedSource = true;
                }
            }

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
            _source.spatialBlend = _originalSpatialBlend;

            if (_movedSource)
            {
                _source.transform.position = _originalPosition;
            }

            await UniTask.CompletedTask;
        }

        public Dictionary<string, object> CaptureState()
        {
            return new Dictionary<string, object>
            {
                ["AudioSourceGuid"] = _audioSourceGuid,
                ["OriginalSpatialBlend"] = _originalSpatialBlend,
                ["OriginalPosition"] = _originalPosition,
                ["MovedSource"] = _movedSource,
                ["HasOriginalState"] = _hasOriginalState
            };
        }
    }
}
