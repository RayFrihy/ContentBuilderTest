using UnityEngine;
using VContainer;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Conditions.Environment;

namespace ReactiveFlowEngine.Runtime
{
    [AddComponentMenu("Reactive Flow Engine/Trigger Reporter")]
    [RequireComponent(typeof(Collider))]
    public class RfeTriggerReporter : MonoBehaviour
    {
        [Inject] private IEventBus _eventBus;

        private RfeSceneObjectId _sceneObjectId;

        private void Awake()
        {
            _sceneObjectId = GetComponent<RfeSceneObjectId>();
            if (_sceneObjectId == null)
                Debug.LogWarning("[RFE] RfeTriggerReporter: No RfeSceneObjectId found on this GameObject.");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_eventBus == null || _sceneObjectId == null) return;

            var otherSceneId = other.GetComponentInParent<RfeSceneObjectId>();
            if (otherSceneId == null) return;

            var data = new TriggerEventData(_sceneObjectId.Guid, otherSceneId.Guid);
            _eventBus.Publish("TriggerEnter", data);
        }

        private void OnTriggerExit(Collider other)
        {
            if (_eventBus == null || _sceneObjectId == null) return;

            var otherSceneId = other.GetComponentInParent<RfeSceneObjectId>();
            if (otherSceneId == null) return;

            var data = new TriggerEventData(_sceneObjectId.Guid, otherSceneId.Guid);
            _eventBus.Publish("TriggerExit", data);
        }
    }
}
