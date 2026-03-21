using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Tests.TestDoubles
{
    public class MockFlowEngine : IFlowEngine
    {
        private readonly ReactiveProperty<EngineState> _state = new ReactiveProperty<EngineState>(EngineState.Idle);
        private readonly ReactiveProperty<IStep> _currentStep = new ReactiveProperty<IStep>(null);
        private readonly ReactiveProperty<IChapter> _currentChapter = new ReactiveProperty<IChapter>(null);

        public int StopCount { get; private set; }
        public int StartCount { get; private set; }

        public ReadOnlyReactiveProperty<EngineState> State => _state;
        public ReadOnlyReactiveProperty<IStep> CurrentStep => _currentStep;
        public ReadOnlyReactiveProperty<IChapter> CurrentChapter => _currentChapter;

        public void SetState(EngineState state) => _state.Value = state;
        public void SetCurrentStep(IStep step) => _currentStep.Value = step;
        public void SetCurrentChapter(IChapter chapter) => _currentChapter.Value = chapter;

        public UniTask StartProcessAsync(IProcess process, CancellationToken ct) { StartCount++; return UniTask.CompletedTask; }
        public UniTask StopAsync() { StopCount++; return UniTask.CompletedTask; }
        public void Dispose() { _state.Dispose(); _currentStep.Dispose(); _currentChapter.Dispose(); }
    }
}
