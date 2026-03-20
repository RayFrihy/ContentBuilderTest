using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;

namespace ReactiveFlowEngine.Tests.TestDoubles
{
    public class MockStateStore : IStateStore
    {
        private readonly Dictionary<string, StepSnapshot> _snapshots = new Dictionary<string, StepSnapshot>();
        private readonly Dictionary<string, object> _globalState = new Dictionary<string, object>();

        public List<string> CapturedStepIds { get; } = new List<string>();

        public StepSnapshot CaptureSnapshot(IStep step)
        {
            if (step == null) return null;
            CapturedStepIds.Add(step.Id);

            var snapshot = new StepSnapshot
            {
                StepId = step.Id,
                State = new Dictionary<string, object>(_globalState)
            };
            _snapshots[step.Id] = snapshot;
            return snapshot;
        }

        public UniTask RestoreSnapshotAsync(StepSnapshot snapshot, CancellationToken ct)
        {
            if (snapshot == null) return UniTask.CompletedTask;

            _globalState.Clear();
            foreach (var kvp in snapshot.State)
                _globalState[kvp.Key] = kvp.Value;

            return UniTask.CompletedTask;
        }

        public StepSnapshot GetSnapshot(string stepId)
        {
            if (stepId != null && _snapshots.TryGetValue(stepId, out var snapshot))
                return snapshot;
            return null;
        }

        public void Clear()
        {
            _snapshots.Clear();
            _globalState.Clear();
        }

        public void SetGlobalState(string key, object value)
        {
            if (key != null) _globalState[key] = value;
        }

        public object GetGlobalState(string key)
        {
            if (key != null && _globalState.TryGetValue(key, out var value))
                return value;
            return null;
        }

        public bool HasGlobalState(string key)
        {
            return key != null && _globalState.ContainsKey(key);
        }

        public void RemoveGlobalState(string key)
        {
            if (key != null) _globalState.Remove(key);
        }

        public Dictionary<string, object> GetAllGlobalState()
        {
            return new Dictionary<string, object>(_globalState);
        }

        public void SetAllGlobalState(Dictionary<string, object> state)
        {
            _globalState.Clear();
            if (state != null)
                foreach (var kvp in state)
                    _globalState[kvp.Key] = kvp.Value;
        }
    }
}
