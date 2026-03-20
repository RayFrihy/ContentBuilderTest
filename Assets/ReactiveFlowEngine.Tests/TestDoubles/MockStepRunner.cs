using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Tests.TestDoubles
{
    public class MockStepRunner : IStepRunner
    {
        private readonly Queue<ITransition> _transitionQueue = new Queue<ITransition>();
        public List<IStep> RunSteps { get; } = new List<IStep>();
        public int CancelCount { get; private set; }

        public void EnqueueTransition(ITransition transition)
        {
            _transitionQueue.Enqueue(transition);
        }

        public UniTask<ITransition> RunStepAsync(IStep step, CancellationToken ct)
        {
            RunSteps.Add(step);
            var transition = _transitionQueue.Count > 0 ? _transitionQueue.Dequeue() : null;
            return UniTask.FromResult(transition);
        }

        public void CancelCurrentStep()
        {
            CancelCount++;
        }
    }
}
