using System;
using R3;
using UnityEngine;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.Environment
{
    public sealed class LayerMaskCondition : IEnvironmentCondition
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly string _targetObjectId;
        private readonly LayerMask _expectedLayerMask;
        private IDisposable _subscription;

        public LayerMaskCondition(ISceneObjectResolver resolver, string targetObjectId, int expectedLayerMask)
        {
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            _targetObjectId = targetObjectId ?? throw new ArgumentNullException(nameof(targetObjectId));
            _expectedLayerMask = expectedLayerMask;
        }

        public Observable<bool> Evaluate()
        {
            return Observable.EveryUpdate()
                .Select(_ => IsOnExpectedLayer())
                .DistinctUntilChanged();
        }

        public void Reset() { }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
        }

        private bool IsOnExpectedLayer()
        {
            var target = _resolver.Resolve(_targetObjectId);
            if (target == null)
                return false;

            int objectLayerBit = 1 << target.gameObject.layer;
            return (_expectedLayerMask.value & objectLayerBit) != 0;
        }
    }
}
