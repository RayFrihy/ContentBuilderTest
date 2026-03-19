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
    public class UnloadSceneBehavior : IBehavior, IStateCaptureBehavior
    {
        private readonly string _sceneName;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public UnloadSceneBehavior(
            string sceneName,
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _sceneName = sceneName;
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public async UniTask ExecuteAsync(CancellationToken ct)
        {
            var op = SceneManager.UnloadSceneAsync(_sceneName);

            while (!op.isDone)
            {
                ct.ThrowIfCancellationRequested();
                await UniTask.Yield(ct);
            }
        }

        public Dictionary<string, object> CaptureState()
        {
            return new Dictionary<string, object>
            {
                ["SceneName"] = _sceneName
            };
        }
    }
}
