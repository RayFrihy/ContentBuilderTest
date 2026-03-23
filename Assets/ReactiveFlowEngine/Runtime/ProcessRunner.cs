using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Navigation;
using UnityEngine;

namespace ReactiveFlowEngine.Runtime
{
    public class ProcessRunner : IAsyncStartable, IDisposable
    {
        private readonly IProcessLoader _loader;
        private readonly IFlowEngine _engine;
        private readonly NavigationService _navigationService;
        private readonly IStepGuidanceService _guidanceService;
        private readonly TextAsset _processJson;
        private CancellationTokenSource _cts;

        [Inject]
        public ProcessRunner(
            IProcessLoader loader,
            IFlowEngine engine,
            NavigationService navigationService,
            IStepGuidanceService guidanceService,
            TextAsset processJson = null)
        {
            _loader = loader;
            _engine = engine;
            _navigationService = navigationService;
            _guidanceService = guidanceService;
            _processJson = processJson;
        }

        public async UniTask StartAsync(CancellationToken ct)
        {
            if (_processJson == null)
            {
                Debug.LogWarning("[RFE] No process JSON assigned. ProcessRunner will not start automatically.");
                return;
            }

            _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

            try
            {
                var process = await _loader.LoadAsync(_processJson.text, _cts.Token);
                if (process == null)
                {
                    Debug.LogError("[RFE] Failed to load process from JSON.");
                    return;
                }

                _navigationService.SetCurrentProcess(process);
                _guidanceService.Enable();
                await _engine.StartProcessAsync(process, _cts.Token);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[RFE] ProcessRunner was cancelled.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RFE] ProcessRunner error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
    }
}
