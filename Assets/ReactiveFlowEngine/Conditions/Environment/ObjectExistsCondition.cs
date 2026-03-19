using System;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.Environment
{
    public sealed class ObjectExistsCondition : IEnvironmentCondition
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly string _targetObjectId;
        private IDisposable _subscription;

        public ObjectExistsCondition(ISceneObjectResolver resolver, string targetObjectId)
        {
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            _targetObjectId = targetObjectId ?? throw new ArgumentNullException(nameof(targetObjectId));
        }

        public Observable<bool> Evaluate()
        {
            return Observable.EveryUpdate()
                .Select(_ => _resolver.Resolve(_targetObjectId) != null)
                .DistinctUntilChanged();
        }

        public void Reset() { }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
        }
    }
}
