using NUnit.Framework;
using ReactiveFlowEngine.Navigation;

namespace ReactiveFlowEngine.Tests
{
    [TestFixture]
    public class HistoryStackTests
    {
        private HistoryStack _stack;

        [SetUp]
        public void SetUp()
        {
            _stack = new HistoryStack();
        }

        [Test]
        public void Push_AddsEntry()
        {
            _stack.Push(new HistoryEntry("ch1", "step1"));
            Assert.AreEqual(1, _stack.Count);
        }

        [Test]
        public void Push_NullEntry_DoesNotAdd()
        {
            _stack.Push(null);
            Assert.AreEqual(0, _stack.Count);
        }

        [Test]
        public void Pop_ReturnsAndRemovesLastEntry()
        {
            _stack.Push(new HistoryEntry("ch1", "step1"));
            _stack.Push(new HistoryEntry("ch1", "step2"));

            var popped = _stack.Pop();
            Assert.AreEqual("step2", popped.StepId);
            Assert.AreEqual(1, _stack.Count);
        }

        [Test]
        public void Pop_ReturnsNull_WhenEmpty()
        {
            var result = _stack.Pop();
            Assert.IsNull(result);
        }

        [Test]
        public void Peek_ReturnsLastEntry_WithoutRemoving()
        {
            _stack.Push(new HistoryEntry("ch1", "step1"));
            var peeked = _stack.Peek();
            Assert.AreEqual("step1", peeked.StepId);
            Assert.AreEqual(1, _stack.Count);
        }

        [Test]
        public void Contains_FindsExistingStepId()
        {
            _stack.Push(new HistoryEntry("ch1", "step1"));
            _stack.Push(new HistoryEntry("ch1", "step2"));

            Assert.IsTrue(_stack.Contains("step1"));
            Assert.IsTrue(_stack.Contains("step2"));
            Assert.IsFalse(_stack.Contains("step3"));
        }

        [Test]
        public void Contains_ReturnsFalse_ForNull()
        {
            _stack.Push(new HistoryEntry("ch1", "step1"));
            Assert.IsFalse(_stack.Contains(null));
        }

        [Test]
        public void Clear_RemovesAllEntries()
        {
            _stack.Push(new HistoryEntry("ch1", "step1"));
            _stack.Push(new HistoryEntry("ch1", "step2"));
            _stack.Clear();

            Assert.AreEqual(0, _stack.Count);
            Assert.IsFalse(_stack.CanGoBack);
        }

        [Test]
        public void CanGoBack_ReturnsFalse_WhenEmpty()
        {
            Assert.IsFalse(_stack.CanGoBack);
        }

        [Test]
        public void CanGoBack_ReturnsTrue_WhenHasEntries()
        {
            _stack.Push(new HistoryEntry("ch1", "step1"));
            Assert.IsTrue(_stack.CanGoBack);
        }

        [Test]
        public void GetAll_ReturnsAllEntries()
        {
            _stack.Push(new HistoryEntry("ch1", "step1"));
            _stack.Push(new HistoryEntry("ch1", "step2"));

            var all = _stack.GetAll();
            Assert.AreEqual(2, all.Count);
            Assert.AreEqual("step1", all[0].StepId);
            Assert.AreEqual("step2", all[1].StepId);
        }

        [Test]
        public void GetEntriesUntil_ReturnsSubset()
        {
            _stack.Push(new HistoryEntry("ch1", "step1"));
            _stack.Push(new HistoryEntry("ch1", "step2"));
            _stack.Push(new HistoryEntry("ch1", "step3"));

            var subset = _stack.GetEntriesUntil("step2");
            Assert.AreEqual(2, subset.Count);
            Assert.AreEqual("step1", subset[0].StepId);
            Assert.AreEqual("step2", subset[1].StepId);
        }

        [Test]
        public void IndexOf_ReturnsCorrectIndex()
        {
            _stack.Push(new HistoryEntry("ch1", "step1"));
            _stack.Push(new HistoryEntry("ch1", "step2"));

            Assert.AreEqual(0, _stack.IndexOf("step1"));
            Assert.AreEqual(1, _stack.IndexOf("step2"));
            Assert.AreEqual(-1, _stack.IndexOf("step3"));
            Assert.AreEqual(-1, _stack.IndexOf(null));
        }

        [Test]
        public void HistoryEntry_Equality_WorksCorrectly()
        {
            var a = new HistoryEntry("ch1", "step1", 0);
            var b = new HistoryEntry("ch1", "step1", 0);
            var c = new HistoryEntry("ch1", "step2", 0);

            Assert.AreEqual(a, b);
            Assert.AreNotEqual(a, c);
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }
    }
}
