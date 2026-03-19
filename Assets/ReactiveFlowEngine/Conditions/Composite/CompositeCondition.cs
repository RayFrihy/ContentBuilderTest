using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.Composite
{
    public enum CompositeMode
    {
        All,
        Any,
        None,
        ExactlyOne,
        AtLeast,
        AtMost
    }

    public sealed class CompositeCondition : ICompositeCondition
    {
        private readonly ICondition[] _children;
        private readonly CompositeMode _mode;
        private readonly int _threshold;

        public IReadOnlyList<ICondition> Children => _children;
        public CompositeMode Mode => _mode;

        public CompositeCondition(CompositeMode mode, int threshold, params ICondition[] children)
        {
            _children = children ?? Array.Empty<ICondition>();
            _mode = mode;
            _threshold = threshold;
        }

        public CompositeCondition(CompositeMode mode, params ICondition[] children)
            : this(mode, 0, children)
        {
        }

        public Observable<bool> Evaluate()
        {
            if (_children.Length == 0)
            {
                return _mode == CompositeMode.All || _mode == CompositeMode.None
                    ? Observable.Return(true)
                    : Observable.Return(false);
            }

            return Observable.CombineLatest(
                _children.Select(c => c.Evaluate()).ToArray()
            ).Select(values => EvaluateMode(values));
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

        private bool EvaluateMode(IList<bool> values)
        {
            int trueCount = 0;
            for (int i = 0; i < values.Count; i++)
            {
                if (values[i])
                    trueCount++;
            }

            return _mode switch
            {
                CompositeMode.All => trueCount == values.Count,
                CompositeMode.Any => trueCount > 0,
                CompositeMode.None => trueCount == 0,
                CompositeMode.ExactlyOne => trueCount == 1,
                CompositeMode.AtLeast => trueCount >= _threshold,
                CompositeMode.AtMost => trueCount <= _threshold,
                _ => false
            };
        }
    }
}
