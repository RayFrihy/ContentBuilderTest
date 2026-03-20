using System;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.TimeBased
{
    public sealed class ElapsedTimeCondition : ITimeBasedCondition
    {
        private readonly float _requiredElapsed;
        private readonly ComparisonOperator _comparisonOperator;

        public float Duration => _requiredElapsed;

        public ElapsedTimeCondition(float requiredElapsed, ComparisonOperator comparisonOperator)
        {
            _requiredElapsed = requiredElapsed;
            _comparisonOperator = comparisonOperator;
        }

        public Observable<bool> Evaluate()
        {
            return Observable.EveryUpdate()
                .Scan(0f, (elapsed, _) => elapsed + UnityEngine.Time.deltaTime)
                .Select(elapsed => CompareElapsed(elapsed))
                .DistinctUntilChanged();
        }

        public void Reset() { }

        public void Dispose() { }

        private bool CompareElapsed(float elapsed)
        {
            return _comparisonOperator switch
            {
                ComparisonOperator.Equal => Math.Abs(elapsed - _requiredElapsed) < 0.01f,
                ComparisonOperator.NotEqual => Math.Abs(elapsed - _requiredElapsed) >= 0.01f,
                ComparisonOperator.LessThan => elapsed < _requiredElapsed,
                ComparisonOperator.LessThanOrEqual => elapsed <= _requiredElapsed,
                ComparisonOperator.GreaterThan => elapsed > _requiredElapsed,
                ComparisonOperator.GreaterThanOrEqual => elapsed >= _requiredElapsed,
                _ => false
            };
        }
    }
}
