using System.Collections.Generic;
using ReactiveFlowEngine.Navigation;

namespace ReactiveFlowEngine.Abstractions
{
    public interface IHistoryService
    {
        void Push(HistoryEntry entry);
        HistoryEntry Pop();
        HistoryEntry Peek();
        bool CanGoBack { get; }
        bool Contains(string stepId);
        IReadOnlyList<HistoryEntry> GetAll();
        void Clear();
    }
}
