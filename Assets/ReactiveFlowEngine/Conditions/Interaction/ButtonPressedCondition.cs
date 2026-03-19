using System;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.Interaction
{
    public sealed class ButtonPressedCondition : IInteractionCondition
    {
        private readonly IEventBus _eventBus;
        private readonly string _buttonId;
        private IDisposable _subscription;

        public string TargetObjectId => _buttonId;

        public ButtonPressedCondition(IEventBus eventBus, string buttonId)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _buttonId = buttonId ?? throw new ArgumentNullException(nameof(buttonId));
        }

        public Observable<bool> Evaluate()
        {
            return _eventBus.On("ButtonPressed")
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
            if (payload is string buttonId)
                return string.Equals(buttonId, _buttonId, StringComparison.Ordinal);
            return false;
        }
    }
}
