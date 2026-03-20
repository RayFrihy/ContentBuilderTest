using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Navigation;
using UnityEngine;

namespace ReactiveFlowEngine.Engine
{
    public class ChapterRunner
    {
        private readonly IStepRunner _stepRunner;
        private readonly IStateStore _stateStore;
        private readonly IHistoryService _historyService;

        public ChapterRunner(IStepRunner stepRunner, IStateStore stateStore, IHistoryService historyService)
        {
            _stepRunner = stepRunner ?? throw new System.ArgumentNullException(nameof(stepRunner));
            _stateStore = stateStore ?? throw new System.ArgumentNullException(nameof(stateStore));
            _historyService = historyService ?? throw new System.ArgumentNullException(nameof(historyService));
        }

        public async UniTask RunAsync(IChapter chapter, CancellationToken ct)
        {
            if (chapter == null)
            {
                Debug.LogWarning("[RFE] ChapterRunner: Chapter is null.");
                return;
            }

            if (chapter.FirstStep == null)
            {
                Debug.LogWarning("[RFE] ChapterRunner: Chapter FirstStep is null.");
                return;
            }

            Debug.Log($"[RFE] Starting chapter: {chapter.Name} ({chapter.Id})");

            var currentStep = chapter.FirstStep;

            while (currentStep != null)
            {
                ct.ThrowIfCancellationRequested();

                var transition = await _stepRunner.RunStepAsync(currentStep, ct);

                if (transition == null)
                {
                    // Step was cancelled or no transition fired
                    Debug.Log($"[RFE] Chapter {chapter.Name}: Step {currentStep.Name} returned no transition. Ending chapter.");
                    break;
                }

                // Capture snapshot and push history
                _stateStore.CaptureSnapshot(currentStep);
                _historyService.Push(new HistoryEntry(chapter.Id, currentStep.Id));

                if (transition.TargetStep == null)
                {
                    // End of chapter
                    Debug.Log($"[RFE] Chapter {chapter.Name} completed.");
                    break;
                }

                currentStep = transition.TargetStep;
            }

            Debug.Log($"[RFE] Chapter {chapter.Name} run completed.");
        }
    }
}
