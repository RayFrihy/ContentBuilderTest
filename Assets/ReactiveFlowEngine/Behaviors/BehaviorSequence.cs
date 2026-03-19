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
    public class BehaviorSequence : IReversibleBehavior
    {
        private readonly List<IBehavior> _children;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        private List<IBehavior> _executedChildren;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public BehaviorSequence(
            List<IBehavior> children,
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _children = children;
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public async UniTask ExecuteAsync(CancellationToken ct)
        {
            _executedChildren = new List<IBehavior>();

            foreach (var child in _children)
            {
                ct.ThrowIfCancellationRequested();
                await child.ExecuteAsync(ct);
                _executedChildren.Add(child);
            }
        }

        public async UniTask UndoAsync(CancellationToken ct)
        {
            if (_executedChildren == null) return;

            for (int i = _executedChildren.Count - 1; i >= 0; i--)
            {
                if (_executedChildren[i] is IReversibleBehavior reversible)
                {
                    await reversible.UndoAsync(ct);
                }
            }
        }
    }
}
