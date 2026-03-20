using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using ReactiveFlowEngine.Abstractions;
using UnityEngine;

namespace ReactiveFlowEngine.Navigation
{
    public class NavigationService : INavigationService
    {
        private readonly IFlowEngine _engine;
        private readonly IEngineController _engineController;
        private readonly IStepRunner _stepRunner;
        private readonly IStateStore _stateStore;
        private readonly IHistoryService _historyService;
        private readonly Subject<NavigationEvent> _onNavigated = new Subject<NavigationEvent>();
        private readonly SemaphoreSlim _navigationLock = new SemaphoreSlim(1, 1);

        private IProcess _currentProcess;
        private bool _disposed = false;

        public Observable<NavigationEvent> OnNavigated
        {
            get { return _onNavigated; }
        }

        public bool CanGoBack
        {
            get { return _historyService.CanGoBack; }
        }

        public bool CanGoForward
        {
            get
            {
                var step = _engine?.CurrentStep.CurrentValue;
                return step != null && _engine?.State.CurrentValue == EngineState.Running;
            }
        }

        public NavigationService(IFlowEngine engine, IEngineController engineController,
                                  IStepRunner stepRunner, IStateStore stateStore,
                                  IHistoryService historyService)
        {
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
            _engineController = engineController ?? throw new ArgumentNullException(nameof(engineController));
            _stepRunner = stepRunner ?? throw new ArgumentNullException(nameof(stepRunner));
            _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
            _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
        }

        public void SetCurrentProcess(IProcess process)
        {
            _currentProcess = process;
            _historyService.Clear();
        }

        public async UniTask NextStepAsync(CancellationToken ct)
        {
            ThrowIfDisposed();

            if (!await _navigationLock.WaitAsync(0, ct))
            {
                Debug.LogWarning("[RFE] Navigation already in progress, ignoring NextStep.");
                return;
            }

            try
            {
                var currentStep = _engine.CurrentStep.CurrentValue;
                if (currentStep == null)
                    return;

                // Cancel current step to force immediate transition to next
                _stepRunner.CancelCurrentStep();

                EmitNavigationEvent(NavigationType.Forward, currentStep.Id, null);
                Debug.Log($"[RFE] NextStep invoked from: {currentStep.Name}");
            }
            finally
            {
                _navigationLock.Release();
            }
        }

