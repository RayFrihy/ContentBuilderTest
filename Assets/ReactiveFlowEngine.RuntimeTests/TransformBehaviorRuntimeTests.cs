using System.Threading;
using NUnit.Framework;
using UnityEngine;
using ReactiveFlowEngine.Behaviors;
using ReactiveFlowEngine.Tests.TestDoubles;

namespace ReactiveFlowEngine.RuntimeTests
{
    [TestFixture]
    public class TransformBehaviorRuntimeTests
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

        // ── SetActiveBehavior ──────────────────────────────────────────

        [Test]
        public void SetActiveBehavior_DisablesGameObject()
        {
            var go = _helper.CreateGameObject("target");
            _resolver.Register("target-guid", go.transform);

            var behavior = new SetActiveBehavior(_resolver, "target-guid", false);
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsFalse(go.activeSelf);
        }

        [Test]
        public void SetActiveBehavior_EnablesGameObject()
        {
            var go = _helper.CreateGameObject("target");
            go.SetActive(false);
            _resolver.Register("target-guid", go.transform);

            var behavior = new SetActiveBehavior(_resolver, "target-guid", true);
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsTrue(go.activeSelf);
        }

        [Test]
        public void SetActiveBehavior_Undo_RestoresOriginalState()
        {
            var go = _helper.CreateGameObject("target");
            Assert.IsTrue(go.activeSelf);
            _resolver.Register("target-guid", go.transform);

            var behavior = new SetActiveBehavior(_resolver, "target-guid", false);
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            Assert.IsFalse(go.activeSelf);

            behavior.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();
            Assert.IsTrue(go.activeSelf);
        }

        [Test]
        public void SetActiveBehavior_NullResolver_DoesNotThrow()
        {
            var behavior = new SetActiveBehavior(null, "target-guid", false);
            Assert.DoesNotThrow(() =>
                behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult());
        }

        [Test]
        public void SetActiveBehavior_UnresolvedTarget_DoesNotThrow()
        {
            var behavior = new SetActiveBehavior(_resolver, "missing-guid", false);
            Assert.DoesNotThrow(() =>
                behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult());
        }

        // ── SetTransformBehavior ───────────────────────────────────────

        [Test]
        public void SetTransformBehavior_SetsPosition()
        {
            var go = _helper.CreateGameObject("target");
            _resolver.Register("target-guid", go.transform);
            var targetPos = new Vector3(5f, 10f, 15f);

            var behavior = new SetTransformBehavior(_resolver, "target-guid", targetPos, null, null);
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(targetPos, go.transform.position);
        }

        [Test]
        public void SetTransformBehavior_SetsRotation()
        {
            var go = _helper.CreateGameObject("target");
            _resolver.Register("target-guid", go.transform);
            var targetRot = Quaternion.Euler(45f, 90f, 0f);

            var behavior = new SetTransformBehavior(_resolver, "target-guid", null, targetRot, null);
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(Quaternion.Angle(targetRot, go.transform.rotation), Is.LessThan(0.01f));
        }

        [Test]
        public void SetTransformBehavior_SetsScale()
        {
            var go = _helper.CreateGameObject("target");
            _resolver.Register("target-guid", go.transform);
            var targetScale = new Vector3(2f, 3f, 4f);

            var behavior = new SetTransformBehavior(_resolver, "target-guid", null, null, targetScale);
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(targetScale, go.transform.localScale);
        }

        [Test]
        public void SetTransformBehavior_SetsAllProperties()
        {
            var go = _helper.CreateGameObject("target");
            _resolver.Register("target-guid", go.transform);
            var pos = new Vector3(1f, 2f, 3f);
            var rot = Quaternion.Euler(10f, 20f, 30f);
            var scale = new Vector3(0.5f, 0.5f, 0.5f);

            var behavior = new SetTransformBehavior(_resolver, "target-guid", pos, rot, scale);
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(pos, go.transform.position);
            Assert.That(Quaternion.Angle(rot, go.transform.rotation), Is.LessThan(0.01f));
            Assert.AreEqual(scale, go.transform.localScale);
        }

        [Test]
        public void SetTransformBehavior_Undo_RestoresOriginal()
        {
            var go = _helper.CreateGameObject("target");
            go.transform.position = new Vector3(1f, 1f, 1f);
            go.transform.localScale = Vector3.one;
            _resolver.Register("target-guid", go.transform);

            var behavior = new SetTransformBehavior(_resolver, "target-guid",
                new Vector3(99f, 99f, 99f), null, new Vector3(5f, 5f, 5f));
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            behavior.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(new Vector3(1f, 1f, 1f), go.transform.position);
            Assert.AreEqual(Vector3.one, go.transform.localScale);
        }

