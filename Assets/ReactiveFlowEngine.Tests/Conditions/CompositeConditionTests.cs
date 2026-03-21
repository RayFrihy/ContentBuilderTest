using System;
using NUnit.Framework;
using R3;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Conditions;
using ReactiveFlowEngine.Conditions.Composite;
using ReactiveFlowEngine.Tests.TestDoubles;

namespace ReactiveFlowEngine.Tests.Conditions
{
    [TestFixture]
    public class CompositeConditionTests
    {
        // ── CompositeAndCondition ──────────────────────────────────────

        [Test]
        public void CompositeAndCondition_NullChildren_CoalescesToEmpty()
        {
            var and = new CompositeAndCondition(null);
            Assert.AreEqual(0, and.Children.Count);
        }

        [Test]
        public void CompositeAndCondition_EmptyChildren_ReturnsTrue()
        {
            var and = new CompositeAndCondition();
            bool? result = null;
            var sub = and.Evaluate().Subscribe(v => result = v);
            Assert.AreEqual(true, result);
            sub.Dispose();
        }

        [Test]
        public void CompositeAndCondition_AllTrue_ReturnsTrue()
        {
            var c1 = new TestCondition();
            var c2 = new TestCondition();
            var and = new CompositeAndCondition(c1, c2);
            bool? result = null;
            var sub = and.Evaluate().Subscribe(v => result = v);

            Assert.AreEqual(false, result);
            c1.EmitResult(true);
            Assert.AreEqual(false, result);
            c2.EmitResult(true);
            Assert.AreEqual(true, result);
            sub.Dispose();
        }

        [Test]
        public void CompositeAndCondition_OneFalse_ReturnsFalse()
        {
            var c1 = new TestCondition();
            var c2 = new TestCondition();
            var and = new CompositeAndCondition(c1, c2);
            bool? result = null;
            var sub = and.Evaluate().Subscribe(v => result = v);

            c1.EmitResult(true);
            c2.EmitResult(true);
            Assert.AreEqual(true, result);

            c1.EmitResult(false);
            Assert.AreEqual(false, result);
            sub.Dispose();
        }

        [Test]
        public void CompositeAndCondition_Reset_PropagatesChildren()
        {
            var c1 = new TestCondition();
            var c2 = new TestCondition();
            var and = new CompositeAndCondition(c1, c2);

            and.Reset();

            Assert.AreEqual(1, c1.ResetCount);
            Assert.AreEqual(1, c2.ResetCount);
        }

        [Test]
        public void CompositeAndCondition_Dispose_PropagatesChildren()
        {
            var c1 = new TestCondition();
            var c2 = new TestCondition();
            var and = new CompositeAndCondition(c1, c2);

            and.Dispose();

            Assert.IsTrue(c1.IsDisposed);
            Assert.IsTrue(c2.IsDisposed);
        }

        // ── CompositeOrCondition ───────────────────────────────────────

        [Test]
        public void CompositeOrCondition_NullChildren_CoalescesToEmpty()
        {
            var or = new CompositeOrCondition(null);
            Assert.AreEqual(0, or.Children.Count);
        }

        [Test]
        public void CompositeOrCondition_EmptyChildren_ReturnsFalse()
        {
            var or = new CompositeOrCondition();
            bool? result = null;
            var sub = or.Evaluate().Subscribe(v => result = v);
            Assert.AreEqual(false, result);
            sub.Dispose();
        }

        [Test]
        public void CompositeOrCondition_OneTrue_ReturnsTrue()
        {
            var c1 = new TestCondition();
            var c2 = new TestCondition();
            var or = new CompositeOrCondition(c1, c2);
            bool? result = null;
            var sub = or.Evaluate().Subscribe(v => result = v);

            Assert.AreEqual(false, result);
            c1.EmitResult(true);
            Assert.AreEqual(true, result);
            sub.Dispose();
        }

        [Test]
        public void CompositeOrCondition_Reset_PropagatesChildren()
        {
            var c1 = new TestCondition();
            var or = new CompositeOrCondition(c1);

            or.Reset();

            Assert.AreEqual(1, c1.ResetCount);
        }

