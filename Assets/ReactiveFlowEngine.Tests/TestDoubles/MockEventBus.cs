using System.Collections.Generic;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Tests.TestDoubles
{
    public class MockEventBus : IEventBus
    {
        private readonly Dictionary<string, Subject<object>> _subjects = new Dictionary<string, Subject<object>>();

        public Observable<object> On(string eventName)
        {
            if (!_subjects.TryGetValue(eventName, out var subject))
            {
                subject = new Subject<object>();
                _subjects[eventName] = subject;
            }
            return subject;
        }

        public void Publish(string eventName, object payload = null)
        {
            if (_subjects.TryGetValue(eventName, out var subject))
                subject.OnNext(payload);
        }

        public void Dispose()
        {
            foreach (var subject in _subjects.Values)
                subject.Dispose();
            _subjects.Clear();
        }
    }
}