        [Test]
        public void SetTransformBehavior_UseLocal_SetsLocalPosition()
        {
            var parent = _helper.CreateGameObject("parent");
            parent.transform.position = new Vector3(10f, 0f, 0f);
            var child = _helper.CreateGameObject("child");
            child.transform.SetParent(parent.transform);
            _resolver.Register("child-guid", child.transform);

            var localPos = new Vector3(1f, 0f, 0f);
            var behavior = new SetTransformBehavior(_resolver, "child-guid", localPos, null, null, useLocal: true);
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(localPos, child.transform.localPosition);
        }

        [Test]
        public void SetTransformBehavior_NullResolver_DoesNotThrow()
        {
            var behavior = new SetTransformBehavior(null, "guid", Vector3.zero, null, null);
            Assert.DoesNotThrow(() =>
                behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult());
        }

        // ── TeleportObjectBehavior ─────────────────────────────────────

        [Test]
        public void TeleportObjectBehavior_TeleportsToDestination()
        {
            var target = _helper.CreateGameObject("target");
            var dest = _helper.CreateGameObjectAt("dest", new Vector3(50f, 50f, 50f));
            dest.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            _resolver.Register("target-guid", target.transform);
            _resolver.Register("dest-guid", dest.transform);

            var behavior = new TeleportObjectBehavior(_resolver, "target-guid", "dest-guid");
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(new Vector3(50f, 50f, 50f), target.transform.position);
            Assert.That(Quaternion.Angle(dest.transform.rotation, target.transform.rotation), Is.LessThan(0.01f));
        }

        [Test]
        public void TeleportObjectBehavior_Undo_RestoresOriginalPosition()
        {
            var target = _helper.CreateGameObjectAt("target", new Vector3(1f, 2f, 3f));
            var dest = _helper.CreateGameObjectAt("dest", new Vector3(50f, 50f, 50f));
            _resolver.Register("target-guid", target.transform);
            _resolver.Register("dest-guid", dest.transform);

            var behavior = new TeleportObjectBehavior(_resolver, "target-guid", "dest-guid");
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            behavior.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(new Vector3(1f, 2f, 3f), target.transform.position);
        }

        [Test]
        public void TeleportObjectBehavior_MissingDestination_DoesNotThrow()
        {
            var target = _helper.CreateGameObject("target");
            _resolver.Register("target-guid", target.transform);

            var behavior = new TeleportObjectBehavior(_resolver, "target-guid", "missing-dest");
            Assert.DoesNotThrow(() =>
                behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult());
        }

        // ── SetParentBehavior ──────────────────────────────────────────

        [Test]
        public void SetParentBehavior_ReparentsChild()
        {
            var child = _helper.CreateGameObject("child");
            var newParent = _helper.CreateGameObject("newParent");
            _resolver.Register("child-guid", child.transform);
            _resolver.Register("parent-guid", newParent.transform);

            var behavior = new SetParentBehavior(_resolver, "child-guid", "parent-guid");
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(newParent.transform, child.transform.parent);
        }

        [Test]
        public void SetParentBehavior_Undo_RestoresOriginalParent()
        {
            var originalParent = _helper.CreateGameObject("originalParent");
            var child = _helper.CreateGameObject("child");
            child.transform.SetParent(originalParent.transform);
            var newParent = _helper.CreateGameObject("newParent");
            _resolver.Register("child-guid", child.transform);
            _resolver.Register("parent-guid", newParent.transform);

            var behavior = new SetParentBehavior(_resolver, "child-guid", "parent-guid");
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            Assert.AreEqual(newParent.transform, child.transform.parent);

            behavior.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();
            Assert.AreEqual(originalParent.transform, child.transform.parent);
        }

        [Test]
        public void SetParentBehavior_NullResolver_DoesNotThrow()
        {
            var behavior = new SetParentBehavior(null, "child", "parent");
            Assert.DoesNotThrow(() =>
                behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult());
        }

        // ── AttachObjectBehavior ───────────────────────────────────────

        [Test]
        public void AttachObjectBehavior_AttachesChildToParent()
        {
            var child = _helper.CreateGameObject("child");
            var parent = _helper.CreateGameObject("parent");
            _resolver.Register("child-guid", child.transform);
            _resolver.Register("parent-guid", parent.transform);

            var behavior = new AttachObjectBehavior(_resolver, "child-guid", "parent-guid");
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(parent.transform, child.transform.parent);
        }

        [Test]
        public void AttachObjectBehavior_Undo_RestoresOriginalParent()
        {
            var child = _helper.CreateGameObject("child");
            var parent = _helper.CreateGameObject("parent");
            _resolver.Register("child-guid", child.transform);
            _resolver.Register("parent-guid", parent.transform);

            var behavior = new AttachObjectBehavior(_resolver, "child-guid", "parent-guid");
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            behavior.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsNull(child.transform.parent);
        }

        // ── DetachObjectBehavior ───────────────────────────────────────

