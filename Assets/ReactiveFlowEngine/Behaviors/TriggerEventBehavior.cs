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
    public class TriggerEventBehavior : IBehavior
    {
        private readonly IEventBus _eventBus;
        private readonly string _eventName;
        private readonly object _payload;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public TriggerEventBehavior(
            IEventBus eventBus,
            string eventName,
            object payload = null,
            bool isBlocking = false,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _eventBus = eventBus;
            _eventName = eventName;
            _payload = payload;
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public UniTask ExecuteAsync(CancellationToken ct)
        {
            _eventBus.Publish(_eventName, _payload);
            return UniTask.CompletedTask;
        }
    }
}
