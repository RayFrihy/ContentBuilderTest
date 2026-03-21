using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Behaviors;
using ReactiveFlowEngine.Model;
using ReactiveFlowEngine.Tests.TestDoubles;

namespace ReactiveFlowEngine.Tests.Behaviors
{
    // ── ForceTransitionBehavior ─────────────────────────────────────

    [TestFixture]
    public class ForceTransitionBehaviorTests
    {
        private MockStepRunner _stepRunner;

        [SetUp]
        public void SetUp()
        {
            _stepRunner = new MockStepRunner();
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void ExecuteAsync_CallsCancelCurrentStep()
        {
            var behavior = new ForceTransitionBehavior(_stepRunner);
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, _stepRunner.CancelCount);
        }

        [Test]
        public void ExecuteAsync_CalledTwice_CancelsCurrentStepTwice()
        {
            var behavior = new ForceTransitionBehavior(_stepRunner);
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(2, _stepRunner.CancelCount);
        }

        [Test]
        public void IsBlocking_DefaultsTrue()
        {
            var behavior = new ForceTransitionBehavior(_stepRunner);
            Assert.IsTrue(behavior.IsBlocking);
        }

        [Test]
        public void IsBlocking_RespectsConstructorArgument()
        {
            var behavior = new ForceTransitionBehavior(_stepRunner, isBlocking: false);
            Assert.IsFalse(behavior.IsBlocking);
        }

        [Test]
        public void Stages_DefaultsToActivation()
        {
            var behavior = new ForceTransitionBehavior(_stepRunner);
            Assert.AreEqual(ExecutionStages.Activation, behavior.Stages);
        }

        [Test]
        public void Stages_RespectsConstructorArgument()
        {
            var behavior = new ForceTransitionBehavior(_stepRunner, stages: ExecutionStages.Deactivation);
            Assert.AreEqual(ExecutionStages.Deactivation, behavior.Stages);
        }

        [Test]
        public void ImplementsIBehavior()
        {
            var behavior = new ForceTransitionBehavior(_stepRunner);
            Assert.IsInstanceOf<IBehavior>(behavior);
        }
    }

    // ── CancelTransitionBehavior ────────────────────────────────────

    [TestFixture]
    public class CancelTransitionBehaviorTests
    {
        [Test]
        public void ExecuteAsync_CompletesWithoutError()
        {
            var behavior = new CancelTransitionBehavior();
            Assert.DoesNotThrow(() =>
                behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult());
        }

        [Test]
        public void IsBlocking_DefaultsFalse()
        {
            var behavior = new CancelTransitionBehavior();
            Assert.IsFalse(behavior.IsBlocking);
        }

        [Test]
        public void IsBlocking_RespectsConstructorArgument()
        {
            var behavior = new CancelTransitionBehavior(isBlocking: true);
            Assert.IsTrue(behavior.IsBlocking);
        }

        [Test]
        public void Stages_DefaultsToActivation()
        {
            var behavior = new CancelTransitionBehavior();
            Assert.AreEqual(ExecutionStages.Activation, behavior.Stages);
        }

        [Test]
        public void Stages_RespectsConstructorArgument()
        {
            var behavior = new CancelTransitionBehavior(stages: ExecutionStages.Deactivation);
            Assert.AreEqual(ExecutionStages.Deactivation, behavior.Stages);
        }

        [Test]
        public void ImplementsIBehavior()
        {
            var behavior = new CancelTransitionBehavior();
            Assert.IsInstanceOf<IBehavior>(behavior);
        }
    }

    // ── RestartStepBehavior ─────────────────────────────────────────

    [TestFixture]
    public class RestartStepBehaviorTests
    {
        private MockNavigationService _navigationService;

        [SetUp]
        public void SetUp()
        {
            _navigationService = new MockNavigationService();
        }

        [TearDown]
        public void TearDown()
        {
            _navigationService.Dispose();
        }

        [Test]
        public void ExecuteAsync_CallsRestartStepAsync()
        {
            var behavior = new RestartStepBehavior(_navigationService);
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, _navigationService.RestartStepCount);
        }

        [Test]
        public void ExecuteAsync_CalledTwice_RestartsStepTwice()
        {
            var behavior = new RestartStepBehavior(_navigationService);
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(2, _navigationService.RestartStepCount);
        }