        [Test]
        public void CompositeOrCondition_Dispose_PropagatesChildren()
        {
            var c1 = new TestCondition();
            var or = new CompositeOrCondition(c1);

            or.Dispose();

            Assert.IsTrue(c1.IsDisposed);
        }

        // ── CompositeNotCondition ──────────────────────────────────────

        [Test]
        public void CompositeNotCondition_NullInner_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CompositeNotCondition(null));
        }

        [Test]
        public void CompositeNotCondition_NegatesInner()
        {
            var inner = new TestCondition();
            var not = new CompositeNotCondition(inner);
            bool? result = null;
            var sub = not.Evaluate().Subscribe(v => result = v);

            // inner prepends false, negated => true
            Assert.AreEqual(true, result);
            inner.EmitResult(true);
            Assert.AreEqual(false, result);
            inner.EmitResult(false);
            Assert.AreEqual(true, result);
            sub.Dispose();
        }

        [Test]
        public void CompositeNotCondition_Reset_PropagatesToInner()
        {
            var inner = new TestCondition();
            var not = new CompositeNotCondition(inner);

            not.Reset();

            Assert.AreEqual(1, inner.ResetCount);
        }

        [Test]
        public void CompositeNotCondition_Dispose_PropagatesToInner()
        {
            var inner = new TestCondition();
            var not = new CompositeNotCondition(inner);

            not.Dispose();

            Assert.IsTrue(inner.IsDisposed);
        }

        // ── XorCondition ───────────────────────────────────────────────

        [Test]
        public void XorCondition_NullChildren_CoalescesToEmpty()
        {
            var xor = new XorCondition(null);
            Assert.AreEqual(0, xor.Children.Count);
        }

        [Test]
        public void XorCondition_EmptyChildren_ReturnsFalse()
        {
            var xor = new XorCondition();
            bool? result = null;
            var sub = xor.Evaluate().Subscribe(v => result = v);
            Assert.AreEqual(false, result);
            sub.Dispose();
        }

        [Test]
        public void XorCondition_ExactlyOneTrue_ReturnsTrue()
        {
            var c1 = new TestCondition();
            var c2 = new TestCondition();
            var xor = new XorCondition(c1, c2);
            bool? result = null;
            var sub = xor.Evaluate().Subscribe(v => result = v);

            // Both start false => 0 true => false
            Assert.AreEqual(false, result);
            c1.EmitResult(true);
            // Exactly one true => true
            Assert.AreEqual(true, result);
            c2.EmitResult(true);
            // Two true => false
            Assert.AreEqual(false, result);
            sub.Dispose();
        }

        [Test]
        public void XorCondition_Reset_PropagatesChildren()
        {
            var c1 = new TestCondition();
            var c2 = new TestCondition();
            var xor = new XorCondition(c1, c2);

            xor.Reset();

            Assert.AreEqual(1, c1.ResetCount);
            Assert.AreEqual(1, c2.ResetCount);
        }

        [Test]
        public void XorCondition_Dispose_PropagatesChildren()
        {
            var c1 = new TestCondition();
            var xor = new XorCondition(c1);

            xor.Dispose();

            Assert.IsTrue(c1.IsDisposed);
        }

        // ── WeightedCondition ──────────────────────────────────────────

        [Test]
        public void WeightedConditionEntry_NullCondition_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new WeightedConditionEntry(null, 1.0f));
        }

        [Test]
        public void WeightedCondition_EmptyEntries_ReturnsFalse()
        {
            var wc = new WeightedCondition(0.5f);
            bool? result = null;
            var sub = wc.Evaluate().Subscribe(v => result = v);
            Assert.AreEqual(false, result);
            sub.Dispose();
        }

        [Test]
        public void WeightedCondition_AchievedWeightMeetsThreshold_ReturnsTrue()
        {
            var c1 = new TestCondition();
            var c2 = new TestCondition();
            var wc = new WeightedCondition(0.5f,
                new WeightedConditionEntry(c1, 3.0f),
                new WeightedConditionEntry(c2, 1.0f));
            bool? result = null;
            var sub = wc.Evaluate().Subscribe(v => result = v);

            // Both false => 0/4 => false
            Assert.AreEqual(false, result);

            // c1 true => 3/4 = 0.75 >= 0.5 => true
            c1.EmitResult(true);
            Assert.AreEqual(true, result);

            // c1 false, c2 true => 1/4 = 0.25 < 0.5 => false
            c1.EmitResult(false);
            c2.EmitResult(true);
            Assert.AreEqual(false, result);

            sub.Dispose();
        }

        [Test]
        public void WeightedCondition_Reset_PropagatesChildren()
        {
            var c1 = new TestCondition();
            var wc = new WeightedCondition(0.5f, new WeightedConditionEntry(c1, 1.0f));

            wc.Reset();

            Assert.AreEqual(1, c1.ResetCount);
        }

        [Test]
        public void WeightedCondition_Dispose_PropagatesChildren()
        {
            var c1 = new TestCondition();
            var wc = new WeightedCondition(0.5f, new WeightedConditionEntry(c1, 1.0f));

            wc.Dispose();

            Assert.IsTrue(c1.IsDisposed);
        }

        // ── PriorityCondition ──────────────────────────────────────────

        [Test]
        public void PriorityConditionEntry_NullCondition_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PriorityConditionEntry(null, 1));
        }

        [Test]
        public void PriorityCondition_EmptyEntries_ReturnsFalse()
        {
            var pc = new PriorityCondition();
            bool? result = null;
            var sub = pc.Evaluate().Subscribe(v => result = v);
            Assert.AreEqual(false, result);
            sub.Dispose();
        }

        [Test]
        public void PriorityCondition_AnyTrue_ReturnsTrue()
        {
            var cLow = new TestCondition();
            var cHigh = new TestCondition();
            var pc = new PriorityCondition(
                new PriorityConditionEntry(cLow, 1),
                new PriorityConditionEntry(cHigh, 10));
            bool? result = null;
            var sub = pc.Evaluate().Subscribe(v => result = v);

            // Both false => false
            Assert.AreEqual(false, result);

            cLow.EmitResult(true);
            Assert.AreEqual(true, result);

            cLow.EmitResult(false);
            Assert.AreEqual(false, result);

            cHigh.EmitResult(true);
            Assert.AreEqual(true, result);

            sub.Dispose();
        }

        [Test]
        public void PriorityCondition_SortsByPriorityDescending()
        {
            var c1 = new TestCondition();
            var c2 = new TestCondition();
            var pc = new PriorityCondition(
                new PriorityConditionEntry(c1, 1),
                new PriorityConditionEntry(c2, 100));

            // After sorting, c2 (priority 100) should be first
            Assert.AreSame(c2, pc.Children[0]);
            Assert.AreSame(c1, pc.Children[1]);
        }

        [Test]
        public void PriorityCondition_Reset_PropagatesChildren()
        {
            var c1 = new TestCondition();
            var pc = new PriorityCondition(new PriorityConditionEntry(c1, 1));

            pc.Reset();

            Assert.AreEqual(1, c1.ResetCount);
        }

        [Test]
        public void PriorityCondition_Dispose_PropagatesChildren()
        {
            var c1 = new TestCondition();
            var pc = new PriorityCondition(new PriorityConditionEntry(c1, 1));

            pc.Dispose();

            Assert.IsTrue(c1.IsDisposed);
        }

        // ── CompositeCondition (mode-based) ────────────────────────────

        [Test]
        public void CompositeCondition_EmptyAll_ReturnsTrue()
        {
            var cc = new CompositeCondition(CompositeMode.All);
            bool? result = null;
            var sub = cc.Evaluate().Subscribe(v => result = v);
            Assert.AreEqual(true, result);
            sub.Dispose();
        }

        [Test]
        public void CompositeCondition_EmptyNone_ReturnsTrue()
        {
            var cc = new CompositeCondition(CompositeMode.None);
            bool? result = null;
            var sub = cc.Evaluate().Subscribe(v => result = v);
            Assert.AreEqual(true, result);
            sub.Dispose();
        }

        [Test]
        public void CompositeCondition_EmptyAny_ReturnsFalse()
        {
            var cc = new CompositeCondition(CompositeMode.Any);
            bool? result = null;
            var sub = cc.Evaluate().Subscribe(v => result = v);
            Assert.AreEqual(false, result);
            sub.Dispose();
        }

        [Test]
        public void CompositeCondition_ModeAll_AllTrue_ReturnsTrue()
        {
            var c1 = new TestCondition();
            var c2 = new TestCondition();
            var cc = new CompositeCondition(CompositeMode.All, c1, c2);
            bool? result = null;
            var sub = cc.Evaluate().Subscribe(v => result = v);

            Assert.AreEqual(false, result);
            c1.EmitResult(true);
            c2.EmitResult(true);
            Assert.AreEqual(true, result);
            sub.Dispose();
        }

        [Test]
        public void CompositeCondition_ModeNone_AllFalse_ReturnsTrue()
        {
            var c1 = new TestCondition();
            var c2 = new TestCondition();
            var cc = new CompositeCondition(CompositeMode.None, c1, c2);
            bool? result = null;
            var sub = cc.Evaluate().Subscribe(v => result = v);

            // Both start false => none true => true
            Assert.AreEqual(true, result);
            c1.EmitResult(true);
            Assert.AreEqual(false, result);
            sub.Dispose();
        }

        [Test]
        public void CompositeCondition_ModeExactlyOne_OneTrue_ReturnsTrue()
        {
            var c1 = new TestCondition();
            var c2 = new TestCondition();
            var cc = new CompositeCondition(CompositeMode.ExactlyOne, c1, c2);
            bool? result = null;
            var sub = cc.Evaluate().Subscribe(v => result = v);

            // Both false => 0 true => false
            Assert.AreEqual(false, result);
            c1.EmitResult(true);
            Assert.AreEqual(true, result);
            c2.EmitResult(true);
            Assert.AreEqual(false, result);
            sub.Dispose();
        }

        [Test]
        public void CompositeCondition_ModeAtLeast_ThresholdMet_ReturnsTrue()
        {
            var c1 = new TestCondition();
            var c2 = new TestCondition();
            var c3 = new TestCondition();
            var cc = new CompositeCondition(CompositeMode.AtLeast, 2, c1, c2, c3);
            bool? result = null;
            var sub = cc.Evaluate().Subscribe(v => result = v);

            Assert.AreEqual(false, result);
            c1.EmitResult(true);
            Assert.AreEqual(false, result);
            c2.EmitResult(true);
            Assert.AreEqual(true, result);
            sub.Dispose();
        }

        [Test]
        public void CompositeCondition_ModeAtMost_ThresholdRespected()
        {
            var c1 = new TestCondition();
            var c2 = new TestCondition();
            var c3 = new TestCondition();
            var cc = new CompositeCondition(CompositeMode.AtMost, 1, c1, c2, c3);
            bool? result = null;
            var sub = cc.Evaluate().Subscribe(v => result = v);

            // 0 true <= 1 => true
            Assert.AreEqual(true, result);
            c1.EmitResult(true);
            // 1 true <= 1 => true
            Assert.AreEqual(true, result);
            c2.EmitResult(true);
            // 2 true <= 1 => false
            Assert.AreEqual(false, result);
            sub.Dispose();
        }

        [Test]
        public void CompositeCondition_Reset_PropagatesChildren()
        {
            var c1 = new TestCondition();
            var c2 = new TestCondition();
            var cc = new CompositeCondition(CompositeMode.All, c1, c2);

            cc.Reset();

            Assert.AreEqual(1, c1.ResetCount);
            Assert.AreEqual(1, c2.ResetCount);
        }

        [Test]
        public void CompositeCondition_Dispose_PropagatesChildren()
        {
            var c1 = new TestCondition();
            var c2 = new TestCondition();
            var cc = new CompositeCondition(CompositeMode.All, c1, c2);

            cc.Dispose();

            Assert.IsTrue(c1.IsDisposed);
            Assert.IsTrue(c2.IsDisposed);
        }
    }
}
