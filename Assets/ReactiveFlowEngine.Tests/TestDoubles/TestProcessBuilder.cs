using System.Collections.Generic;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;

namespace ReactiveFlowEngine.Tests.TestDoubles
{
    public class TestProcessBuilder
    {
        private readonly ProcessModel _process;
        private ChapterModel _currentChapter;
        private StepModel _currentStep;

        public TestProcessBuilder(string name = "TestProcess")
        {
            _process = new ProcessModel
            {
                Id = System.Guid.NewGuid().ToString(),
                Name = name
            };
        }

        public TestProcessBuilder AddChapter(string name = null, string id = null)
        {
            _currentChapter = new ChapterModel
            {
                Id = id ?? System.Guid.NewGuid().ToString(),
                Name = name ?? $"Chapter {_process.ChapterModels.Count + 1}"
            };
            _process.ChapterModels.Add(_currentChapter);
            if (_process.FirstChapterModel == null)
                _process.FirstChapterModel = _currentChapter;
            return this;
        }

        public TestProcessBuilder AddStep(string name = null, string id = null, StepType type = StepType.Default)
        {
            if (_currentChapter == null)
                AddChapter();

            _currentStep = new StepModel
            {
                Id = id ?? System.Guid.NewGuid().ToString(),
                Name = name ?? $"Step {_currentChapter.StepModels.Count + 1}",
                Type = type
            };
            _currentChapter.StepModels.Add(_currentStep);
            if (_currentChapter.FirstStepModel == null)
                _currentChapter.FirstStepModel = _currentStep;
            return this;
        }

        public TestProcessBuilder AddBehavior(IBehavior behavior)
        {
            if (_currentStep == null)
                AddStep();

            _currentStep.BehaviorList.Add(behavior);
            return this;
        }

        public TestProcessBuilder AddTransition(ICondition condition = null, StepModel targetStep = null)
        {
            if (_currentStep == null)
                AddStep();

            var transition = new TransitionModel
            {
                TargetStepModel = targetStep
            };
            if (condition != null)
                transition.ConditionList.Add(condition);

            _currentStep.TransitionModels.Add(transition);
            return this;
        }

        public TestProcessBuilder AddTransitionToNext()
        {
            if (_currentStep == null || _currentChapter == null)
                return this;

            var currentIndex = _currentChapter.StepModels.IndexOf(_currentStep);
            StepModel nextStep = currentIndex + 1 < _currentChapter.StepModels.Count
                ? _currentChapter.StepModels[currentIndex + 1]
                : null;

            return AddTransition(targetStep: nextStep);
        }

        public TestProcessBuilder SetSubChapter(ChapterModel subChapter)
        {
            if (_currentStep != null)
                _currentStep.SubChapterModel = subChapter;
            return this;
        }

        public StepModel GetCurrentStep() => _currentStep;
        public ChapterModel GetCurrentChapter() => _currentChapter;

        public ProcessModel Build()
        {
            return _process;
        }
    }
}
