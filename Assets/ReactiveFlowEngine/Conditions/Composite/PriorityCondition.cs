using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.Composite
{
    public sealed class PriorityConditionEntry
    {
        public ICondition Condition { get; }
        public int Priority { get; }

        public PriorityConditionEntry(ICondition condition, int priority)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            Priority = priority;
        }
    }

    public sealed class PriorityCondition : ICompositeCondition
    {
        private readonly PriorityConditionEntry[] _entries;

        public IReadOnlyList<ICondition> Children => _entries.Select(e => e.Condition).ToArray();

        public PriorityCondition(params PriorityConditionEntry[] entries)
        {
            _entries = entries ?? Array.Empty<PriorityConditionEntry>();
            Array.Sort(_entries, (a, b) => b.Priority.CompareTo(a.Priority));
        }

        public Observable<bool> Evaluate()
        {
            if (_entries.Length == 0)
                return Observable.Return(false);

            return Observable.CombineLatest(
                _entries.Select(e => e.Condition.Evaluate()).ToArray()
            ).Select(values => EvaluateByPriority(values));
        }

        public void Reset()
        {
            foreach (var entry in _entries)
                entry.Condition.Reset();
        }

        public void Dispose()
        {
            foreach (var entry in _entries)
                entry.Condition.Dispose();
        }

        private bool EvaluateByPriority(IList<bool> values)
        {
            for (int i = 0; i < values.Count; i++)
            {
                if (values[i])
                    return true;
            }
            return false;
        }
    }
}
