using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Runtime.UI
{
    [AddComponentMenu("Reactive Flow Engine/Navigation UI")]
    public class RfeNavigationUI : MonoBehaviour
    {
        [Inject] private INavigationService _navigationService;
        [Inject] private IFlowEngine _engine;

        [Header("Buttons")]
        [SerializeField] private Button _previousButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _stopButton;

        private void OnEnable()
        {
            if (_previousButton != null) _previousButton.onClick.AddListener(OnPreviousClicked);
            if (_nextButton != null) _nextButton.onClick.AddListener(OnNextClicked);
            if (_restartButton != null) _restartButton.onClick.AddListener(OnRestartClicked);
            if (_stopButton != null) _stopButton.onClick.AddListener(OnStopClicked);
        }

        private void OnDisable()
        {
            if (_previousButton != null) _previousButton.onClick.RemoveListener(OnPreviousClicked);
            if (_nextButton != null) _nextButton.onClick.RemoveListener(OnNextClicked);
            if (_restartButton != null) _restartButton.onClick.RemoveListener(OnRestartClicked);
            if (_stopButton != null) _stopButton.onClick.RemoveListener(OnStopClicked);
        }

        private void Update()
        {
            if (_navigationService == null) return;

            if (_previousButton != null)
                _previousButton.interactable = _navigationService.CanGoBack;

            if (_nextButton != null)
                _nextButton.interactable = _engine?.State.CurrentValue == EngineState.Running;

            if (_stopButton != null)
                _stopButton.interactable = _engine?.State.CurrentValue == EngineState.Running;
        }

        private void OnPreviousClicked()
        {
            if (_navigationService?.CanGoBack == true)
                _navigationService.PreviousStepAsync(CancellationToken.None).Forget();
        }

        private void OnNextClicked()
        {
            _navigationService?.NextStepAsync(CancellationToken.None).Forget();
        }

        private void OnRestartClicked()
        {
            _navigationService?.RestartStepAsync(CancellationToken.None).Forget();
        }

        private void OnStopClicked()
        {
            _engine?.StopAsync().Forget();
        }
    }
}
