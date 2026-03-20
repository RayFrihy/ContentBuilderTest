using System;
using R3;
using UnityEngine;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.Spatial
{
    public sealed class ObjectRotationCondition : ISpatialCondition
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly string _targetObjectId;
        private readonly Vector3 _targetEulerAngles;
        private readonly float _angleTolerance;

        public string TargetObjectId => _targetObjectId;

        public ObjectRotationCondition(ISceneObjectResolver resolver, string targetObjectId, Vector3 targetEulerAngles, float angleTolerance)
        {
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            _targetObjectId = targetObjectId ?? throw new ArgumentNullException(nameof(targetObjectId));
            _targetEulerAngles = targetEulerAngles;
            _angleTolerance = angleTolerance;
        }

        public Observable<bool> Evaluate()
        {
            return Observable.EveryUpdate()
                .Select(_ => IsAtTargetRotation())
                .DistinctUntilChanged();
        }

        public void Reset() { }

        public void Dispose() { }

        private bool IsAtTargetRotation()
        {
            var target = _resolver.Resolve(_targetObjectId);
            if (target == null)
                return false;

            var targetRotation = Quaternion.Euler(_targetEulerAngles);
            float angle = Quaternion.Angle(target.rotation, targetRotation);
            return angle <= _angleTolerance;
        }
    }
}
