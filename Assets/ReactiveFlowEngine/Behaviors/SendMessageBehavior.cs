using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ReactiveFlowEngine.Behaviors
{
    public class SendMessageBehavior : IBehavior
    {
        private readonly ISceneObjectResolver _resolver;
        private readonly string _targetGuid;
        private readonly string _methodName;
        private readonly object _argument;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public SendMessageBehavior(
            ISceneObjectResolver resolver,
            string targetGuid,
            string methodName,
            object argument = null,
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _resolver = resolver;
            _targetGuid = targetGuid;
            _methodName = methodName;
            _argument = argument;
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public async UniTask ExecuteAsync(CancellationToken ct)
        {
            var target = _resolver.Resolve(_targetGuid);
            if (target == null) return;

            if (_argument != null)
                target.gameObject.SendMessage(_methodName, _argument, SendMessageOptions.DontRequireReceiver);
            else
                target.gameObject.SendMessage(_methodName, SendMessageOptions.DontRequireReceiver);

            await UniTask.CompletedTask;
        }
    }
}
