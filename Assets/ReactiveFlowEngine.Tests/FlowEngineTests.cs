using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Engine;
using ReactiveFlowEngine.Model;
using ReactiveFlowEngine.Navigation;
using ReactiveFlowEngine.Tests.TestDoubles;

namespace ReactiveFlowEngine.Tests
{
    [TestFixture]
    public class FlowEngineTests
    {
        private MockStepRunner _stepRunner;
        private MockStateStore _stateStore;
        private HistoryStack _historyService;
        private ChapterRunner _chapterRunner;
        private FlowEngine _engine;

        [SetUp]
        public void SetUp()
        {
            _stepRunner = new MockStepRunner();
            _stateStore = new MockStateStore();
            _historyService = new HistoryStack();
            _chapterRunner = new ChapterRunner(_stepRunner, _stateStore, _historyService);
            _engine = new FlowEngine(_stepRunner, _stateStore, _chapterRunner, _historyService);
        }

        [TearDown]
        public void TearDown()
        {
            _engine.Dispose();
        }

        [Test]
        public void InitialState_IsIdle()
        {
            Assert.AreEqual(EngineState.Idle, _engine.State.CurrentValue);
            Assert.IsNull(_engine.CurrentStep.CurrentValue);
            Assert.IsNull(_engine.CurrentChapter.CurrentValue);
        }

        [Test]
        public void StartProcess_WithNullProcess_ReturnsWithoutCrash()
        {
            _engine.StartProcessAsync(null, CancellationToken.None).GetAwaiter().GetResult();
            Assert.AreEqual(EngineState.Idle, _engine.State.CurrentValue);
        }

        [Test]
        public void StartProcess_WithEmptyChapters_ReturnsWithoutCrash()
        {
            var process = new ProcessModel { Id = "p1", Name = "Empty" };
            _engine.StartProcessAsync(process, CancellationToken.None).GetAwaiter().GetResult();
            Assert.AreEqual(EngineState.Idle, _engine.State.CurrentValue);
        }

        [Test]
        public void StartProcess_RunsThroughChapterSteps()
        {
            var step1 = new StepModel { Id = "s1", Name = "Step1" };
            var step2 = new StepModel { Id = "s2", Name = "Step2" };
            var t1 = new TransitionModel { TargetStepModel = step2 };
            step1.TransitionModels.Add(t1);
            var t2 = new TransitionModel { TargetStepModel = null }; // end
            step2.TransitionModels.Add(t2);

            var chapter = new ChapterModel { Id = "ch1", Name = "Chapter1" };
            chapter.StepModels.Add(step1);
            chapter.StepModels.Add(step2);
            chapter.FirstStepModel = step1;

            var process = new ProcessModel { Id = "p1", Name = "Test" };
            process.ChapterModels.Add(chapter);
            process.FirstChapterModel = chapter;

            // Set up step runner to return transitions
            _stepRunner.EnqueueTransition(t1);
            _stepRunner.EnqueueTransition(t2);

            _engine.StartProcessAsync(process, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(2, _stepRunner.RunSteps.Count);
            Assert.AreEqual("s1", _stepRunner.RunSteps[0].Id);
            Assert.AreEqual("s2", _stepRunner.RunSteps[1].Id);
            Assert.AreEqual(EngineState.Completed, _engine.State.CurrentValue);
        }

        [Test]
        public void StartProcess_CapturesSnapshotsAfterEachStep()
        {
            var step1 = new StepModel { Id = "s1", Name = "Step1" };
            var t1 = new TransitionModel { TargetStepModel = null };
            step1.TransitionModels.Add(t1);

            var chapter = new ChapterModel { Id = "ch1", Name = "Ch1" };
            chapter.StepModels.Add(step1);
            chapter.FirstStepModel = step1;

            var process = new ProcessModel { Id = "p1", Name = "Test" };
            process.ChapterModels.Add(chapter);
            process.FirstChapterModel = chapter;

            _stepRunner.EnqueueTransition(t1);

            _engine.StartProcessAsync(process, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, _stateStore.CapturedStepIds.Count);
            Assert.AreEqual("s1", _stateStore.CapturedStepIds[0]);
        }

        [Test]
        public void StopAsync_SetsStateToIdle()
        {
            _engine.StopAsync().GetAwaiter().GetResult();
            Assert.AreEqual(EngineState.Idle, _engine.State.CurrentValue);
        }

        [Test]
        public void SetCurrentStep_UpdatesProperty()
        {
            var step = new StepModel { Id = "s1", Name = "Step1" };
            ((IEngineController)_engine).SetCurrentStep(step);
            Assert.AreEqual("s1", _engine.CurrentStep.CurrentValue.Id);
        }

        [Test]
        public void SetCurrentChapter_UpdatesProperty()
        {
            var chapter = new ChapterModel { Id = "ch1", Name = "Chapter1" };
            ((IEngineController)_engine).SetCurrentChapter(chapter);
            Assert.AreEqual("ch1", _engine.CurrentChapter.CurrentValue.Id);
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            _engine.Dispose();
            _engine.Dispose(); // double dispose should not throw
        }
    }
}
