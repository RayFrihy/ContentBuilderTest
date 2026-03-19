using System;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.Interaction
{
    public sealed class GesturePerformedCondition : IInteractionCondition
    {
        private readonly IEventBus _eventBus;
        private readonly string _targetObjectId;
        private readonly GestureType _gestureType;
        private IDisposable _subscription;

        public string TargetObjectId => _targetObjectId;
        public GestureType GestureType => _gestureType;

        public GesturePerformedCondition(IEventBus eventBus, string targetObjectId, GestureType gestureType)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _targetObjectId = targetObjectId ?? throw new ArgumentNullException(nameof(targetObjectId));
            _gestureType = gestureType;
        }

        public Observable<bool> Evaluate()
        {
            return _eventBus.On("GesturePerformed")
                .Select(payload => FilterByGesture(payload))
                .Prepend(false);
        }

        public void Reset() { }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
        }

        private bool FilterByGesture(object payload)
        {
            if (payload is GestureEventData data)
            {
                return string.Equals(data.ObjectId, _targetObjectId, StringComparison.Ordinal)
                    && data.GestureType == _gestureType;
            }
            return false;
        }
    }

    public sealed class GestureEventData
    {
        public string ObjectId { get; }
        public GestureType GestureType { get; }

        public GestureEventData(string objectId, GestureType gestureType)
        {
            ObjectId = objectId;
            GestureType = gestureType;
        }
    }
}
