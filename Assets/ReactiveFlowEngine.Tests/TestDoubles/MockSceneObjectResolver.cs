using System.Collections.Generic;
using ReactiveFlowEngine.Abstractions;
using UnityEngine;

namespace ReactiveFlowEngine.Tests.TestDoubles
{
    public class MockSceneObjectResolver : ISceneObjectResolver
    {
        private readonly Dictionary<string, Transform> _objects = new Dictionary<string, Transform>();
        private readonly List<string> _resolvedGuids = new List<string>();

        public IReadOnlyList<string> ResolvedGuids => _resolvedGuids;

        public void Register(string guid, Transform transform)
        {
            _objects[guid] = transform;
        }

        public Transform Resolve(string guid)
        {
            _resolvedGuids.Add(guid);
            if (guid != null && _objects.TryGetValue(guid, out var transform))
                return transform;
            return null;
        }
    }
}
