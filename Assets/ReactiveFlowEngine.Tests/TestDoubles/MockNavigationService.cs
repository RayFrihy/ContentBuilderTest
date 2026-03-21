using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Tests.TestDoubles
{
    public class MockNavigationService : INavigationService
    {
        public int NextStepCount { get; private set; }
        public int PreviousStepCount { get; private set; }
        public int RestartStepCount { get; private set; }
        public string LastGoToStepId { get; private set; }
        public string LastJumpToChapterId { get; private set; }

        public Observable<NavigationEvent> OnNavigated => Observable.Empty<NavigationEvent>();
        public bool CanGoBack { get; set; }
        public bool CanGoForward { get; set; }

        public UniTask NextStepAsync(CancellationToken ct) { NextStepCount++; return UniTask.CompletedTask; }
        public UniTask PreviousStepAsync(CancellationToken ct) { PreviousStepCount++; return UniTask.CompletedTask; }
        public UniTask GoToStepAsync(string stepId, CancellationToken ct) { LastGoToStepId = stepId; return UniTask.CompletedTask; }
        public UniTask JumpToChapterAsync(string chapterId, CancellationToken ct) { LastJumpToChapterId = chapterId; return UniTask.CompletedTask; }
        public UniTask RestartStepAsync(CancellationToken ct) { RestartStepCount++; return UniTask.CompletedTask; }
        public void Dispose() { }
    }
}
