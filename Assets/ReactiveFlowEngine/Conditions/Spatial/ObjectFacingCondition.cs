using System;
using R3;
using UnityEngine;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.Spatial
{
    public sealed class ObjectFacingCondition : ISpatialCondition
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly string _targetObjectId;
        private readonly string _facingObjectId;
        private readonly float _angleTolerance;

        public string TargetObjectId => _targetObjectId;

        public ObjectFacingCondition(ISceneObjectResolver resolver, string targetObjectId, string facingObjectId, float angleTolerance)
        {
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            _targetObjectId = targetObjectId ?? throw new ArgumentNullException(nameof(targetObjectId));
            _facingObjectId = facingObjectId ?? throw new ArgumentNullException(nameof(facingObjectId));
            _angleTolerance = angleTolerance;
        }

        public Observable<bool> Evaluate()
        {
            return Observable.EveryUpdate()
                .Select(_ => IsFacing())
                .DistinctUntilChanged();
        }

        public void Reset() { }

        public void Dispose() { }

        private bool IsFacing()
        {
            var target = _resolver.Resolve(_targetObjectId);
            var facingTarget = _resolver.Resolve(_facingObjectId);

            if (target == null || facingTarget == null)
                return false;

            var directionToTarget = (facingTarget.position - target.position).normalized;
            float angle = Vector3.Angle(target.forward, directionToTarget);
            return angle <= _angleTolerance;
        }
    }
}
