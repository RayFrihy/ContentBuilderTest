using System.Threading;
using NUnit.Framework;
using ReactiveFlowEngine.Engine;
using ReactiveFlowEngine.Model;
using ReactiveFlowEngine.Navigation;
using ReactiveFlowEngine.Tests.TestDoubles;

namespace ReactiveFlowEngine.Tests
{
    [TestFixture]
    public class ChapterRunnerTests
    {
        private MockStepRunner _stepRunner;
        private MockStateStore _stateStore;
        private HistoryStack _historyService;
        private ChapterRunner _runner;

        [SetUp]
        public void SetUp()
        {
            _stepRunner = new MockStepRunner();
            _stateStore = new MockStateStore();
            _historyService = new HistoryStack();
            _runner = new ChapterRunner(_stepRunner, _stateStore, _historyService);
        }

        [Test]
        public void RunAsync_NullChapter_ReturnsWithoutCrash()
        {
            _runner.RunAsync(null, CancellationToken.None).GetAwaiter().GetResult();
            Assert.AreEqual(0, _stepRunner.RunSteps.Count);
        }

        [Test]
        public void RunAsync_RunsStepsSequentially()
        {
            var step1 = new StepModel { Id = "s1", Name = "Step1" };
            var step2 = new StepModel { Id = "s2", Name = "Step2" };
            var t1 = new TransitionModel { TargetStepModel = step2 };
            var t2 = new TransitionModel { TargetStepModel = null };

            var chapter = new ChapterModel { Id = "ch1", Name = "Ch1" };
            chapter.StepModels.Add(step1);
            chapter.StepModels.Add(step2);
            chapter.FirstStepModel = step1;

            _stepRunner.EnqueueTransition(t1);
            _stepRunner.EnqueueTransition(t2);

            _runner.RunAsync(chapter, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(2, _stepRunner.RunSteps.Count);
            Assert.AreEqual("s1", _stepRunner.RunSteps[0].Id);
            Assert.AreEqual("s2", _stepRunner.RunSteps[1].Id);
        }

        [Test]
        public void RunAsync_PushesHistoryAfterEachStep()
        {
            var step1 = new StepModel { Id = "s1", Name = "Step1" };
            var t1 = new TransitionModel { TargetStepModel = null };

            var chapter = new ChapterModel { Id = "ch1", Name = "Ch1" };
            chapter.StepModels.Add(step1);
            chapter.FirstStepModel = step1;

            _stepRunner.EnqueueTransition(t1);

            _runner.RunAsync(chapter, CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsTrue(_historyService.CanGoBack);
            var entry = _historyService.Pop();
            Assert.AreEqual("s1", entry.StepId);
            Assert.AreEqual("ch1", entry.ChapterId);
        }

        [Test]
        public void RunAsync_NullFirstStep_ReturnsGracefully()
        {
            var chapter = new ChapterModel { Id = "ch1", Name = "Ch1" };
            // FirstStepModel is null

            _runner.RunAsync(chapter, CancellationToken.None).GetAwaiter().GetResult();
            Assert.AreEqual(0, _stepRunner.RunSteps.Count);
        }
    }
}
