using System.Threading;
using Cysharp.Threading.Tasks;

namespace ReactiveFlowEngine.Abstractions
{
    public interface IReversibleBehavior : IBehavior
    {
        UniTask UndoAsync(CancellationToken ct);
    }
}
