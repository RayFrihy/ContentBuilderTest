using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Serialization.JsonModels;
using UnityEngine;

namespace ReactiveFlowEngine.Serialization
{
    public class VRBuilderJsonLoader : IProcessLoader
    {
        private readonly RfeTypeBinder _typeBinder;
        private readonly ModelBuilder _modelBuilder;

        public VRBuilderJsonLoader(RfeTypeBinder typeBinder, ModelBuilder modelBuilder)
        {
            _typeBinder = typeBinder;
            _modelBuilder = modelBuilder;
        }

        public UniTask<IProcess> LoadAsync(string json, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            // Phase 1: Deserialize JSON to DTO graph
            var settings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                TypeNameHandling = TypeNameHandling.Auto,
                SerializationBinder = _typeBinder,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                Error = (sender, args) =>
                {
                    // Log but don't fail on non-critical deserialization errors
                    Debug.LogWarning($"[RFE] JSON deserialization warning: {args.ErrorContext.Error.Message} at path: {args.ErrorContext.Path}");
                    args.ErrorContext.Handled = true;
                }
            };

            var wrapper = JsonConvert.DeserializeObject<JsonProcessWrapper>(json, settings);
            if (wrapper == null)
            {
                Debug.LogError("[RFE] Failed to deserialize JSON process wrapper.");
                return UniTask.FromResult<IProcess>(null);
            }

            ct.ThrowIfCancellationRequested();

            // Phase 2: Build domain model from DTO graph
            var process = _modelBuilder.Build(wrapper);

            // Validate
            var validation = _modelBuilder.Validate(process, wrapper);
            foreach (var error in validation.Errors)
                Debug.LogError($"[RFE] Validation error: {error}");
            foreach (var warning in validation.Warnings)
                Debug.LogWarning($"[RFE] Validation warning: {warning}");
            foreach (var guid in validation.SceneObjectGuids)
                Debug.Log($"[RFE] Scene object GUID referenced: {guid}");

            if (!validation.IsValid)
            {
                Debug.LogError("[RFE] Process validation failed. Aborting load.");
                return UniTask.FromResult<IProcess>(null);
            }

            return UniTask.FromResult<IProcess>(process);
        }
    }
}
