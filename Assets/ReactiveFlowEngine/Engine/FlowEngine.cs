using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using ReactiveFlowEngine.Abstractions;
using UnityEngine;

namespace ReactiveFlowEngine.Engine
{
    public class FlowEngine : IFlowEngine, IEngineController
    {
        private readonly IStepRunner _stepRunner;
        private readonly IStateStore _stateStore;
        private readonly ChapterRunner _chapterRunner;
        private readonly IHistoryService _historyService;
        private CancellationTokenSource _processCts;

        private readonly ReactiveProperty<EngineState> _state = new ReactiveProperty<EngineState>(EngineState.Idle);
        private readonly ReactiveProperty<IStep> _currentStep = new ReactiveProperty<IStep>(null);
        private readonly ReactiveProperty<IChapter> _currentChapter = new ReactiveProperty<IChapter>(null);

        public ReadOnlyReactiveProperty<EngineState> State
        {
            get { return _state; }
        }

        public ReadOnlyReactiveProperty<IStep> CurrentStep
        {
            get { return _currentStep; }
        }

        public ReadOnlyReactiveProperty<IChapter> CurrentChapter
        {
            get { return _currentChapter; }
        }

        public FlowEngine(IStepRunner stepRunner, IStateStore stateStore,
                          ChapterRunner chapterRunner, IHistoryService historyService)
        {
            _stepRunner = stepRunner ?? throw new ArgumentNullException(nameof(stepRunner));
            _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
            _chapterRunner = chapterRunner ?? throw new ArgumentNullException(nameof(chapterRunner));
            _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
        }

        public async UniTask StartProcessAsync(IProcess process, CancellationToken ct)
        {
            if (process == null)
            {
                Debug.LogError("[RFE] Cannot start null process.");
                return;
            }

            if (process.Chapters == null || process.Chapters.Count == 0)
            {
                Debug.LogError("[RFE] Process has no chapters.");
                return;
            }

            _processCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            var processCt = _processCts.Token;

            _state.Value = EngineState.Running;
            _stateStore.Clear();
            _historyService.Clear();

            Debug.Log($"[RFE] Starting process: {process.Name} ({process.Id})");

            try
            {
                // Wire up ExecuteChapterBehavior delegates
                WireChapterBehaviors(process);

                // Run chapters sequentially
                for (int chapterIndex = 0; chapterIndex < process.Chapters.Count; chapterIndex++)
                {
                    var chapter = process.Chapters[chapterIndex];

                    if (chapter == null)
                        continue;

                    processCt.ThrowIfCancellationRequested();

                    _currentChapter.Value = chapter;
                    var currentStep = chapter.FirstStep;

                    while (currentStep != null)
                    {
                        processCt.ThrowIfCancellationRequested();

                        _currentStep.Value = currentStep;
                        _state.Value = EngineState.Running;

                        var transition = await _stepRunner.RunStepAsync(currentStep, processCt);

                        if (transition == null)
                        {
                            // Step was cancelled or no transition fired
                            Debug.Log("[RFE] Step returned no transition. Ending process.");
                            _state.Value = EngineState.Completed;
                            return;
                        }

                        // Transition occurred
                        _state.Value = EngineState.Transitioning;
                        _stateStore.CaptureSnapshot(currentStep);
                        _historyService.Push(new Navigation.HistoryEntry(chapter.Id, currentStep.Id));

                        if (transition.TargetStep == null)
                        {
                            // End of chapter
                            Debug.Log($"[RFE] End of chapter: {chapter.Name}");
                            break;
                        }

                        currentStep = transition.TargetStep;
                    }
                }

                _state.Value = EngineState.Completed;
                _currentStep.Value = null;
                _currentChapter.Value = null;
                Debug.Log("[RFE] Process completed successfully.");
            }
            catch (OperationCanceledException)
            {
                _state.Value = EngineState.Idle;
                Debug.Log("[RFE] Process was cancelled.");
            }
            catch (Exception ex)
            {
                _state.Value = EngineState.Idle;
                Debug.LogError($"[RFE] Process error: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                _processCts?.Dispose();
                _processCts = null;
            }
        }

        public UniTask StopAsync()
        {
            if (_processCts != null && !_processCts.IsCancellationRequested)
            {
                _processCts.Cancel();
            }

            _stepRunner.CancelCurrentStep();

            _state.Value = EngineState.Idle;
            _currentStep.Value = null;
            _currentChapter.Value = null;

            _processCts?.Dispose();
            _processCts = null;

            return UniTask.CompletedTask;
        }

        // IEngineController implementation
        public void SetCurrentStep(IStep step)
        {
            _currentStep.Value = step;
        }

        public void SetCurrentChapter(IChapter chapter)
        {
            _currentChapter.Value = chapter;
        }

        public void Dispose()
        {
            _state?.Dispose();
            _currentStep?.Dispose();
            _currentChapter?.Dispose();
            _processCts?.Dispose();
        }

        private void WireChapterBehaviors(IProcess process)
        {
            if (process?.Chapters == null)
                return;

            for (int i = 0; i < process.Chapters.Count; i++)
            {
                var chapter = process.Chapters[i];
                if (chapter != null)
                {
                    WireChapterBehaviorsInChapter(chapter);
                }
            }
        }

        private void WireChapterBehaviorsInChapter(IChapter chapter)
        {
            if (chapter?.Steps == null)
                return;

            for (int i = 0; i < chapter.Steps.Count; i++)
            {
                var step = chapter.Steps[i];
                if (step == null || step.Behaviors == null)
                    continue;

                for (int j = 0; j < step.Behaviors.Count; j++)
                {
                    var behavior = step.Behaviors[j];
                    if (behavior == null)
                        continue;

                    if (behavior is IExecuteChapterBehavior execBehavior)
                    {
                        execBehavior.SetChapterRunner(async (subChapter, ct) =>
                        {
                            await _chapterRunner.RunAsync(subChapter, ct);
                        });

                        // Recursively wire sub-chapter's behaviors
                        var subChapter = execBehavior.GetSubChapter();
                        if (subChapter != null)
                        {
                            WireChapterBehaviorsInChapter(subChapter);
                        }
                    }
                }
            }
        }
    }
}
