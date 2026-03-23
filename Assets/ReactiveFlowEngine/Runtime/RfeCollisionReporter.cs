using UnityEngine;
using VContainer;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Conditions.Environment;

namespace ReactiveFlowEngine.Runtime
{
    [AddComponentMenu("Reactive Flow Engine/Collision Reporter")]
    [RequireComponent(typeof(Collider))]
    public class RfeCollisionReporter : MonoBehaviour
    {
        [Inject] private IEventBus _eventBus;

        private RfeSceneObjectId _sceneObjectId;

        private void Awake()
        {
            _sceneObjectId = GetComponent<RfeSceneObjectId>();
            if (_sceneObjectId == null)
                Debug.LogWarning("[RFE] RfeCollisionReporter: No RfeSceneObjectId found on this GameObject.");
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_eventBus == null || _sceneObjectId == null) return;

            var otherSceneId = collision.gameObject.GetComponentInParent<RfeSceneObjectId>();
            if (otherSceneId == null) return;

            var data = new CollisionEventData(_sceneObjectId.Guid, otherSceneId.Guid);
            _eventBus.Publish("CollisionEnter", data);
        }

        private void OnCollisionExit(Collision collision)
        {
            if (_eventBus == null || _sceneObjectId == null) return;

            var otherSceneId = collision.gameObject.GetComponentInParent<RfeSceneObjectId>();
            if (otherSceneId == null) return;

            var data = new CollisionEventData(_sceneObjectId.Guid, otherSceneId.Guid);
            _eventBus.Publish("CollisionExit", data);
        }
    }
}
