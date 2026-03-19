using System;
using System.Collections.Generic;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions
{
    public class CompositeNotCondition : ICompositeCondition
    {
        private readonly ICondition _inner;

        public IReadOnlyList<ICondition> Children => new[] { _inner };

        public CompositeNotCondition(ICondition inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public Observable<bool> Evaluate()
        {
            return _inner.Evaluate().Select(v => !v);
        }

        public void Reset()
        {
            _inner.Reset();
        }

        public void Dispose()
        {
            _inner.Dispose();
        }
    }
}
