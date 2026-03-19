using System;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.Interaction
{
    public sealed class InputActionTriggeredCondition : IInteractionCondition
    {
        private readonly IEventBus _eventBus;
        private readonly string _actionName;
        private IDisposable _subscription;

        public string TargetObjectId => _actionName;

        public InputActionTriggeredCondition(IEventBus eventBus, string actionName)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _actionName = actionName ?? throw new ArgumentNullException(nameof(actionName));
        }

        public Observable<bool> Evaluate()
        {
            return _eventBus.On("InputActionTriggered")
                .Select(payload => FilterByAction(payload))
                .Prepend(false);
        }

        public void Reset() { }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
        }

        private bool FilterByAction(object payload)
        {
            if (payload is string actionName)
                return string.Equals(actionName, _actionName, StringComparison.Ordinal);
            return false;
        }
    }
}
