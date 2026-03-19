using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Model;

namespace ReactiveFlowEngine.Abstractions
{
    public interface IStateStore
    {
        StepSnapshot CaptureSnapshot(IStep step);
        UniTask RestoreSnapshotAsync(StepSnapshot snapshot, CancellationToken ct);
        StepSnapshot GetSnapshot(string stepId);
        IReadOnlyList<string> GetHistory();
        void PushHistory(string stepId);
        string PopHistory();
        void Clear();
    }
}
