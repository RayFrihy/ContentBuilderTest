using System.Threading;
using Cysharp.Threading.Tasks;

namespace ReactiveFlowEngine.Abstractions
{
    public interface IBehavior
    {
        ExecutionStages Stages { get; }
        bool IsBlocking { get; }
        UniTask ExecuteAsync(CancellationToken ct);
    }
}
