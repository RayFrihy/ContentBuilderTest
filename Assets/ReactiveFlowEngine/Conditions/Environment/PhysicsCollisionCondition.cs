using System;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.Environment
{
    public sealed class PhysicsCollisionCondition : IEnvironmentCondition
    {
        private readonly IEventBus _eventBus;
        private readonly string _objectAId;
        private readonly string _objectBId;
        private IDisposable _subscription;

        public PhysicsCollisionCondition(IEventBus eventBus, string objectAId, string objectBId)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _objectAId = objectAId ?? throw new ArgumentNullException(nameof(objectAId));
            _objectBId = objectBId ?? throw new ArgumentNullException(nameof(objectBId));
        }

        public Observable<bool> Evaluate()
        {
            var collisionEnter = _eventBus.On("CollisionEnter")
                .Select(payload => IsMatchingCollision(payload) ? true : (bool?)null)
                .Where(v => v.HasValue)
                .Select(v => v.Value);

            var collisionExit = _eventBus.On("CollisionExit")
                .Select(payload => IsMatchingCollision(payload) ? false : (bool?)null)
                .Where(v => v.HasValue)
                .Select(v => v.Value);

            return Observable.Merge(collisionEnter, collisionExit)
                .Prepend(false);
        }

        public void Reset() { }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
        }

        private bool IsMatchingCollision(object payload)
        {
            if (payload is CollisionEventData data)
            {
                return (string.Equals(data.ObjectAId, _objectAId, StringComparison.Ordinal) &&
                        string.Equals(data.ObjectBId, _objectBId, StringComparison.Ordinal)) ||
                       (string.Equals(data.ObjectAId, _objectBId, StringComparison.Ordinal) &&
                        string.Equals(data.ObjectBId, _objectAId, StringComparison.Ordinal));
            }
            return false;
        }
    }

    public sealed class CollisionEventData
    {
        public string ObjectAId { get; }
        public string ObjectBId { get; }

        public CollisionEventData(string objectAId, string objectBId)
        {
            ObjectAId = objectAId;
            ObjectBId = objectBId;
        }
    }
}
