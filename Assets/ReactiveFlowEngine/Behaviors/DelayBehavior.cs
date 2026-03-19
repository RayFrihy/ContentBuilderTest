using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Behaviors
{
    public class DelayBehavior : IBehavior
    {
        private readonly float _duration;

        public ExecutionStages Stages => ExecutionStages.Activation;
        public bool IsBlocking => true;

        public DelayBehavior(float duration)
        {
            _duration = duration;
        }

        public async UniTask ExecuteAsync(CancellationToken ct)
        {
            if (_duration <= 0f) return;
            await UniTask.Delay(TimeSpan.FromSeconds(_duration), cancellationToken: ct);
        }
    }
}
