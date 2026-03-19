using System;
using System.Collections.Generic;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.State
{
    public class EventBus : IEventBus, IDisposable
    {
        private readonly Subject<(string EventName, object Payload)> _subject = new Subject<(string, object)>();

        public void Publish(string eventName, object payload = null)
        {
            if (string.IsNullOrEmpty(eventName)) return;
            _subject.OnNext((eventName, payload));
        }

        public Observable<object> On(string eventName)
        {
            return _subject
                .Where(e => string.Equals(e.EventName, eventName, StringComparison.Ordinal))
                .Select(e => e.Payload);
        }

        public void Dispose()
        {
            _subject.Dispose();
        }
    }
}
