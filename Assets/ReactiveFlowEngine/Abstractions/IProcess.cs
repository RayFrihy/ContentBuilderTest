using System.Collections.Generic;

namespace ReactiveFlowEngine.Abstractions
{
    public interface IProcess
    {
        string Id { get; }
        string Name { get; }
        IReadOnlyList<IChapter> Chapters { get; }
        IChapter FirstChapter { get; }
    }
}
