using System;
using System.Collections.Generic;
using R3;
using UnityEngine;
using VContainer;
using ReactiveFlowEngine.Abstractions;

namespace ReactiveFlowEngine.Runtime
{
    [AddComponentMenu("Reactive Flow Engine/Interactable")]
    [RequireComponent(typeof(RfeSceneObjectId))]
    public class RfeInteractable : MonoBehaviour
    {
        [SerializeField]
        private InteractionFlags _supportedInteractions = InteractionFlags.All;

        [Header("Hover Highlight")]
        [SerializeField]
        private bool _highlightOnHover = true;

        [SerializeField]
        private Color _hoverHighlightColor = new Color(1f, 0.9f, 0.4f, 1f);

        [SerializeField]
        private float _hoverHighlightIntensity = 0.5f;

        [Inject] private IEventBus _eventBus;

        private RfeSceneObjectId _sceneObjectId;
        private InteractionState _currentState = InteractionState.Idle;
        private IDisposable _subscriptions;
        private List<(Renderer renderer, MaterialPropertyBlock originalBlock)> _originalStates;
        private bool _isHoverHighlighted;
        private bool _isGuidanceHighlighted;
        private Color _guidanceColor;
        private float _guidanceIntensity;

        public InteractionFlags SupportedInteractions => _supportedInteractions;
        public InteractionState CurrentState => _currentState;
        public string ObjectId => _sceneObjectId?.Guid;

        public bool SupportsInteraction(InteractionFlags flag)
            => (_supportedInteractions & flag) != 0;

        private void Awake()
        {
            _sceneObjectId = GetComponent<RfeSceneObjectId>();
        }

        private void OnEnable()
        {
            if (_eventBus == null || _sceneObjectId == null) return;

            var guid = _sceneObjectId.Guid;
            if (string.IsNullOrEmpty(guid)) return;

            var composite = new CompositeDisposable();

            composite.Add(_eventBus.On("ObjectHoverEnter")
                .Where(payload => payload is string id && id == guid)
                .Subscribe(_ =>
                {
                    _currentState = InteractionState.Hovered;
                    if (_highlightOnHover) ApplyHoverHighlight();
                }));

            composite.Add(_eventBus.On("ObjectHoverExit")
                .Where(payload => payload is string id && id == guid)
                .Subscribe(_ =>
                {
                    if (_currentState == InteractionState.Hovered)
                        _currentState = InteractionState.Idle;
                    if (_isHoverHighlighted) RemoveHoverHighlight();
                }));

            composite.Add(_eventBus.On("ObjectGrabbed")
                .Where(payload => payload is string id && id == guid)
                .Subscribe(_ => _currentState = InteractionState.Grabbed));

            composite.Add(_eventBus.On("ObjectReleased")
                .Where(payload => payload is string id && id == guid)
                .Subscribe(_ => _currentState = InteractionState.Idle));

            composite.Add(_eventBus.On("ObjectSelected")
                .Where(payload => payload is string id && id == guid)
                .Subscribe(_ => _currentState = InteractionState.Selected));

            composite.Add(_eventBus.On("ObjectDeselected")
                .Where(payload => payload is string id && id == guid)
                .Subscribe(_ => _currentState = InteractionState.Idle));

            _subscriptions = composite;
        }

        private void OnDisable()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
            _currentState = InteractionState.Idle;
            if (_isHoverHighlighted) RemoveHoverHighlight();
        }

        public void SetGuidanceHighlight(bool active, Color color, float intensity)
        {
            _isGuidanceHighlighted = active;
            _guidanceColor = color;
            _guidanceIntensity = intensity;

            if (active)
            {
                ApplyEmissionHighlight(color, intensity);
            }
            else if (_isHoverHighlighted)
            {
                ApplyEmissionHighlight(_hoverHighlightColor, _hoverHighlightIntensity);
            }
            else
            {
                RestoreOriginalMaterials();
            }
        }

        private void ApplyHoverHighlight()
        {
            if (_isGuidanceHighlighted) return;
            _isHoverHighlighted = true;
            ApplyEmissionHighlight(_hoverHighlightColor, _hoverHighlightIntensity);
        }

        private void RemoveHoverHighlight()
        {
            _isHoverHighlighted = false;
            if (_isGuidanceHighlighted)
            {
                ApplyEmissionHighlight(_guidanceColor, _guidanceIntensity);
                return;
            }
            RestoreOriginalMaterials();
        }

        private void ApplyEmissionHighlight(Color color, float intensity)
        {
            if (_originalStates == null)
            {
                var renderers = GetComponentsInChildren<Renderer>();
                _originalStates = new List<(Renderer, MaterialPropertyBlock)>(renderers.Length);
                foreach (var renderer in renderers)
                {
                    var block = new MaterialPropertyBlock();
                    renderer.GetPropertyBlock(block);
                    _originalStates.Add((renderer, block));
                }
            }

            foreach (var (renderer, _) in _originalStates)
            {
                if (renderer == null) continue;
                var block = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(block);
                block.SetColor("_EmissionColor", color * intensity);
                renderer.SetPropertyBlock(block);

                foreach (var material in renderer.materials)
                {
                    material.EnableKeyword("_EMISSION");
                }
            }
        }

        private void RestoreOriginalMaterials()
        {
            if (_originalStates == null) return;

            foreach (var (renderer, originalBlock) in _originalStates)
            {
                if (renderer == null) continue;
                renderer.SetPropertyBlock(originalBlock);
            }

            _originalStates = null;
        }
    }
}
