using System;
using R3;
using UnityEngine;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.Spatial
{
    public sealed class ObjectPositionCondition : ISpatialCondition
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly string _targetObjectId;
        private readonly Vector3 _targetPosition;
        private readonly float _distanceTolerance;

        public string TargetObjectId => _targetObjectId;

        public ObjectPositionCondition(ISceneObjectResolver resolver, string targetObjectId, Vector3 targetPosition, float distanceTolerance)
        {
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            _targetObjectId = targetObjectId ?? throw new ArgumentNullException(nameof(targetObjectId));
            _targetPosition = targetPosition;
            _distanceTolerance = distanceTolerance;
        }

        public Observable<bool> Evaluate()
        {
            return Observable.EveryUpdate()
                .Select(_ => IsAtTargetPosition())
                .DistinctUntilChanged();
        }

        public void Reset() { }

        public void Dispose() { }

        private bool IsAtTargetPosition()
        {
            var target = _resolver.Resolve(_targetObjectId);
            if (target == null)
                return false;

            return Vector3.Distance(target.position, _targetPosition) <= _distanceTolerance;
        }
    }
}
