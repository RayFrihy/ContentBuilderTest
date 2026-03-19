using System;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.State
{
    public sealed class VariableComparisonCondition : IStateCondition
    {
        private readonly IStateStore _stateStore;
        private readonly string _leftKey;
        private readonly string _rightKey;
        private readonly ComparisonOperator _comparisonOperator;
        private IDisposable _subscription;

        public string StateKey => _leftKey;

        public VariableComparisonCondition(IStateStore stateStore, string leftKey, string rightKey, ComparisonOperator comparisonOperator)
        {
            _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
            _leftKey = leftKey ?? throw new ArgumentNullException(nameof(leftKey));
            _rightKey = rightKey ?? throw new ArgumentNullException(nameof(rightKey));
            _comparisonOperator = comparisonOperator;
        }

        public Observable<bool> Evaluate()
        {
            return Observable.EveryUpdate()
                .Select(_ => CompareValues())
                .DistinctUntilChanged();
        }

        public void Reset() { }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
        }

        private bool CompareValues()
        {
            var leftValue = _stateStore.GetGlobalState(_leftKey);
            var rightValue = _stateStore.GetGlobalState(_rightKey);

            if (leftValue == null || rightValue == null)
                return false;

            if (leftValue is IComparable leftComparable && rightValue is IComparable)
            {
                try
                {
                    int comparison = leftComparable.CompareTo(rightValue);
                    return _comparisonOperator switch
                    {
                        ComparisonOperator.Equal => comparison == 0,
                        ComparisonOperator.NotEqual => comparison != 0,
                        ComparisonOperator.LessThan => comparison < 0,
                        ComparisonOperator.LessThanOrEqual => comparison <= 0,
                        ComparisonOperator.GreaterThan => comparison > 0,
                        ComparisonOperator.GreaterThanOrEqual => comparison >= 0,
                        _ => false
                    };
                }
                catch (ArgumentException)
                {
                    return _comparisonOperator == ComparisonOperator.Equal
                        ? leftValue.Equals(rightValue)
                        : !leftValue.Equals(rightValue);
                }
            }

            return _comparisonOperator == ComparisonOperator.Equal && leftValue.Equals(rightValue);
        }
    }
}
