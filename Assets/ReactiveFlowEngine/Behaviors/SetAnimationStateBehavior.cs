using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class SetAnimationStateBehavior : IReversibleBehavior, IStateCaptureBehavior
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly string _targetGuid;
        private readonly string _parameterName;
        private readonly object _parameterValue;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        private object _previousValue;
        private bool _isTrigger;
        private bool _hasOriginalState;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public SetAnimationStateBehavior(
            ISceneObjectResolver resolver,
            string targetGuid,
            string parameterName,
            object parameterValue,
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _resolver = resolver;
            _targetGuid = targetGuid;
            _parameterName = parameterName;
            _parameterValue = parameterValue;
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public async UniTask ExecuteAsync(CancellationToken ct)
        {
            if (_resolver == null) return;

            var target = _resolver.Resolve(_targetGuid);
            if (target == null) return;

            var animator = target.GetComponent<Animator>();
            if (animator == null) return;

            _isTrigger = false;

            if (_parameterValue is bool boolValue)
            {
                _previousValue = animator.GetBool(_parameterName);
                animator.SetBool(_parameterName, boolValue);
            }
            else if (_parameterValue is float floatValue)
            {
                _previousValue = animator.GetFloat(_parameterName);
                animator.SetFloat(_parameterName, floatValue);
            }
            else if (_parameterValue is int intValue)
            {
                _previousValue = animator.GetInteger(_parameterName);
                animator.SetInteger(_parameterName, intValue);
            }
            else if (_parameterValue is string)
            {
                _isTrigger = true;
                animator.SetTrigger(_parameterName);
            }

            _hasOriginalState = true;
            await UniTask.CompletedTask;
        }

        public async UniTask UndoAsync(CancellationToken ct)
        {
            if (_resolver == null || !_hasOriginalState || _isTrigger) return;

            var target = _resolver.Resolve(_targetGuid);
            if (target == null) return;

            var animator = target.GetComponent<Animator>();
            if (animator == null) return;

            if (_previousValue is bool boolValue)
            {
                animator.SetBool(_parameterName, boolValue);
            }
            else if (_previousValue is float floatValue)
            {
                animator.SetFloat(_parameterName, floatValue);
            }
            else if (_previousValue is int intValue)
            {
                animator.SetInteger(_parameterName, intValue);
            }

            await UniTask.CompletedTask;
        }

        public Dictionary<string, object> CaptureState()
        {
            return new Dictionary<string, object>
            {
                ["HasOriginalState"] = _hasOriginalState,
                ["TargetGuid"] = _targetGuid,
                ["ParameterName"] = _parameterName,
                ["ParameterValue"] = _parameterValue,
                ["PreviousValue"] = _previousValue,
                ["IsTrigger"] = _isTrigger
            };
        }
    }
}
