using System;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Tests.TestDoubles
{
    public class TestCondition : ICondition
    {
        private readonly Subject<bool> _subject = new Subject<bool>();
        public int ResetCount { get; private set; }
        public bool IsDisposed { get; private set; }

        public Observable<bool> Evaluate()
        {
            return _subject.Prepend(false);
        }

        public void EmitResult(bool value)
        {
            _subject.OnNext(value);
        }

        public void Reset()
        {
            ResetCount++;
        }

        public void Dispose()
        {
            IsDisposed = true;
            _subject.Dispose();
        }
    }
}
