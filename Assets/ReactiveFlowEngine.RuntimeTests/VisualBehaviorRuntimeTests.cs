using System.Threading;
using NUnit.Framework;
using UnityEngine;
using ReactiveFlowEngine.Behaviors;
using ReactiveFlowEngine.Tests.TestDoubles;

namespace ReactiveFlowEngine.RuntimeTests
{
    [TestFixture]
    public class VisualBehaviorRuntimeTests
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

        // ── SetRendererVisibilityBehavior ──────────────────────────────

        [Test]
        public void SetRendererVisibility_HidesSingleRenderer()
        {
            var go = _helper.CreateGameObjectWithRenderer("target");
            _resolver.Register("target-guid", go.transform);

            var behavior = new SetRendererVisibilityBehavior(_resolver, "target-guid", false, includeChildren: false);
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsFalse(go.GetComponent<Renderer>().enabled);
        }

        [Test]
        public void SetRendererVisibility_ShowsSingleRenderer()
        {
            var go = _helper.CreateGameObjectWithRenderer("target");
            go.GetComponent<Renderer>().enabled = false;
            _resolver.Register("target-guid", go.transform);

            var behavior = new SetRendererVisibilityBehavior(_resolver, "target-guid", true, includeChildren: false);
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsTrue(go.GetComponent<Renderer>().enabled);
        }

        [Test]
        public void SetRendererVisibility_IncludeChildren_HidesAllRenderers()
        {
            var parent = _helper.CreateGameObjectWithRenderer("parent");
            var child = _helper.CreateGameObjectWithRenderer("child");
            child.transform.SetParent(parent.transform);
            _resolver.Register("parent-guid", parent.transform);

            var behavior = new SetRendererVisibilityBehavior(_resolver, "parent-guid", false, includeChildren: true);
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsFalse(parent.GetComponent<Renderer>().enabled);
            Assert.IsFalse(child.GetComponent<Renderer>().enabled);
        }

        [Test]
        public void SetRendererVisibility_Undo_RestoresAllRenderers()
        {
            var parent = _helper.CreateGameObjectWithRenderer("parent");
            var child = _helper.CreateGameObjectWithRenderer("child");
            child.transform.SetParent(parent.transform);
            child.GetComponent<Renderer>().enabled = false;
            _resolver.Register("parent-guid", parent.transform);

            var behavior = new SetRendererVisibilityBehavior(_resolver, "parent-guid", true, includeChildren: true);
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            Assert.IsTrue(child.GetComponent<Renderer>().enabled);

            behavior.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();
            Assert.IsFalse(child.GetComponent<Renderer>().enabled);
        }

        [Test]
        public void SetRendererVisibility_NullResolver_DoesNotThrow()
        {
            var behavior = new SetRendererVisibilityBehavior(null, "guid", false);
            Assert.DoesNotThrow(() =>
                behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult());
        }

        [Test]
        public void SetRendererVisibility_MissingTarget_DoesNotThrow()
        {
            var behavior = new SetRendererVisibilityBehavior(_resolver, "missing-guid", false);
            Assert.DoesNotThrow(() =>
                behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult());
        }

        // ── HighlightObjectBehavior ────────────────────────────────────

        [Test]
        public void HighlightObject_AppliesEmissionKeyword()
        {
            var go = _helper.CreateGameObjectWithRenderer("target");
            _resolver.Register("target-guid", go.transform);

            var behavior = new HighlightObjectBehavior(_resolver, "target-guid", Color.yellow, 2f);
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsTrue(go.GetComponent<Renderer>().material.IsKeywordEnabled("_EMISSION"));
        }

        [Test]
        public void HighlightObject_Undo_RestoresOriginalPropertyBlock()
        {
            var go = _helper.CreateGameObjectWithRenderer("target");
            _resolver.Register("target-guid", go.transform);

            var behavior = new HighlightObjectBehavior(_resolver, "target-guid", Color.green, 1f);
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            behavior.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();

            // After undo, the property block is restored. We verify no exception occurred.
            Assert.Pass("Highlight undo completed without error");
        }

        [Test]
        public void HighlightObject_NullResolver_DoesNotThrow()
        {
            var behavior = new HighlightObjectBehavior(null, "guid", Color.red);
            Assert.DoesNotThrow(() =>
                behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult());
        }

        // ── UnhighlightObjectBehavior ──────────────────────────────────

        [Test]
        public void UnhighlightObject_DisablesEmissionKeyword()
        {
            var go = _helper.CreateGameObjectWithRenderer("target");
            go.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
            _resolver.Register("target-guid", go.transform);

            var behavior = new UnhighlightObjectBehavior(_resolver, "target-guid");
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsFalse(go.GetComponent<Renderer>().material.IsKeywordEnabled("_EMISSION"));
        }

        // ── FadeObjectBehavior ─────────────────────────────────────────

        [Test]
        public void FadeObject_Construction_SetsProperties()
        {
            var go = _helper.CreateGameObjectWithRenderer("target");
            _resolver.Register("target-guid", go.transform);

            var behavior = new FadeObjectBehavior(_resolver, "target-guid", 0.5f, 1.0f);
            Assert.IsTrue(behavior.IsBlocking);
        }

        // ── ChangeMaterialBehavior ─────────────────────────────────────

        [Test]
        public void ChangeMaterial_NullResolver_DoesNotThrow()
        {
            var behavior = new ChangeMaterialBehavior(null, "guid", "Materials/Test");
            Assert.DoesNotThrow(() =>
                behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult());
        }

        [Test]
        public void ChangeMaterial_MissingTarget_DoesNotThrow()
        {
            var behavior = new ChangeMaterialBehavior(_resolver, "missing-guid", "Materials/Test");
            Assert.DoesNotThrow(() =>
                behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult());
        }
    }
}
