using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;

namespace ReactiveFlowEngine.Abstractions
{
    public interface IFlowEngine : IDisposable
    {
        ReadOnlyReactiveProperty<EngineState> State { get; }
        ReadOnlyReactiveProperty<IStep> CurrentStep { get; }
        ReadOnlyReactiveProperty<IChapter> CurrentChapter { get; }

        UniTask StartProcessAsync(IProcess process, CancellationToken ct);
        UniTask StopAsync();
    }
}
