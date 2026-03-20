using UnityEngine;
using VContainer;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Navigation;
using System.Threading;

namespace ReactiveFlowEngine.Runtime
{
    public class RfeDebugUI : MonoBehaviour
    {
        [Inject] private IFlowEngine _engine;
        [Inject] private INavigationService _navigationService;
        [Inject] private IHistoryService _historyService;

        private string _currentStepName = "None";
        private string _currentChapterName = "None";
        private string _engineState = "Idle";
        private int _historyCount = 0;
        private bool _canGoBack = false;

        private void Update()
        {
            if (_engine == null) return;

            var step = _engine.CurrentStep.CurrentValue;
            var chapter = _engine.CurrentChapter.CurrentValue;
            _currentStepName = step?.Name ?? "None";
            _currentChapterName = chapter?.Name ?? "None";
            _engineState = _engine.State.CurrentValue.ToString();
            _historyCount = _historyService?.GetAll()?.Count ?? 0;
            _canGoBack = _navigationService?.CanGoBack ?? false;
        }

        private void OnGUI()
        {
            if (_engine == null) return;

            var boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.alignment = TextAnchor.UpperLeft;
            boxStyle.fontSize = 14;
            boxStyle.padding = new RectOffset(10, 10, 10, 10);

            GUILayout.BeginArea(new Rect(10, 10, 350, 300));
            GUILayout.BeginVertical(boxStyle);

            GUILayout.Label($"<b>Reactive Flow Engine</b>", new GUIStyle(GUI.skin.label) { richText = true, fontSize = 16 });
            GUILayout.Space(5);
            GUILayout.Label($"State: {_engineState}");
            GUILayout.Label($"Chapter: {_currentChapterName}");
            GUILayout.Label($"Step: {_currentStepName}");
            GUILayout.Label($"History: {_historyCount} entries");
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Previous", GUILayout.Width(80)) && _canGoBack)
            {
                _navigationService.PreviousStepAsync(CancellationToken.None).Forget();
            }

            if (GUILayout.Button("Restart", GUILayout.Width(80)))
            {
                _navigationService.RestartStepAsync(CancellationToken.None).Forget();
            }

            if (GUILayout.Button("Next", GUILayout.Width(80)))
            {
                _navigationService.NextStepAsync(CancellationToken.None).Forget();
            }

            GUILayout.EndHorizontal();

            if (GUILayout.Button("Stop Process", GUILayout.Width(120)))
            {
                _engine.StopAsync().Forget();
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
