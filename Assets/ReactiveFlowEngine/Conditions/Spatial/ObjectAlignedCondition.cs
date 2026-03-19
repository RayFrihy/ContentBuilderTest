using System;
using R3;
using UnityEngine;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.Spatial
{
    public sealed class ObjectAlignedCondition : ISpatialCondition
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly string _targetObjectId;
        private readonly string _referenceObjectId;
        private readonly float _angleTolerance;
        private IDisposable _subscription;

        public string TargetObjectId => _targetObjectId;

        public ObjectAlignedCondition(ISceneObjectResolver resolver, string targetObjectId, string referenceObjectId, float angleTolerance)
        {
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            _targetObjectId = targetObjectId ?? throw new ArgumentNullException(nameof(targetObjectId));
            _referenceObjectId = referenceObjectId ?? throw new ArgumentNullException(nameof(referenceObjectId));
            _angleTolerance = angleTolerance;
        }

        public Observable<bool> Evaluate()
        {
            return Observable.EveryUpdate()
                .Select(_ => IsAligned())
                .DistinctUntilChanged();
        }

        public void Reset() { }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
        }

        private bool IsAligned()
        {
            var target = _resolver.Resolve(_targetObjectId);
            var reference = _resolver.Resolve(_referenceObjectId);

            if (target == null || reference == null)
                return false;

            float angle = Quaternion.Angle(target.rotation, reference.rotation);
            return angle <= _angleTolerance;
        }
    }
}
