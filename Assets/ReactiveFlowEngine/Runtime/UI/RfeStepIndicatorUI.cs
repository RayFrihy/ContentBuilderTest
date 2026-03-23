using System;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Runtime.UI
{
    [AddComponentMenu("Reactive Flow Engine/Step Indicator UI")]
    public class RfeStepIndicatorUI : MonoBehaviour
    {
        [Inject] private IFlowEngine _engine;

        [Header("UI References")]
        [SerializeField] private Text _stepNameText;
        [SerializeField] private Text _chapterNameText;
        [SerializeField] private Text _progressText;
        [SerializeField] private Text _stateText;

        private IDisposable _subscriptions;

        private void OnEnable()
        {
            if (_engine == null) return;

            var composite = new CompositeDisposable();

            composite.Add(_engine.CurrentStep.Subscribe(step =>
            {
                if (_stepNameText != null)
                    _stepNameText.text = step?.Name ?? "---";

                UpdateProgress();
            }));

            composite.Add(_engine.CurrentChapter.Subscribe(chapter =>
            {
                if (_chapterNameText != null)
                    _chapterNameText.text = chapter?.Name ?? "---";

                UpdateProgress();
            }));

            composite.Add(_engine.State.Subscribe(state =>
            {
                if (_stateText != null)
                    _stateText.text = state.ToString();
            }));

            _subscriptions = composite;
        }

        private void OnDisable()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
        }

        private void UpdateProgress()
        {
            if (_progressText == null) return;

            var chapter = _engine.CurrentChapter.CurrentValue;
            var step = _engine.CurrentStep.CurrentValue;

            if (chapter == null || step == null)
            {
                _progressText.text = "";
                return;
            }

            int totalSteps = chapter.Steps.Count;
            int currentIndex = -1;
            for (int i = 0; i < chapter.Steps.Count; i++)
            {
                if (chapter.Steps[i].Id == step.Id)
                {
                    currentIndex = i;
                    break;
                }
            }

            if (currentIndex >= 0)
                _progressText.text = $"Step {currentIndex + 1} of {totalSteps}";
            else
                _progressText.text = $"{totalSteps} steps";
        }
    }
}
