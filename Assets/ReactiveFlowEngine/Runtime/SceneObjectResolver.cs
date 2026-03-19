using System.Collections.Generic;
using UnityEngine;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Runtime
{
    public class SceneObjectResolver : ISceneObjectResolver
    {
        private readonly Dictionary<string, Transform> _cache = new Dictionary<string, Transform>();

        public Transform Resolve(string guid)
        {
            if (string.IsNullOrEmpty(guid))
                return null;

            // Check cache first
            if (_cache.TryGetValue(guid, out var cached))
            {
                if (cached != null) return cached;
                // Cached object was destroyed, remove from cache
                _cache.Remove(guid);
            }

            // Search all RfeSceneObjectId components in the scene
            var sceneObjects = Object.FindObjectsByType<RfeSceneObjectId>(FindObjectsSortMode.None);
            foreach (var obj in sceneObjects)
            {
                if (obj.Guid == guid)
                {
                    _cache[guid] = obj.transform;
                    return obj.transform;
                }
            }

            Debug.LogWarning($"[RFE] Scene object with GUID '{guid}' not found.");
            return null;
        }

        public void ClearCache()
        {
            _cache.Clear();
        }
    }

    /// <summary>
    /// Attach this component to GameObjects in the scene that should be resolvable by the RFE.
    /// Set the Guid to match the GUID in the process JSON.
    /// </summary>
    [AddComponentMenu("Reactive Flow Engine/Scene Object ID")]
    public class RfeSceneObjectId : MonoBehaviour
    {
        [SerializeField]
        private string _guid;

        public string Guid
        {
            get => _guid;
            set => _guid = value;
        }
    }
}
