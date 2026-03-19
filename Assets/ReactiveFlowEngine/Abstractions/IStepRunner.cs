using System.Threading;
using Cysharp.Threading.Tasks;

namespace ReactiveFlowEngine.Abstractions
{
    public interface IStepRunner
    {
        UniTask<ITransition> RunStepAsync(IStep step, CancellationToken ct);
        void CancelCurrentStep();
    }
}
