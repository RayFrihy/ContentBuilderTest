using UnityEngine;
using VContainer;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Runtime.Input
{
    [AddComponentMenu("Reactive Flow Engine/Mouse Input Provider")]
    public class MouseInputProvider : MonoBehaviour, IInputProvider
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private float _maxRayDistance = 100f;
        [SerializeField] private LayerMask _interactableMask = -1;

        [Inject] private IEventBus _eventBus;

        private string _hoveredObjectId;
        private string _grabbedObjectId;
        private bool _isActive = true;

        public bool IsActive => _isActive;

        public void Enable() => _isActive = true;

        public void Disable()
        {
            _isActive = false;
            ClearHover();
            ClearGrab();
        }

        private void Awake()
        {
            if (_camera == null)
                _camera = Camera.main;

            if (_camera == null)
                Debug.LogWarning("[RFE] MouseInputProvider: No camera assigned and Camera.main is null.");
        }

        private void Update()
        {
            if (!_isActive || _eventBus == null || _camera == null) return;

            ProcessHover();
            ProcessClick();
        }

        private void ProcessHover()
        {
            string newHoveredId = null;

            if (TryRaycast(out var hit))
            {
                if (TryGetInteractable(hit, out var sceneId, out var interactable))
                {
                    if (interactable.SupportsInteraction(InteractionFlags.Hoverable))
                    {
                        newHoveredId = sceneId.Guid;
                    }
                }
            }

            if (newHoveredId == _hoveredObjectId) return;

            if (_hoveredObjectId != null)
            {
                _eventBus.Publish("ObjectHoverExit", _hoveredObjectId);
            }

            _hoveredObjectId = newHoveredId;

            if (_hoveredObjectId != null)
            {
                _eventBus.Publish("ObjectHoverEnter", _hoveredObjectId);
            }
        }

        private void ProcessClick()
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                if (_hoveredObjectId != null && TryRaycast(out var hit))
                {
                    if (TryGetInteractable(hit, out var sceneId, out var interactable))
                    {
                        var guid = sceneId.Guid;

                        if (interactable.SupportsInteraction(InteractionFlags.Grabbable))
                        {
                            _grabbedObjectId = guid;
                            _eventBus.Publish("ObjectGrabbed", guid);
                        }

                        if (interactable.SupportsInteraction(InteractionFlags.Touchable))
                        {
                            _eventBus.Publish("ObjectTouched", guid);
                        }

                        if (interactable.SupportsInteraction(InteractionFlags.Selectable))
                        {
                            _eventBus.Publish("ObjectSelected", guid);
                        }
                    }
                }
            }

            if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                if (_grabbedObjectId != null)
                {
                    _eventBus.Publish("ObjectReleased", _grabbedObjectId);
                    _grabbedObjectId = null;
                }
            }
        }

        protected virtual bool TryRaycast(out RaycastHit hit)
        {
            var ray = _camera.ScreenPointToRay(UnityEngine.Input.mousePosition);
            return Physics.Raycast(ray, out hit, _maxRayDistance, _interactableMask);
        }

        private static bool TryGetInteractable(RaycastHit hit, out RfeSceneObjectId sceneId, out RfeInteractable interactable)
        {
            sceneId = hit.collider.GetComponentInParent<RfeSceneObjectId>();
            interactable = null;

            if (sceneId == null) return false;

            interactable = sceneId.GetComponent<RfeInteractable>();
            return interactable != null;
        }

        private void ClearHover()
        {
            if (_hoveredObjectId != null && _eventBus != null)
            {
                _eventBus.Publish("ObjectHoverExit", _hoveredObjectId);
                _hoveredObjectId = null;
            }
        }

        private void ClearGrab()
        {
            if (_grabbedObjectId != null && _eventBus != null)
            {
                _eventBus.Publish("ObjectReleased", _grabbedObjectId);
                _grabbedObjectId = null;
            }
        }
    }
}
