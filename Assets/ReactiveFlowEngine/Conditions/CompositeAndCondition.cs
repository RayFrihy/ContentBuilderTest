using System;
using System.Linq;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions
{
    public class CompositeAndCondition : ICondition
    {
        private readonly ICondition[] _children;

        public CompositeAndCondition(params ICondition[] children)
        {
            _children = children ?? Array.Empty<ICondition>();
        }

        public Observable<bool> Evaluate()
        {
            if (_children.Length == 0)
                return Observable.Return(true);

            return Observable.CombineLatest(
                _children.Select(c => c.Evaluate()).ToArray()
            ).Select(values => values.All(v => v));
        }

        public void Reset()
        {
            foreach (var child in _children)
                child.Reset();
        }

        public void Dispose()
        {
            foreach (var child in _children)
                child.Dispose();
        }
    }
}
