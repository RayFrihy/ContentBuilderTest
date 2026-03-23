using System;
using System.Collections.Generic;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Navigation;

namespace ReactiveFlowEngine.Runtime.UI
{
    [AddComponentMenu("Reactive Flow Engine/Step List UI")]
    public class RfeStepListUI : MonoBehaviour
    {
        [Inject] private IFlowEngine _engine;
        [Inject] private IHistoryService _historyService;

        [Header("UI References")]
        [SerializeField] private Transform _stepListContainer;
        [SerializeField] private GameObject _stepItemPrefab;

        [Header("Colors")]
        [SerializeField] private Color _completedColor = new Color(0.3f, 0.8f, 0.3f, 1f);
        [SerializeField] private Color _currentColor = new Color(0.2f, 0.6f, 1f, 1f);
        [SerializeField] private Color _upcomingColor = new Color(0.6f, 0.6f, 0.6f, 1f);

        private IDisposable _subscriptions;
        private readonly List<GameObject> _spawnedItems = new List<GameObject>();

        private void OnEnable()
        {
            if (_engine == null) return;

            var composite = new CompositeDisposable();

            composite.Add(_engine.CurrentChapter.Subscribe(_ => RebuildList()));
            composite.Add(_engine.CurrentStep.Subscribe(_ => UpdateHighlights()));

            _subscriptions = composite;
        }

        private void OnDisable()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
            ClearList();
        }

        private void RebuildList()
        {
            ClearList();

            var chapter = _engine.CurrentChapter.CurrentValue;
            if (chapter == null || _stepListContainer == null || _stepItemPrefab == null) return;

            foreach (var step in chapter.Steps)
            {
                var item = Instantiate(_stepItemPrefab, _stepListContainer);
                item.SetActive(true);

                var text = item.GetComponentInChildren<Text>();
                if (text != null)
                    text.text = step.Name ?? step.Id;

                _spawnedItems.Add(item);
            }

            UpdateHighlights();
        }

        private void UpdateHighlights()
        {
            var chapter = _engine.CurrentChapter.CurrentValue;
            var currentStep = _engine.CurrentStep.CurrentValue;
            if (chapter == null) return;

            var completedIds = new HashSet<string>();
            var history = _historyService?.GetAll();
            if (history != null)
            {
                foreach (var entry in history)
                    completedIds.Add(entry.StepId);
            }

            for (int i = 0; i < chapter.Steps.Count && i < _spawnedItems.Count; i++)
            {
                var step = chapter.Steps[i];
                var text = _spawnedItems[i].GetComponentInChildren<Text>();
                if (text == null) continue;

                if (currentStep != null && step.Id == currentStep.Id)
                    text.color = _currentColor;
                else if (completedIds.Contains(step.Id))
                    text.color = _completedColor;
                else
                    text.color = _upcomingColor;
            }
        }

        private void ClearList()
        {
            foreach (var item in _spawnedItems)
            {
                if (item != null) Destroy(item);
            }
            _spawnedItems.Clear();
        }
    }
}
