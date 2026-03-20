using System;
using R3;
using UnityEngine;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Conditions.State
{
    public sealed class FloatStateCondition : IStateCondition
    {
        private readonly IStateStore _stateStore;
        private readonly string _stateKey;
        private readonly float _compareValue;
        private readonly ComparisonOperator _comparisonOperator;
        private readonly float _tolerance;

        public string StateKey => _stateKey;

        public FloatStateCondition(IStateStore stateStore, string stateKey, float compareValue, ComparisonOperator comparisonOperator, float tolerance = 0.001f)
        {
            _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
            _stateKey = stateKey ?? throw new ArgumentNullException(nameof(stateKey));
            _compareValue = compareValue;
            _comparisonOperator = comparisonOperator;
            _tolerance = tolerance;
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

            float floatValue;
            if (value is float f) floatValue = f;
            else if (value is double d) floatValue = (float)d;
            else if (value is int i) floatValue = i;
            else if (value is long l) floatValue = l;
            else return false;

            return _comparisonOperator switch
            {
                ComparisonOperator.Equal => Mathf.Abs(floatValue - _compareValue) <= _tolerance,
                ComparisonOperator.NotEqual => Mathf.Abs(floatValue - _compareValue) > _tolerance,
                ComparisonOperator.LessThan => floatValue < _compareValue,
                ComparisonOperator.LessThanOrEqual => floatValue <= _compareValue,
                ComparisonOperator.GreaterThan => floatValue > _compareValue,
                ComparisonOperator.GreaterThanOrEqual => floatValue >= _compareValue,
                _ => false
            };
        }
    }
}
