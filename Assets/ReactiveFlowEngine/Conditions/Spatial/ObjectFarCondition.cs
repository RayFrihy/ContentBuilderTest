using System;
using R3;
using UnityEngine;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.Spatial
{
    public sealed class ObjectFarCondition : ISpatialCondition
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly string _targetObjectId;
        private readonly string _referenceObjectId;
        private readonly float _threshold;

        public string TargetObjectId => _targetObjectId;

        public ObjectFarCondition(ISceneObjectResolver resolver, string targetObjectId, string referenceObjectId, float threshold)
        {
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            _targetObjectId = targetObjectId ?? throw new ArgumentNullException(nameof(targetObjectId));
            _referenceObjectId = referenceObjectId ?? throw new ArgumentNullException(nameof(referenceObjectId));
            _threshold = threshold;
        }

        public Observable<bool> Evaluate()
        {
            return Observable.EveryUpdate()
                .Select(_ => IsFar())
                .DistinctUntilChanged();
        }

        public void Reset() { }

        public void Dispose() { }

        private bool IsFar()
        {
            var target = _resolver.Resolve(_targetObjectId);
            var reference = _resolver.Resolve(_referenceObjectId);

            if (target == null || reference == null)
                return false;

            return Vector3.Distance(target.position, reference.position) > _threshold;
        }
    }
}
