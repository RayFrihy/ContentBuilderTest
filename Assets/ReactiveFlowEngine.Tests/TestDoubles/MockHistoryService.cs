using System.Collections.Generic;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Navigation;

namespace ReactiveFlowEngine.Tests.TestDoubles
{
    public class MockHistoryService : IHistoryService
    {
        private readonly Stack<HistoryEntry> _stack = new Stack<HistoryEntry>();

        public bool CanGoBack => _stack.Count > 0;

        public void Push(HistoryEntry entry) => _stack.Push(entry);
        public HistoryEntry Pop() => _stack.Count > 0 ? _stack.Pop() : null;
        public HistoryEntry Peek() => _stack.Count > 0 ? _stack.Peek() : null;
        public bool Contains(string stepId)
        {
            foreach (var e in _stack)
                if (e.StepId == stepId) return true;
            return false;
        }
        public IReadOnlyList<HistoryEntry> GetAll() => new List<HistoryEntry>(_stack);
        public void Clear() => _stack.Clear();
    }
}
