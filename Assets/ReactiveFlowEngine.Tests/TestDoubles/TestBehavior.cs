using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Tests.TestDoubles
{
    public class TestBehavior : IReversibleBehavior, IStateCaptureBehavior
    {
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        public int ExecuteCount { get; private set; }
        public int UndoCount { get; private set; }
        public List<string> ExecutionLog { get; } = new List<string>();

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public TestBehavior(
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public UniTask ExecuteAsync(CancellationToken ct)
        {
            ExecuteCount++;
            ExecutionLog.Add("Execute");
            return UniTask.CompletedTask;
        }

        public UniTask UndoAsync(CancellationToken ct)
        {
            UndoCount++;
            ExecutionLog.Add("Undo");
            return UniTask.CompletedTask;
        }

        public Dictionary<string, object> CaptureState()
        {
            return new Dictionary<string, object>
            {
                ["ExecuteCount"] = ExecuteCount,
                ["UndoCount"] = UndoCount
            };
        }
    }
}
