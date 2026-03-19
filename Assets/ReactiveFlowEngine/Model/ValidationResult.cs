using System.Collections.Generic;

namespace ReactiveFlowEngine.Model
{
    public sealed class ValidationResult
    {
        public bool IsValid => Errors.Count == 0;
        public List<string> Errors { get; } = new List<string>();
        public List<string> Warnings { get; } = new List<string>();
        public List<string> SceneObjectGuids { get; } = new List<string>();

        public void AddError(string message) => Errors.Add(message);
        public void AddWarning(string message) => Warnings.Add(message);
        public void AddSceneObjectGuid(string guid)
        {
            if (!SceneObjectGuids.Contains(guid))
                SceneObjectGuids.Add(guid);
        }
    }
}
