using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;
using VContainer;

namespace ReactiveFlowEngine.State
{
    public class StateStore : IStateStore
    {
        private readonly Dictionary<string, StepSnapshot> _snapshots = new Dictionary<string, StepSnapshot>();
        private readonly List<string> _history = new List<string>();
        private readonly Dictionary<string, object> _globalState = new Dictionary<string, object>();
        private readonly int _maxSnapshots;
        private readonly object _lockObject = new object();

        [VContainer.Inject]
        public StateStore()
        {
            _maxSnapshots = 50;
        }

        public StepSnapshot CaptureSnapshot(IStep step)
        {
            if (step == null)
                return null;

            lock (_lockObject)
            {
                var snapshot = new StepSnapshot
                {
                    StepId = step.Id,
                    Timestamp = DateTimeOffset.UtcNow,
                    State = new Dictionary<string, object>(_globalState),
                    BehaviorStates = CaptureBehaviorStates(step)
                };

                _snapshots[step.Id] = snapshot;

                // Sliding window pruning - remove oldest snapshot if we exceed max
                if (_snapshots.Count > _maxSnapshots)
                {
                    var oldestKey = _snapshots.OrderBy(kvp => kvp.Value.Timestamp).First().Key;
                    _snapshots.Remove(oldestKey);
                }

                return snapshot;
            }
        }

        public async UniTask RestoreSnapshotAsync(StepSnapshot snapshot, CancellationToken ct)
        {
            if (snapshot == null)
                return;

            lock (_lockObject)
            {
                _globalState.Clear();
                foreach (var kvp in snapshot.State)
                {
                    _globalState[kvp.Key] = kvp.Value;
                }
            }

            await UniTask.CompletedTask;
        }

        public StepSnapshot GetSnapshot(string stepId)
        {
            if (stepId == null)
                return null;

            lock (_lockObject)
            {
                _snapshots.TryGetValue(stepId, out var snapshot);
                return snapshot;
            }
        }

        public IReadOnlyList<string> GetHistory()
        {
            lock (_lockObject)
            {
                return new List<string>(_history).AsReadOnly();
            }
        }

        public void PushHistory(string stepId)
        {
            if (stepId == null)
                return;

            lock (_lockObject)
            {
                _history.Add(stepId);
            }
        }

        public string PopHistory()
        {
            lock (_lockObject)
            {
                if (_history.Count == 0)
                    return null;

                var last = _history[_history.Count - 1];
                _history.RemoveAt(_history.Count - 1);
                return last;
            }
        }

        public void Clear()
        {
            lock (_lockObject)
            {
                _snapshots.Clear();
                _history.Clear();
                _globalState.Clear();
            }
        }

        public void SetGlobalState(string key, object value)
        {
            if (key == null)
                return;

            lock (_lockObject)
            {
                _globalState[key] = value;
            }
        }

        public object GetGlobalState(string key)
        {
            if (key == null)
                return null;

            lock (_lockObject)
            {
                _globalState.TryGetValue(key, out var value);
                return value;
            }
        }

        public bool HasGlobalState(string key)
        {
            if (key == null) return false;
            lock (_lockObject)
            {
                return _globalState.ContainsKey(key);
            }
        }

        public void RemoveGlobalState(string key)
        {
            if (key == null) return;
            lock (_lockObject)
            {
                _globalState.Remove(key);
            }
        }

        public Dictionary<string, object> GetAllGlobalState()
        {
            lock (_lockObject)
            {
                return new Dictionary<string, object>(_globalState);
            }
        }

        public void SetAllGlobalState(Dictionary<string, object> state)
        {
            lock (_lockObject)
            {
                _globalState.Clear();
                if (state != null)
                {
                    foreach (var kvp in state)
                        _globalState[kvp.Key] = kvp.Value;
                }
            }
        }

        private List<BehaviorSnapshot> CaptureBehaviorStates(IStep step)
        {
            var behaviorStates = new List<BehaviorSnapshot>();

            if (step?.Behaviors == null)
                return behaviorStates;

            foreach (var behavior in step.Behaviors)
            {
                if (behavior == null)
                    continue;

                // Check if behavior can capture state
                if (behavior is IStateCaptureBehavior captureBehavior)
                {
                    var state = captureBehavior.CaptureState();
                    if (state != null)
                    {
                        behaviorStates.Add(new BehaviorSnapshot
                        {
                            BehaviorType = behavior.GetType().Name,
                            Data = state
                        });
                    }
                }
            }

            return behaviorStates;
        }
    }
}
