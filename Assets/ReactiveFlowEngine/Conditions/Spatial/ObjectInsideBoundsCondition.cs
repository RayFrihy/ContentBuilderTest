using System;
using R3;
using UnityEngine;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.Spatial
{
    public sealed class ObjectInsideBoundsCondition : ISpatialCondition
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly string _targetObjectId;
        private readonly Vector3 _boundsCenter;
        private readonly Vector3 _boundsSize;

        public string TargetObjectId => _targetObjectId;

        public ObjectInsideBoundsCondition(ISceneObjectResolver resolver, string targetObjectId, Vector3 boundsCenter, Vector3 boundsSize)
        {
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            _targetObjectId = targetObjectId ?? throw new ArgumentNullException(nameof(targetObjectId));
            _boundsCenter = boundsCenter;
            _boundsSize = boundsSize;
        }

        public Observable<bool> Evaluate()
        {
            return Observable.EveryUpdate()
                .Select(_ => IsInsideBounds())
                .DistinctUntilChanged();
        }

        public void Reset() { }

        public void Dispose() { }

        private bool IsInsideBounds()
        {
            var target = _resolver.Resolve(_targetObjectId);
            if (target == null)
                return false;

            var bounds = new Bounds(_boundsCenter, _boundsSize);
            return bounds.Contains(target.position);
        }
    }
}
