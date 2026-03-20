using System;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.Environment
{
    public sealed class SceneLoadedCondition : ICondition
    {
        private readonly IEventBus _eventBus;
        private readonly string _sceneName;

        public SceneLoadedCondition(IEventBus eventBus, string sceneName)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _sceneName = sceneName ?? throw new ArgumentNullException(nameof(sceneName));
        }

        public Observable<bool> Evaluate()
        {
            return _eventBus.On("SceneLoaded")
                .Select(payload => IsMatchingScene(payload))
                .Prepend(false);
        }

        public void Reset() { }

        public void Dispose() { }

        private bool IsMatchingScene(object payload)
        {
            if (payload is string sceneName)
                return string.Equals(sceneName, _sceneName, StringComparison.Ordinal);
            return false;
        }
    }
}
