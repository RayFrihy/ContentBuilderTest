using System;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.State
{
    public sealed class IntegerStateCondition : IStateCondition
    {
        private readonly IStateStore _stateStore;
        private readonly string _stateKey;
        private readonly int _compareValue;
        private readonly ComparisonOperator _comparisonOperator;

        public string StateKey => _stateKey;

        public IntegerStateCondition(IStateStore stateStore, string stateKey, int compareValue, ComparisonOperator comparisonOperator)
        {
            _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
            _stateKey = stateKey ?? throw new ArgumentNullException(nameof(stateKey));
            _compareValue = compareValue;
            _comparisonOperator = comparisonOperator;
        }

        public Observable<bool> Evaluate()
        {
            return Observable.EveryUpdate()
                .Select(_ => CheckState())
                .DistinctUntilChanged();
        }

        public void Reset() { }

        public void Dispose() { }

        private bool CheckState()
        {
            var value = _stateStore.GetGlobalState(_stateKey);
            if (value == null)
                return false;

            int intValue;
            if (value is int i) intValue = i;
            else if (value is long l) intValue = (int)l;
            else if (value is float f) intValue = (int)f;
            else if (value is double d) intValue = (int)d;
            else return false;

            return _comparisonOperator switch
            {
                ComparisonOperator.Equal => intValue == _compareValue,
                ComparisonOperator.NotEqual => intValue != _compareValue,
                ComparisonOperator.LessThan => intValue < _compareValue,
                ComparisonOperator.LessThanOrEqual => intValue <= _compareValue,
                ComparisonOperator.GreaterThan => intValue > _compareValue,
                ComparisonOperator.GreaterThanOrEqual => intValue >= _compareValue,
                _ => false
            };
        }
    }
}
