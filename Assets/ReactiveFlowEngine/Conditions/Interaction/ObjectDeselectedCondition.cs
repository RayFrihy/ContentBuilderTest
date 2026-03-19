using System;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.Interaction
{
    public sealed class ObjectDeselectedCondition : IInteractionCondition
    {
        private readonly IEventBus _eventBus;
        private readonly string _targetObjectId;
        private IDisposable _subscription;

        public string TargetObjectId => _targetObjectId;

        public ObjectDeselectedCondition(IEventBus eventBus, string targetObjectId)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _targetObjectId = targetObjectId ?? throw new ArgumentNullException(nameof(targetObjectId));
        }

        public Observable<bool> Evaluate()
        {
            return _eventBus.On("ObjectDeselected")
                .Select(payload => FilterByTarget(payload))
                .Prepend(false);
        }

        public void Reset() { }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
        }

        private bool FilterByTarget(object payload)
        {
            if (payload is string objectId)
                return string.Equals(objectId, _targetObjectId, StringComparison.Ordinal);
            return false;
        }
    }
}
