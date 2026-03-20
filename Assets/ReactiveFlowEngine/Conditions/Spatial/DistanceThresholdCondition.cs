using System;
using R3;
using UnityEngine;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.Spatial
{
    public sealed class DistanceThresholdCondition : ISpatialCondition
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly string _targetObjectId;
        private readonly string _referenceObjectId;
        private readonly float _threshold;
        private readonly ComparisonOperator _comparisonOperator;

        public string TargetObjectId => _targetObjectId;

        public DistanceThresholdCondition(
            ISceneObjectResolver resolver,
            string targetObjectId,
            string referenceObjectId,
            float threshold,
            ComparisonOperator comparisonOperator)
        {
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            _targetObjectId = targetObjectId ?? throw new ArgumentNullException(nameof(targetObjectId));
            _referenceObjectId = referenceObjectId ?? throw new ArgumentNullException(nameof(referenceObjectId));
            _threshold = threshold;
            _comparisonOperator = comparisonOperator;
        }

        public Observable<bool> Evaluate()
        {
            return Observable.EveryUpdate()
                .Select(_ => MeetsThreshold())
                .DistinctUntilChanged();
        }

        public void Reset() { }

        public void Dispose() { }

        private bool MeetsThreshold()
        {
            var target = _resolver.Resolve(_targetObjectId);
            var reference = _resolver.Resolve(_referenceObjectId);

            if (target == null || reference == null)
                return false;

            float distance = Vector3.Distance(target.position, reference.position);

            return _comparisonOperator switch
            {
                ComparisonOperator.Equal => Mathf.Approximately(distance, _threshold),
                ComparisonOperator.NotEqual => !Mathf.Approximately(distance, _threshold),
                ComparisonOperator.LessThan => distance < _threshold,
                ComparisonOperator.LessThanOrEqual => distance <= _threshold,
                ComparisonOperator.GreaterThan => distance > _threshold,
                ComparisonOperator.GreaterThanOrEqual => distance >= _threshold,
                _ => false
            };
        }
    }
}
