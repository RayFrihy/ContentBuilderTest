using System.Collections.Generic;

namespace ReactiveFlowEngine.Navigation
{
    public sealed class HistoryEntry
    {
        public string ChapterId { get; set; }
        public string StepId { get; set; }
        public int Depth { get; set; }

        public HistoryEntry(string chapterId, string stepId, int depth = 0)
        {
            ChapterId = chapterId;
            StepId = stepId;
            Depth = depth;
        }

        public override bool Equals(object obj)
        {
            if (obj is not HistoryEntry other)
                return false;
            return ChapterId == other.ChapterId && StepId == other.StepId && Depth == other.Depth;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ChapterId?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (StepId?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ Depth.GetHashCode();
                return hashCode;
            }
        }
    }

    public class HistoryStack
    {
        private readonly List<HistoryEntry> _entries = new List<HistoryEntry>();
        private readonly object _lockObject = new object();

        public int Count
        {
            get
            {
                lock (_lockObject)
                {
                    return _entries.Count;
                }
            }
        }

        public bool CanGoBack
        {
            get
            {
                lock (_lockObject)
                {
                    return _entries.Count > 0;
                }
            }
        }

        public void Push(HistoryEntry entry)
        {
            if (entry == null)
                return;

            lock (_lockObject)
            {
                _entries.Add(entry);
            }
        }

        public HistoryEntry Pop()
        {
            lock (_lockObject)
            {
                if (_entries.Count == 0)
                    return null;

                var last = _entries[_entries.Count - 1];
                _entries.RemoveAt(_entries.Count - 1);
                return last;
            }
        }

        public HistoryEntry Peek()
        {
            lock (_lockObject)
            {
                if (_entries.Count == 0)
                    return null;

                return _entries[_entries.Count - 1];
            }
        }

        public bool Contains(string stepId)
        {
            if (stepId == null)
                return false;

            lock (_lockObject)
            {
                for (int i = 0; i < _entries.Count; i++)
                {
                    if (_entries[i].StepId == stepId)
                        return true;
                }
                return false;
            }
        }

        public IReadOnlyList<HistoryEntry> GetAll()
        {
            lock (_lockObject)
            {
                return new List<HistoryEntry>(_entries).AsReadOnly();
            }
        }

        public void Clear()
        {
            lock (_lockObject)
            {
                _entries.Clear();
            }
        }

        public List<HistoryEntry> GetEntriesUntil(string stepId)
        {
            lock (_lockObject)
            {
                var result = new List<HistoryEntry>();
                for (int i = 0; i < _entries.Count; i++)
                {
                    result.Add(_entries[i]);
                    if (_entries[i].StepId == stepId)
                        break;
                }
                return result;
            }
        }

        public int IndexOf(string stepId)
        {
            if (stepId == null)
                return -1;

            lock (_lockObject)
            {
                for (int i = 0; i < _entries.Count; i++)
                {
                    if (_entries[i].StepId == stepId)
                        return i;
                }
                return -1;
            }
        }
    }
}
