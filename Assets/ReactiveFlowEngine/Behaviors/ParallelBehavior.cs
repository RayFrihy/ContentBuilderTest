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
    public class ParallelBehavior : IReversibleBehavior
    {
        private readonly List<IBehavior> _children;
        private readonly bool _waitForAll;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public ParallelBehavior(
            List<IBehavior> children,
            bool waitForAll = true,
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _children = children;
            _waitForAll = waitForAll;
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public async UniTask ExecuteAsync(CancellationToken ct)
        {
            var tasks = _children.Select(c => c.ExecuteAsync(ct)).ToList();

            if (_waitForAll)
                await UniTask.WhenAll(tasks);
            else
                await UniTask.WhenAny(tasks);
        }

        public async UniTask UndoAsync(CancellationToken ct)
        {
            var undoTasks = _children
                .OfType<IReversibleBehavior>()
                .Select(r => r.UndoAsync(ct));

            await UniTask.WhenAll(undoTasks);
        }
    }
}