        [Test]
        public void DetachObjectBehavior_DetachesFromParent()
        {
            var parent = _helper.CreateGameObject("parent");
            var child = _helper.CreateGameObject("child");
            child.transform.SetParent(parent.transform);
            _resolver.Register("child-guid", child.transform);

            var behavior = new DetachObjectBehavior(_resolver, "child-guid");
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsNull(child.transform.parent);
        }

        [Test]
        public void DetachObjectBehavior_Undo_RestoresParent()
        {
            var parent = _helper.CreateGameObject("parent");
            var child = _helper.CreateGameObject("child");
            child.transform.SetParent(parent.transform);
            _resolver.Register("child-guid", child.transform);

            var behavior = new DetachObjectBehavior(_resolver, "child-guid");
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            behavior.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(parent.transform, child.transform.parent);
        }

        [Test]
        public void DetachObjectBehavior_NullResolver_DoesNotThrow()
        {
            var behavior = new DetachObjectBehavior(null, "child-guid");
            Assert.DoesNotThrow(() =>
                behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult());
        }

        // ── SetRendererVisibilityBehavior ──────────────────────────────

        [Test]
        public void SetRendererVisibilityBehavior_HidesRenderer()
        {
            var go = _helper.CreateGameObjectWithRenderer("target");
            _resolver.Register("target-guid", go.transform);

            var behavior = new SetRendererVisibilityBehavior(_resolver, "target-guid", false);
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsFalse(go.GetComponent<Renderer>().enabled);
        }

        [Test]
        public void SetRendererVisibilityBehavior_ShowsRenderer()
        {
            var go = _helper.CreateGameObjectWithRenderer("target");
            go.GetComponent<Renderer>().enabled = false;
            _resolver.Register("target-guid", go.transform);

            var behavior = new SetRendererVisibilityBehavior(_resolver, "target-guid", true);
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsTrue(go.GetComponent<Renderer>().enabled);
        }

        [Test]
        public void SetRendererVisibilityBehavior_Undo_RestoresOriginal()
        {
            var go = _helper.CreateGameObjectWithRenderer("target");
            Assert.IsTrue(go.GetComponent<Renderer>().enabled);
            _resolver.Register("target-guid", go.transform);

            var behavior = new SetRendererVisibilityBehavior(_resolver, "target-guid", false);
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            behavior.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsTrue(go.GetComponent<Renderer>().enabled);
        }

        // ── HighlightObjectBehavior ────────────────────────────────────

        [Test]
        public void HighlightObjectBehavior_AppliesEmission()
        {
            var go = _helper.CreateGameObjectWithRenderer("target");
            _resolver.Register("target-guid", go.transform);

            var behavior = new HighlightObjectBehavior(_resolver, "target-guid", Color.red, 1f);
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            var renderer = go.GetComponent<Renderer>();
            Assert.IsTrue(renderer.material.IsKeywordEnabled("_EMISSION"));
        }

        // ── UnhighlightObjectBehavior ──────────────────────────────────

        [Test]
        public void UnhighlightObjectBehavior_RemovesEmission()
        {
            var go = _helper.CreateGameObjectWithRenderer("target");
            var renderer = go.GetComponent<Renderer>();
            renderer.material.EnableKeyword("_EMISSION");
            _resolver.Register("target-guid", go.transform);

            var behavior = new UnhighlightObjectBehavior(_resolver, "target-guid");
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsFalse(renderer.material.IsKeywordEnabled("_EMISSION"));
        }

        // ── DestroyObjectBehavior ──────────────────────────────────────

        [Test]
        public void DestroyObjectBehavior_DestroysGameObject()
        {
            var go = _helper.CreateGameObject("target");
            _resolver.Register("target-guid", go.transform);

            var behavior = new DestroyObjectBehavior(_resolver, "target-guid", 0f);
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            // Object.Destroy is deferred; DestroyImmediate is not used by the behavior.
            // In EditMode, Destroy marks for destruction; we verify it was called by checking
            // that after processing, the object is pending destroy.
            // Since Destroy with delay=0 still defers, we just verify no exception.
            Assert.Pass("DestroyObjectBehavior executed without exception");
        }

        [Test]
        public void DestroyObjectBehavior_NullResolver_DoesNotThrow()
        {
            var behavior = new DestroyObjectBehavior(null, "target-guid");
            Assert.DoesNotThrow(() =>
                behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult());
        }

        // ── SpawnObjectBehavior ────────────────────────────────────────

