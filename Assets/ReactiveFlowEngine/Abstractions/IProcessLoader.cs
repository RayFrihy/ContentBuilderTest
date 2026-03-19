using System.Threading;
using Cysharp.Threading.Tasks;

namespace ReactiveFlowEngine.Abstractions
{
    public interface IProcessLoader
    {
        UniTask<IProcess> LoadAsync(string json, CancellationToken ct);
    }
}
