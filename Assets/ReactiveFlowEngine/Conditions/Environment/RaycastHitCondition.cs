using System;
using R3;
using UnityEngine;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.Environment
{
    public sealed class RaycastHitCondition : ICondition
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly string _sourceObjectId;
        private readonly string _targetObjectId;
        private readonly float _maxDistance;
        private readonly LayerMask _layerMask;

        public RaycastHitCondition(ISceneObjectResolver resolver, string sourceObjectId, string targetObjectId, float maxDistance, int layerMask)
        {
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            _sourceObjectId = sourceObjectId ?? throw new ArgumentNullException(nameof(sourceObjectId));
            _targetObjectId = targetObjectId ?? throw new ArgumentNullException(nameof(targetObjectId));
            _maxDistance = maxDistance;
            _layerMask = layerMask;
        }

        public Observable<bool> Evaluate()
        {
            return Observable.EveryUpdate()
                .Select(_ => CheckRaycastHit())
                .DistinctUntilChanged();
        }

        public void Reset() { }

        public void Dispose() { }

        private bool CheckRaycastHit()
        {
            var source = _resolver.Resolve(_sourceObjectId);
            var target = _resolver.Resolve(_targetObjectId);

            if (source == null || target == null)
                return false;

            var direction = (target.position - source.position).normalized;

            if (Physics.Raycast(source.position, direction, out RaycastHit hit, _maxDistance, _layerMask))
            {
                return hit.transform == target || hit.transform.IsChildOf(target);
            }

            return false;
        }
    }
}
