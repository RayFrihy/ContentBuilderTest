using System;
using R3;
using UnityEngine;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.Spatial
{
    public sealed class ObjectExitedZoneCondition : ISpatialCondition
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly string _targetObjectId;
        private readonly string _zoneObjectId;

        public string TargetObjectId => _targetObjectId;

        public ObjectExitedZoneCondition(ISceneObjectResolver resolver, string targetObjectId, string zoneObjectId)
        {
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            _targetObjectId = targetObjectId ?? throw new ArgumentNullException(nameof(targetObjectId));
            _zoneObjectId = zoneObjectId ?? throw new ArgumentNullException(nameof(zoneObjectId));
        }

        public Observable<bool> Evaluate()
        {
            return Observable.EveryUpdate()
                .Select(_ => IsOutsideZone())
                .DistinctUntilChanged();
        }

        public void Reset() { }

        public void Dispose() { }

        private bool IsOutsideZone()
        {
            var target = _resolver.Resolve(_targetObjectId);
            var zone = _resolver.Resolve(_zoneObjectId);

            if (target == null || zone == null)
                return false;

            var collider = zone.GetComponent<Collider>();
            if (collider != null)
                return !collider.bounds.Contains(target.position);

            var renderer = zone.GetComponent<Renderer>();
            if (renderer != null)
                return !renderer.bounds.Contains(target.position);

            return true;
        }
    }
}
