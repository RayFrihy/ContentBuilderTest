using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.Composite
{
    public sealed class WeightedConditionEntry
    {
        public ICondition Condition { get; }
        public float Weight { get; }

        public WeightedConditionEntry(ICondition condition, float weight)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            Weight = weight;
        }
    }

    public sealed class WeightedCondition : ICompositeCondition
    {
        private readonly WeightedConditionEntry[] _entries;
        private readonly float _requiredWeightThreshold;

        public IReadOnlyList<ICondition> Children => _entries.Select(e => e.Condition).ToArray();

        public WeightedCondition(float requiredWeightThreshold, params WeightedConditionEntry[] entries)
        {
            _entries = entries ?? Array.Empty<WeightedConditionEntry>();
            _requiredWeightThreshold = requiredWeightThreshold;
        }

        public Observable<bool> Evaluate()
        {
            if (_entries.Length == 0)
                return Observable.Return(false);

            return Observable.CombineLatest(
                _entries.Select(e => e.Condition.Evaluate()).ToArray()
            ).Select(values => CalculateWeightedResult(values));
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

        private bool CalculateWeightedResult(IList<bool> values)
        {
            float totalWeight = 0f;
            float achievedWeight = 0f;

            for (int i = 0; i < _entries.Length; i++)
            {
                totalWeight += _entries[i].Weight;
                if (values[i])
                    achievedWeight += _entries[i].Weight;
            }

            if (totalWeight <= 0f)
                return false;

            float normalizedScore = achievedWeight / totalWeight;
            return normalizedScore >= _requiredWeightThreshold;
        }
    }
}
