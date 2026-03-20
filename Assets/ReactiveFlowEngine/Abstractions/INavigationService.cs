using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;

namespace ReactiveFlowEngine.Abstractions
{
    public interface INavigationService : IDisposable
    {
        Observable<NavigationEvent> OnNavigated { get; }
        UniTask NextStepAsync(CancellationToken ct);
        UniTask PreviousStepAsync(CancellationToken ct);
        UniTask GoToStepAsync(string stepId, CancellationToken ct);
        UniTask JumpToChapterAsync(string chapterId, CancellationToken ct);
        UniTask RestartStepAsync(CancellationToken ct);
        bool CanGoBack { get; }
        bool CanGoForward { get; }
    }
}