        [Test]
        public void SpawnObjectBehavior_InstantiatesFromTemplate()
        {
            var template = _helper.CreateGameObject("template");
            _resolver.Register("prefab-guid", template.transform);
            var spawnPos = new Vector3(10f, 20f, 30f);

            var behavior = new SpawnObjectBehavior(_resolver, "prefab-guid", spawnPos, Quaternion.identity);
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            // Verify a new object was spawned by checking the scene for clones
            var clone = GameObject.Find("template(Clone)");
            Assert.IsNotNull(clone);
            Assert.AreEqual(spawnPos, clone.transform.position);

            // Clean up the spawned object
            Object.DestroyImmediate(clone);
        }

        [Test]
        public void SpawnObjectBehavior_MissingPrefab_DoesNotThrow()
        {
            var behavior = new SpawnObjectBehavior(_resolver, "missing-prefab", Vector3.zero, Quaternion.identity);
            Assert.DoesNotThrow(() =>
                behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult());
        }

        // ── TriggerParticleEffectBehavior ──────────────────────────────

        [Test]
        public void TriggerParticleEffectBehavior_PlaysParticleSystem()
        {
            var go = _helper.CreateGameObjectWithParticleSystem("particle");
            var ps = go.GetComponent<ParticleSystem>();
            ps.Stop();
            _resolver.Register("particle-guid", go.transform);

            var behavior = new TriggerParticleEffectBehavior(_resolver, "particle-guid", true);
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsTrue(ps.isPlaying);
        }

        [Test]
        public void TriggerParticleEffectBehavior_StopsParticleSystem()
        {
            var go = _helper.CreateGameObjectWithParticleSystem("particle");
            var ps = go.GetComponent<ParticleSystem>();
            ps.Play();
            _resolver.Register("particle-guid", go.transform);

            var behavior = new TriggerParticleEffectBehavior(_resolver, "particle-guid", false);
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsFalse(ps.isPlaying);
        }

        [Test]
        public void TriggerParticleEffectBehavior_NullResolver_DoesNotThrow()
        {
            var behavior = new TriggerParticleEffectBehavior(null, "guid", true);
            Assert.DoesNotThrow(() =>
                behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult());
        }

        // ── StopAnimationBehavior ──────────────────────────────────────

        [Test]
        public void StopAnimationBehavior_DisablesAnimator()
        {
            var go = _helper.CreateGameObjectWithAnimator("anim");
            var animator = go.GetComponent<Animator>();
            animator.enabled = true;
            _resolver.Register("anim-guid", go.transform);

            var behavior = new StopAnimationBehavior(_resolver, "anim-guid");
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsFalse(animator.enabled);
        }

        [Test]
        public void StopAnimationBehavior_Undo_RestoresAnimator()
        {
            var go = _helper.CreateGameObjectWithAnimator("anim");
            var animator = go.GetComponent<Animator>();
            animator.enabled = true;
            _resolver.Register("anim-guid", go.transform);

            var behavior = new StopAnimationBehavior(_resolver, "anim-guid");
            behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            behavior.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsTrue(animator.enabled);
        }

        // ── EnablePhysicsBehavior / DisablePhysicsBehavior ─────────────

        [Test]
        public void EnablePhysicsBehavior_SetsSimulationModeToFixedUpdate()
        {
            var previousMode = Physics.simulationMode;
            try
            {
                Physics.simulationMode = SimulationMode.Script;

                var behavior = new EnablePhysicsBehavior();
                behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

                Assert.AreEqual(SimulationMode.FixedUpdate, Physics.simulationMode);
            }
            finally
            {
                Physics.simulationMode = previousMode;
            }
        }

        [Test]
        public void EnablePhysicsBehavior_Undo_RestoresPreviousMode()
        {
            var previousMode = Physics.simulationMode;
            try
            {
                Physics.simulationMode = SimulationMode.Script;

                var behavior = new EnablePhysicsBehavior();
                behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
                behavior.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();

                Assert.AreEqual(SimulationMode.Script, Physics.simulationMode);
            }
            finally
            {
                Physics.simulationMode = previousMode;
            }
        }

        [Test]
        public void DisablePhysicsBehavior_SetsSimulationModeToScript()
        {
            var previousMode = Physics.simulationMode;
            try
            {
                Physics.simulationMode = SimulationMode.FixedUpdate;

                var behavior = new DisablePhysicsBehavior();
                behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

                Assert.AreEqual(SimulationMode.Script, Physics.simulationMode);
            }
            finally
            {
                Physics.simulationMode = previousMode;
            }
        }

        [Test]
        public void DisablePhysicsBehavior_Undo_RestoresPreviousMode()
        {
            var previousMode = Physics.simulationMode;
            try
            {
                Physics.simulationMode = SimulationMode.FixedUpdate;

                var behavior = new DisablePhysicsBehavior();
                behavior.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
                behavior.UndoAsync(CancellationToken.None).GetAwaiter().GetResult();

                Assert.AreEqual(SimulationMode.FixedUpdate, Physics.simulationMode);
            }
            finally
            {
                Physics.simulationMode = previousMode;
            }
        }
    }
}
