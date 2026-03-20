using System;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.Environment
{
    public sealed class ObjectDestroyedCondition : ICondition
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly string _targetObjectId;

        public ObjectDestroyedCondition(ISceneObjectResolver resolver, string targetObjectId)
        {
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            _targetObjectId = targetObjectId ?? throw new ArgumentNullException(nameof(targetObjectId));
        }

        public Observable<bool> Evaluate()
        {
            return Observable.EveryUpdate()
                .Select(_ => _resolver.Resolve(_targetObjectId) == null)
                .DistinctUntilChanged();
        }

        public void Reset() { }

        public void Dispose() { }
    }
}