        [Test]
        public void IsBlocking_DefaultsTrue()
        {
            var behavior = new RestartStepBehavior(_navigationService);
            Assert.IsTrue(behavior.IsBlocking);
        }

        [Test]
        public void IsBlocking_RespectsConstructorArgument()
        {
            var behavior = new RestartStepBehavior(_navigationService, isBlocking: false);
            Assert.IsFalse(behavior.IsBlocking);
        }

        [Test]
        public void Stages_DefaultsToActivation()
        {
            var behavior = new RestartStepBehavior(_navigationService);
            Assert.AreEqual(ExecutionStages.Activation, behavior.Stages);
        }

        [Test]
        public void Stages_RespectsConstructorArgument()
        {
            var behavior = new RestartStepBehavior(_navigationService, stages: ExecutionStages.Deactivation);
            Assert.AreEqual(ExecutionStages.Deactivation, behavior.Stages);
        }

        [Test]
        public void ImplementsIBehavior()
        {
            var behavior = new RestartStepBehavior(_navigationService);
            Assert.IsInstanceOf<IBehavior>(behavior);
        }
    }

    // ── SkipStepBehavior ────────────────────────────────────────────

    [TestFixture]
    public class SkipStepBehaviorTests
    {
        private MockNavigationService _navigationService;

        [SetUp]
        public void SetUp()
        {
            _navigationService = new MockNavigationService();
        }

        [TearDown]
        public void TearDown()
        {
            _navigationService.Dispose();
        }

        [Test]
        public void ExecuteAsync_CallsNextStepAsync()
        {
            var behavior = new SkipStepBehavior(_navigationService);
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, _navigationService.NextStepCount);
        }

        [Test]
        public void ExecuteAsync_CalledTwice_SkipsStepTwice()
        {
            var behavior = new SkipStepBehavior(_navigationService);
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(2, _navigationService.NextStepCount);
        }

        [Test]
        public void IsBlocking_DefaultsTrue()
        {
            var behavior = new SkipStepBehavior(_navigationService);
            Assert.IsTrue(behavior.IsBlocking);
        }

        [Test]
        public void IsBlocking_RespectsConstructorArgument()
        {
            var behavior = new SkipStepBehavior(_navigationService, isBlocking: false);
            Assert.IsFalse(behavior.IsBlocking);
        }

        [Test]
        public void Stages_DefaultsToActivation()
        {
            var behavior = new SkipStepBehavior(_navigationService);
            Assert.AreEqual(ExecutionStages.Activation, behavior.Stages);
        }

        [Test]
        public void Stages_RespectsConstructorArgument()
        {
            var behavior = new SkipStepBehavior(_navigationService, stages: ExecutionStages.Deactivation);
            Assert.AreEqual(ExecutionStages.Deactivation, behavior.Stages);
        }

        [Test]
        public void ImplementsIBehavior()
        {
            var behavior = new SkipStepBehavior(_navigationService);
            Assert.IsInstanceOf<IBehavior>(behavior);
        }
    }

    // ── EndProcessBehavior ──────────────────────────────────────────

    [TestFixture]
    public class EndProcessBehaviorTests
    {
        private MockFlowEngine _flowEngine;

        [SetUp]
        public void SetUp()
        {
            _flowEngine = new MockFlowEngine();
        }

        [TearDown]
        public void TearDown()
        {
            _flowEngine.Dispose();
        }

        [Test]
        public void ExecuteAsync_CallsStopAsync()
        {
            var behavior = new EndProcessBehavior(_flowEngine);
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, _flowEngine.StopCount);
        }

        [Test]
        public void ExecuteAsync_CalledTwice_StopsTwice()
        {
            var behavior = new EndProcessBehavior(_flowEngine);
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(2, _flowEngine.StopCount);
        }

        [Test]
        public void IsBlocking_DefaultsTrue()
        {
            var behavior = new EndProcessBehavior(_flowEngine);
            Assert.IsTrue(behavior.IsBlocking);
        }

        [Test]
        public void IsBlocking_RespectsConstructorArgument()
        {
            var behavior = new EndProcessBehavior(_flowEngine, isBlocking: false);
            Assert.IsFalse(behavior.IsBlocking);
        }

        [Test]
        public void Stages_DefaultsToActivation()
        {
            var behavior = new EndProcessBehavior(_flowEngine);
            Assert.AreEqual(ExecutionStages.Activation, behavior.Stages);
        }

        [Test]
        public void Stages_RespectsConstructorArgument()
        {
            var behavior = new EndProcessBehavior(_flowEngine, stages: ExecutionStages.Deactivation);
            Assert.AreEqual(ExecutionStages.Deactivation, behavior.Stages);
        }

        [Test]
        public void ImplementsIBehavior()
        {
            var behavior = new EndProcessBehavior(_flowEngine);
            Assert.IsInstanceOf<IBehavior>(behavior);
        }
    }

    // ── ExecuteChapterBehavior ──────────────────────────────────────

    [TestFixture]
    public class ExecuteChapterBehaviorTests
    {
        [Test]
        public void ExecuteAsync_NullChapter_LogsWarningAndReturns()
        {
            var behavior = new ExecuteChapterBehavior(null, (ch, ct) => UniTask.CompletedTask);
            Assert.DoesNotThrow(() =>
                behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult());
        }

        [Test]
        public void ExecuteAsync_NullRunner_LogsWarningAndReturns()
        {
            var chapter = new ChapterModel { Id = "ch-1", Name = "Test Chapter" };
            var behavior = new ExecuteChapterBehavior(chapter, null);
            Assert.DoesNotThrow(() =>
                behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult());
        }

        [Test]
        public void ExecuteAsync_ExecutesDelegate()
        {
            var chapter = new ChapterModel { Id = "ch-1", Name = "Test Chapter" };
            IChapter receivedChapter = null;
            Func<IChapter, CancellationToken, UniTask> runner = (ch, ct) =>
            {
                receivedChapter = ch;
                return UniTask.CompletedTask;
            };

            var behavior = new ExecuteChapterBehavior(chapter, runner);
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreSame(chapter, receivedChapter);
        }

        [Test]
        public void UndoAsync_CompletesWithoutError()
        {
            var chapter = new ChapterModel { Id = "ch-1", Name = "Test Chapter" };
            var behavior = new ExecuteChapterBehavior(chapter, (ch, ct) => UniTask.CompletedTask);
            Assert.DoesNotThrow(() =>
                behavior.UndoAsync(CancellationToken.None).GetAwaiter().GetResult());
        }

        [Test]
        public void IsBlocking_IsTrue()
        {
            var chapter = new ChapterModel { Id = "ch-1", Name = "Test Chapter" };
            var behavior = new ExecuteChapterBehavior(chapter, (ch, ct) => UniTask.CompletedTask);
            Assert.IsTrue(behavior.IsBlocking);
        }

        [Test]
        public void Stages_IsActivation()
        {
            var chapter = new ChapterModel { Id = "ch-1", Name = "Test Chapter" };
            var behavior = new ExecuteChapterBehavior(chapter, (ch, ct) => UniTask.CompletedTask);
            Assert.AreEqual(ExecutionStages.Activation, behavior.Stages);
        }

        [Test]
        public void SubChapterGuid_RespectsConstructorArgument()
        {
            var chapter = new ChapterModel { Id = "ch-1", Name = "Test Chapter" };
            var behavior = new ExecuteChapterBehavior(chapter, (ch, ct) => UniTask.CompletedTask, "sub-guid-123");
            Assert.AreEqual("sub-guid-123", behavior.SubChapterGuid);
        }

        [Test]
        public void SubChapterGuid_DefaultsNull()
        {
            var chapter = new ChapterModel { Id = "ch-1", Name = "Test Chapter" };
            var behavior = new ExecuteChapterBehavior(chapter, (ch, ct) => UniTask.CompletedTask);
            Assert.IsNull(behavior.SubChapterGuid);
        }

        [Test]
        public void GetSubChapter_ReturnsChapterModel()
        {
            var chapter = new ChapterModel { Id = "ch-1", Name = "Test Chapter" };
            var behavior = new ExecuteChapterBehavior(chapter, (ch, ct) => UniTask.CompletedTask);
            Assert.AreSame(chapter, behavior.GetSubChapter());
        }

        [Test]
        public void ImplementsIExecuteChapterBehavior()
        {
            var chapter = new ChapterModel { Id = "ch-1", Name = "Test Chapter" };
            var behavior = new ExecuteChapterBehavior(chapter, (ch, ct) => UniTask.CompletedTask);
            Assert.IsInstanceOf<IExecuteChapterBehavior>(behavior);
        }
    }
}
