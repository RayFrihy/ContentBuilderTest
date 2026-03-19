using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;

namespace ReactiveFlowEngine.Behaviors
{
    public class ExecuteChapterBehavior : IExecuteChapterBehavior
    {
        private readonly ChapterModel _subChapter;
        private Func<IChapter, CancellationToken, UniTask> _chapterRunner;

        public ExecutionStages Stages => ExecutionStages.Activation;
        public bool IsBlocking => true;

        public ExecuteChapterBehavior(ChapterModel subChapter, Func<IChapter, CancellationToken, UniTask> chapterRunner)
        {
            _subChapter = subChapter;
            _chapterRunner = chapterRunner;
        }

        public void SetChapterRunner(Func<IChapter, CancellationToken, UniTask> chapterRunner)
        {
            _chapterRunner = chapterRunner;
        }

        public IChapter GetSubChapter() => _subChapter;

        public async UniTask ExecuteAsync(CancellationToken ct)
        {
            if (_subChapter == null)
            {
                UnityEngine.Debug.LogWarning("[RFE] ExecuteChapterBehavior: Sub-chapter is null, skipping.");
                return;
            }

            if (_chapterRunner == null)
            {
                UnityEngine.Debug.LogWarning("[RFE] ExecuteChapterBehavior: ChapterRunner delegate is null, skipping.");
                return;
            }

            await _chapterRunner(_subChapter, ct);
        }

        public async UniTask UndoAsync(CancellationToken ct)
        {
            // Undo of sub-chapter execution is handled by the NavigationService
            // which unwinds the sub-chapter's history stack
            await UniTask.CompletedTask;
        }
    }
}
