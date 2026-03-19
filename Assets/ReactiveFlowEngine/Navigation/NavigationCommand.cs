using System;

namespace ReactiveFlowEngine.Navigation
{
    public enum NavigationCommandType
    {
        Next,
        Previous,
        GoToStep,
        JumpToChapter,
        Restart
    }

    public sealed class NavigationCommand
    {
        public NavigationCommandType CommandType { get; private set; }
        public string TargetId { get; private set; }

        public NavigationCommand(NavigationCommandType commandType, string targetId = null)
        {
            CommandType = commandType;
            TargetId = targetId;
        }

        public static NavigationCommand Next()
        {
            return new NavigationCommand(NavigationCommandType.Next);
        }

        public static NavigationCommand Previous()
        {
            return new NavigationCommand(NavigationCommandType.Previous);
        }

        public static NavigationCommand GoTo(string stepId)
        {
            if (string.IsNullOrEmpty(stepId))
                throw new ArgumentException("stepId cannot be null or empty", nameof(stepId));

            return new NavigationCommand(NavigationCommandType.GoToStep, stepId);
        }

        public static NavigationCommand JumpChapter(string chapterId)
        {
            if (string.IsNullOrEmpty(chapterId))
                throw new ArgumentException("chapterId cannot be null or empty", nameof(chapterId));

            return new NavigationCommand(NavigationCommandType.JumpToChapter, chapterId);
        }

        public static NavigationCommand Restart()
        {
            return new NavigationCommand(NavigationCommandType.Restart);
        }

        public override bool Equals(object obj)
        {
            if (obj is not NavigationCommand other)
                return false;

            return CommandType == other.CommandType && TargetId == other.TargetId;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (CommandType.GetHashCode() * 397) ^ (TargetId?.GetHashCode() ?? 0);
            }
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(TargetId))
                return $"NavigationCommand({CommandType})";
            return $"NavigationCommand({CommandType}, {TargetId})";
        }
    }
}
