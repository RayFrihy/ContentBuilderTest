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

        // Global state management
        void SetGlobalState(string key, object value);
        object GetGlobalState(string key);
        bool HasGlobalState(string key);
        void RemoveGlobalState(string key);
        Dictionary<string, object> GetAllGlobalState();
        void SetAllGlobalState(Dictionary<string, object> state);
    }
}