        public async UniTask PreviousStepAsync(CancellationToken ct)
        {
            ThrowIfDisposed();

            if (!await _navigationLock.WaitAsync(0, ct))
            {
                Debug.LogWarning("[RFE] Navigation already in progress, ignoring PreviousStep.");
                return;
            }

            try
            {
                var currentStep = _engine.CurrentStep.CurrentValue;
                if (currentStep == null)
                {
                    Debug.LogWarning("[RFE] Cannot go back: no current step.");
                    return;
                }

                if (!_historyService.CanGoBack)
                {
                    Debug.LogWarning("[RFE] Cannot go back: no history.");
                    return;
                }

                // Cancel current step execution
                _stepRunner.CancelCurrentStep();

                // Undo current step's behaviors (reverse order)
                await UndoStepBehaviorsAsync(currentStep, ct);

                // Pop from history stack
                var previousEntry = _historyService.Pop();
                if (previousEntry == null)
                {
                    Debug.LogWarning("[RFE] Previous entry is null.");
                    return;
                }

                var previousStepId = previousEntry.StepId;

                // Restore snapshot
                var snapshot = _stateStore.GetSnapshot(previousStepId);
                if (snapshot != null)
                {
                    await _stateStore.RestoreSnapshotAsync(snapshot, ct);
                }

                // Navigate to previous step
                var previousStep = FindStep(previousStepId);
                if (previousStep != null)
                {
                    _engineController.SetCurrentStep(previousStep);
                    EmitNavigationEvent(NavigationType.Reverse, currentStep.Id, previousStepId);
                    Debug.Log($"[RFE] Navigated back to step: {previousStep.Name}");
                }
                else
                {
                    Debug.LogWarning($"[RFE] Could not find previous step: {previousStepId}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RFE] Error in PreviousStep: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                _navigationLock.Release();
            }
        }

        public async UniTask GoToStepAsync(string stepId, CancellationToken ct)
        {
            ThrowIfDisposed();

            if (string.IsNullOrEmpty(stepId))
            {
                Debug.LogError("[RFE] GoToStep: stepId is null or empty.");
                return;
            }

            if (!await _navigationLock.WaitAsync(0, ct))
            {
                Debug.LogWarning("[RFE] Navigation already in progress, ignoring GoToStep.");
                return;
            }

            try
            {
                var currentStep = _engine.CurrentStep.CurrentValue;
                if (currentStep == null)
                    return;

                var targetStep = FindStep(stepId);
                if (targetStep == null)
                {
                    Debug.LogError($"[RFE] GoToStep: Target step '{stepId}' not found.");
                    return;
                }

                // Cancel current step
                _stepRunner.CancelCurrentStep();

                // Check if target is in history (behind us)
                if (_historyService.Contains(stepId))
                {
                    // Unwind backwards to target
                    await UnwindToStepAsync(stepId, ct);
                }
                else
                {
                    // Jump forward - record current position
                    _stateStore.CaptureSnapshot(currentStep);
                    _historyService.Push(new HistoryEntry(
                        _engine.CurrentChapter.CurrentValue?.Id, currentStep.Id));
                }

                _engineController.SetCurrentStep(targetStep);
                EmitNavigationEvent(NavigationType.Jump, currentStep.Id, stepId);
                Debug.Log($"[RFE] Jumped to step: {targetStep.Name}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RFE] Error in GoToStep: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                _navigationLock.Release();
            }
        }

        public async UniTask JumpToChapterAsync(string chapterId, CancellationToken ct)
        {
            ThrowIfDisposed();

            if (string.IsNullOrEmpty(chapterId))
            {
                Debug.LogError("[RFE] JumpToChapter: chapterId is null or empty.");
                return;
            }

            if (!await _navigationLock.WaitAsync(0, ct))
            {
                Debug.LogWarning("[RFE] Navigation already in progress, ignoring JumpToChapter.");
                return;
            }

            try
            {
                if (_currentProcess == null)
                {
                    Debug.LogWarning("[RFE] JumpToChapter: No current process set.");
                    return;
                }

                var currentStep = _engine.CurrentStep.CurrentValue;
                _stepRunner.CancelCurrentStep();

                // Find target chapter
                IChapter targetChapter = null;
                for (int i = 0; i < _currentProcess.Chapters.Count; i++)
                {
                    var chapter = _currentProcess.Chapters[i];
                    if (chapter != null && chapter.Id == chapterId)
                    {
                        targetChapter = chapter;
                        break;
                    }
                }

                if (targetChapter == null)
                {
                    Debug.LogError($"[RFE] JumpToChapter: Chapter '{chapterId}' not found.");
                    return;
                }

                if (targetChapter.FirstStep == null)
                {
                    Debug.LogWarning($"[RFE] JumpToChapter: Target chapter has no FirstStep.");
                    return;
                }

                _engineController.SetCurrentChapter(targetChapter);
                _engineController.SetCurrentStep(targetChapter.FirstStep);

                EmitNavigationEvent(NavigationType.Jump,
                    currentStep?.Id, targetChapter.FirstStep?.Id);
                Debug.Log($"[RFE] Jumped to chapter: {targetChapter.Name}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RFE] Error in JumpToChapter: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                _navigationLock.Release();
            }
        }

        public async UniTask RestartStepAsync(CancellationToken ct)
        {
            ThrowIfDisposed();

            if (!await _navigationLock.WaitAsync(0, ct))
            {
                Debug.LogWarning("[RFE] Navigation already in progress, ignoring RestartStep.");
                return;
            }

            try
            {
                var currentStep = _engine.CurrentStep.CurrentValue;
                if (currentStep == null)
                    return;

                // Cancel current step
                _stepRunner.CancelCurrentStep();

                // Undo current step's behaviors
                await UndoStepBehaviorsAsync(currentStep, ct);

                // Restore the entry snapshot (snapshot from before entering this step)
                var entries = _historyService.GetAll();
                if (entries.Count > 0)
                {
                    var previousStepId = entries[entries.Count - 1].StepId;
                    var snapshot = _stateStore.GetSnapshot(previousStepId);
                    if (snapshot != null)
                    {
                        await _stateStore.RestoreSnapshotAsync(snapshot, ct);
                    }
                }

                // Re-enter the same step (engine will re-run it)
                _engineController.SetCurrentStep(currentStep);
                EmitNavigationEvent(NavigationType.Restart, currentStep.Id, currentStep.Id);
                Debug.Log($"[RFE] Restarted step: {currentStep.Name}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RFE] Error in RestartStep: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                _navigationLock.Release();
            }
        }

        public void OnStepCompleted(IStep step, IChapter chapter)
        {
            if (step == null)
                return;

            _historyService.Push(new HistoryEntry(chapter?.Id, step.Id));
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _navigationLock?.Dispose();
            _onNavigated?.Dispose();
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(NavigationService));
        }

        private async UniTask UndoStepBehaviorsAsync(IStep step, CancellationToken ct)
        {
            if (step?.Behaviors == null)
                return;

            // Undo in reverse order
            for (int i = step.Behaviors.Count - 1; i >= 0; i--)
            {
                var behavior = step.Behaviors[i];
                if (behavior == null)
                    continue;

                if (behavior is IReversibleBehavior reversible)
                {
                    try
                    {
                        await reversible.UndoAsync(ct);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[RFE] Error undoing behavior: {ex.Message}");
                    }
                }
            }
        }

        private async UniTask UnwindToStepAsync(string targetStepId, CancellationToken ct)
        {
            // Iteratively undo steps from current back to target
            while (_historyService.CanGoBack)
            {
                var entry = _historyService.Peek();
                if (entry == null || entry.StepId == targetStepId)
                    break;

                _historyService.Pop();

                var step = FindStep(entry.StepId);
                if (step != null)
                {
                    try
                    {
                        await UndoStepBehaviorsAsync(step, ct);
                        var snapshot = _stateStore.GetSnapshot(entry.StepId);
                        if (snapshot != null)
                        {
                            await _stateStore.RestoreSnapshotAsync(snapshot, ct);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[RFE] Error unwinding to step: {ex.Message}");
                    }
                }
            }
        }

        private IStep FindStep(string stepId)
        {
            if (string.IsNullOrEmpty(stepId) || _currentProcess == null)
                return null;

            if (_currentProcess.Chapters == null)
                return null;

            for (int i = 0; i < _currentProcess.Chapters.Count; i++)
            {
                var chapter = _currentProcess.Chapters[i];
                if (chapter != null)
                {
                    var step = FindStepInChapter(chapter, stepId);
                    if (step != null)
                        return step;
                }
            }
            return null;
        }

        private IStep FindStepInChapter(IChapter chapter, string stepId)
        {
            if (chapter?.Steps == null)
                return null;

            for (int i = 0; i < chapter.Steps.Count; i++)
            {
                var step = chapter.Steps[i];
                if (step == null)
                    continue;

                if (step.Id == stepId)
                    return step;

                // Check sub-chapter steps
                if (step.SubChapter != null)
                {
                    var found = FindStepInChapter(step.SubChapter, stepId);
                    if (found != null)
                        return found;
                }
            }
            return null;
        }

        private void EmitNavigationEvent(NavigationType type, string fromStepId, string toStepId)
        {
            var @event = new NavigationEvent(type, fromStepId, toStepId, DateTimeOffset.UtcNow);
            _onNavigated.OnNext(@event);
        }
    }
}
