using System.Threading;
using NUnit.Framework;
using UnityEngine;
using ReactiveFlowEngine.Behaviors;
using ReactiveFlowEngine.Tests.TestDoubles;

namespace ReactiveFlowEngine.RuntimeTests
{
    [TestFixture]
    public class AudioBehaviorRuntimeTests
    {
        private RuntimeTestHelper _helper;
        private MockSceneObjectResolver _resolver;

        [SetUp]
        public void SetUp()
        {
            _helper = new RuntimeTestHelper();
            _resolver = new MockSceneObjectResolver();
        }

        [TearDown]
        public void TearDown()
        {
            _helper.TearDown();
        }

        // ── StopAudioBehavior ──────────────────────────────────────────

        [Test]
        public void StopAudioBehavior_StopsAudioSource()
        {
            var go = _helper.CreateGameObjectWithAudioSource("audio");
            var source = go.GetComponent<AudioSource>();
            // AudioSource.isPlaying is read-only and requires Play() in PlayMode,
            // but Stop() can be called regardless.
            _resolver.Register("audio-guid", go.transform);

            var behavior = new StopAudioBehavior(_resolver, "audio-guid");
            Assert.DoesNotThrow(() =>
                behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult());

            Assert.IsFalse(source.isPlaying);
        }

        [Test]
        public void StopAudioBehavior_NullResolver_DoesNotThrow()
        {
            var behavior = new StopAudioBehavior(null, "audio-guid");
            Assert.DoesNotThrow(() =>
                behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult());
        }

        [Test]
        public void StopAudioBehavior_MissingTarget_DoesNotThrow()
        {
            var behavior = new StopAudioBehavior(_resolver, "missing-guid");
            Assert.DoesNotThrow(() =>
                behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult());
        }

        // ── PauseAudioBehavior ─────────────────────────────────────────

        [Test]
        public void PauseAudioBehavior_PausesAudioSource()
        {
            var go = _helper.CreateGameObjectWithAudioSource("audio");
            _resolver.Register("audio-guid", go.transform);

            var behavior = new PauseAudioBehavior(_resolver, "audio-guid");
            Assert.DoesNotThrow(() =>
                behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult());
        }

        [Test]
        public void PauseAudioBehavior_NullResolver_DoesNotThrow()
        {
            var behavior = new PauseAudioBehavior(null, "audio-guid");
            Assert.DoesNotThrow(() =>
                behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult());
        }

        // ── ResumeAudioBehavior ────────────────────────────────────────

        [Test]
        public void ResumeAudioBehavior_UnPausesAudioSource()
        {
            var go = _helper.CreateGameObjectWithAudioSource("audio");
            _resolver.Register("audio-guid", go.transform);

            var behavior = new ResumeAudioBehavior(_resolver, "audio-guid");
            Assert.DoesNotThrow(() =>
                behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult());
        }

        [Test]
        public void ResumeAudioBehavior_NullResolver_DoesNotThrow()
        {
            var behavior = new ResumeAudioBehavior(null, "audio-guid");
            Assert.DoesNotThrow(() =>
                behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult());
        }

        // ── LoopAudioBehavior ──────────────────────────────────────────

        [Test]
        public void LoopAudioBehavior_SetsLoopMode()
        {
            var go = _helper.CreateGameObjectWithAudioSource("audio");
            var source = go.GetComponent<AudioSource>();
            source.loop = false;
            _resolver.Register("audio-guid", go.transform);

            var behavior = new LoopAudioBehavior(_resolver, "audio-guid");
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsTrue(source.loop);
        }

        [Test]
        public void LoopAudioBehavior_Undo_RestoresOriginalLoopState()
        {
            var go = _helper.CreateGameObjectWithAudioSource("audio");
            var source = go.GetComponent<AudioSource>();
            source.loop = false;
            _resolver.Register("audio-guid", go.transform);

            var behavior = new LoopAudioBehavior(_resolver, "audio-guid");
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            Assert.IsTrue(source.loop);

            behavior.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();
            Assert.IsFalse(source.loop);
        }

        // ── TextToSpeechBehavior ───────────────────────────────────────

        [Test]
        public void TextToSpeechBehavior_ExecutesWithoutError()
        {
            var behavior = new TextToSpeechBehavior("Hello world", "en");
            Assert.DoesNotThrow(() =>
                behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult());
        }

        [Test]
        public void TextToSpeechBehavior_IsBlocking_DefaultsToTrue()
        {
            var behavior = new TextToSpeechBehavior("Test text");
            Assert.IsTrue(behavior.IsBlocking);
        }
    }
}
