using System.Threading;
using NUnit.Framework;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Engine;
using ReactiveFlowEngine.Model;
using ReactiveFlowEngine.Navigation;
using ReactiveFlowEngine.Tests.TestDoubles;

namespace ReactiveFlowEngine.Tests
{
    [TestFixture]
    public class NavigationServiceTests
    {
        private FlowEngine _engine;
        private MockStepRunner _stepRunner;
        private MockStateStore _stateStore;
        private HistoryStack _historyService;
        private ChapterRunner _chapterRunner;
        private NavigationService _navService;

        [SetUp]
        public void SetUp()
        {
            _stepRunner = new MockStepRunner();
            _stateStore = new MockStateStore();
            _historyService = new HistoryStack();
            _chapterRunner = new ChapterRunner(_stepRunner, _stateStore, _historyService);
            _engine = new FlowEngine(_stepRunner, _stateStore, _chapterRunner, _historyService);
            _navService = new NavigationService(_engine, _engine, _stepRunner, _stateStore, _historyService);
        }

        [TearDown]
        public void TearDown()
        {
            _navService.Dispose();
            _engine.Dispose();
        }

        [Test]
        public void CanGoBack_ReturnsFalse_WhenNoHistory()
        {
            Assert.IsFalse(_navService.CanGoBack);
        }

        [Test]
        public void CanGoBack_ReturnsTrue_AfterOnStepCompleted()
        {
            var step = new StepModel { Id = "s1", Name = "Step1" };
            var chapter = new ChapterModel { Id = "ch1", Name = "Ch1" };
            _navService.OnStepCompleted(step, chapter);

            Assert.IsTrue(_navService.CanGoBack);
        }

        [Test]
        public void PreviousStepAsync_WhenNoHistory_DoesNotThrow()
        {
            // Set a current step on the engine
            ((IEngineController)_engine).SetCurrentStep(new StepModel { Id = "s1", Name = "Step1" });

            _navService.PreviousStepAsync(CancellationToken.None).GetAwaiter().GetResult();
            // Should not throw; just log a warning
        }

        [Test]
        public void NextStepAsync_CancelsCurrentStep()
        {
            ((IEngineController)_engine).SetCurrentStep(new StepModel { Id = "s1", Name = "Step1" });

            _navService.NextStepAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, _stepRunner.CancelCount);
        }

        [Test]
        public void NextStepAsync_EmitsNavigationEvent()
        {
            ((IEngineController)_engine).SetCurrentStep(new StepModel { Id = "s1", Name = "Step1" });

            NavigationEvent receivedEvent = null;
            _navService.OnNavigated.Subscribe(e => receivedEvent = e);

            _navService.NextStepAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsNotNull(receivedEvent);
            Assert.AreEqual(NavigationType.Forward, receivedEvent.Type);
            Assert.AreEqual("s1", receivedEvent.FromStepId);
        }

        [Test]
        public void SetCurrentProcess_ClearsHistory()
        {
            _historyService.Push(new HistoryEntry("ch1", "s1"));

            var process = new TestProcessBuilder()
                .AddChapter()
                .AddStep()
                .Build();

            _navService.SetCurrentProcess(process);

            Assert.IsFalse(_historyService.CanGoBack);
        }

        [Test]
        public void PreviousStepAsync_UsesSharedHistory()
        {
            // Setup process and steps
            var step1 = new StepModel { Id = "s1", Name = "Step1" };
            var step2 = new StepModel { Id = "s2", Name = "Step2" };
            var chapter = new ChapterModel { Id = "ch1", Name = "Ch1" };
            chapter.StepModels.Add(step1);
            chapter.StepModels.Add(step2);
            chapter.FirstStepModel = step1;

            var process = new ProcessModel { Id = "p1", Name = "Test" };
            process.ChapterModels.Add(chapter);
            process.FirstChapterModel = chapter;

            _navService.SetCurrentProcess(process);

            // Simulate engine at step2 with step1 in history
            ((IEngineController)_engine).SetCurrentStep(step2);
            ((IEngineController)_engine).SetCurrentChapter(chapter);
            _historyService.Push(new HistoryEntry("ch1", "s1"));
            _stateStore.CaptureSnapshot(step1);

            _navService.PreviousStepAsync(CancellationToken.None).GetAwaiter().GetResult();

            // After going back, current step should be step1
            Assert.AreEqual("s1", _engine.CurrentStep.CurrentValue.Id);
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            _navService.Dispose();
            _navService.Dispose(); // double dispose
        }

        [Test]
        public void AfterDispose_ThrowsObjectDisposed()
        {
            _navService.Dispose();
            Assert.ThrowsAsync<System.ObjectDisposedException>(async () =>
                await _navService.NextStepAsync(CancellationToken.None));
        }
    }
}
