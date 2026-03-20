using System;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.Interaction
{
    public sealed class ObjectGrabbedCondition : IInteractionCondition
    {
        private readonly IEventBus _eventBus;
        private readonly string _targetObjectId;
        private bool _isGrabbed;

        public string TargetObjectId => _targetObjectId;

        public ObjectGrabbedCondition(IEventBus eventBus, string targetObjectId)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _targetObjectId = targetObjectId ?? throw new ArgumentNullException(nameof(targetObjectId));
        }

        public Observable<bool> Evaluate()
        {
            return _eventBus.On("ObjectGrabbed")
                .Select(payload => FilterByTarget(payload))
                .Prepend(false);
        }

        public void Reset()
        {
            _isGrabbed = false;
        }

        public void Dispose() { }

        private bool FilterByTarget(object payload)
        {
            if (payload is string objectId)
                return string.Equals(objectId, _targetObjectId, StringComparison.Ordinal);
            return false;
        }
    }
}
