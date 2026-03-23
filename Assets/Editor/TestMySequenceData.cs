using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using ReactiveFlowEngine.Serialization;
using ReactiveFlowEngine.Serialization.JsonModels;
using System.Collections.Generic;
using System.Text;

public static class TestMySequenceData
{
    [MenuItem("Tools/Test MySequenceData")]
    public static void Run()
    {
        var sb = new StringBuilder();
        var outputPath = System.IO.Path.Combine(Application.dataPath, "..", "test_output.txt");

        try
        {
            var jsonPath = System.IO.Path.Combine(Application.dataPath, "MySequenceData.json");
            var json = System.IO.File.ReadAllText(jsonPath);
            sb.AppendLine("JSON loaded, length=" + json.Length);

            var binder = new RfeTypeBinder();
            var builder = new ModelBuilder();
            var loader = new VRBuilderJsonLoader(binder, builder);

            var task = loader.LoadAsync(json, System.Threading.CancellationToken.None);
            var process = task.GetAwaiter().GetResult();

            if (process == null)
            {
                sb.AppendLine("ERROR: Process is NULL - load failed");
                System.IO.File.WriteAllText(outputPath, sb.ToString());
                Debug.LogError("[TEST] Process is NULL");
                return;
            }

            sb.AppendLine("Process: " + process.Name + " (ID=" + process.Id + ")");
            sb.AppendLine("Chapters: " + process.Chapters.Count);

            foreach (var chapter in process.Chapters)
            {
                sb.AppendLine("  Chapter: '" + chapter.Name + "' ID=" + chapter.Id);
                sb.AppendLine("  Steps: " + chapter.Steps.Count);
                sb.AppendLine("  FirstStep: " + (chapter.FirstStep != null ? chapter.FirstStep.Name : "null"));

                foreach (var step in chapter.Steps)
                {
                    sb.AppendLine("    Step: '" + step.Name + "' ID=" + step.Id);
                    sb.AppendLine("      Behaviors: " + step.Behaviors.Count);
                    foreach (var b in step.Behaviors)
                        sb.AppendLine("        " + b.GetType().Name + " Blocking=" + b.IsBlocking + " Stages=" + b.Stages);

                    sb.AppendLine("      Transitions: " + step.Transitions.Count);
                    foreach (var t in step.Transitions)
                    {
                        sb.AppendLine("        -> " + (t.TargetStep != null ? t.TargetStep.Name : "null (end)"));
                        sb.AppendLine("        Conditions: " + t.Conditions.Count);
                        foreach (var c in t.Conditions)
                            sb.AppendLine("          " + c.GetType().Name);
                    }
                }
            }

            // Phase 2: Check raw deserialization
            sb.AppendLine("");
            sb.AppendLine("=== Raw Deserialization Check ===");
            var binder2 = new RfeTypeBinder();
            var settings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                TypeNameHandling = TypeNameHandling.Auto,
                SerializationBinder = binder2,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                Error = (sender, args) =>
                {
                    sb.AppendLine("  DESER WARNING: " + args.ErrorContext.Path + ": " + args.ErrorContext.Error.Message);
                    args.ErrorContext.Handled = true;
                }
            };

            var wrapper = JsonConvert.DeserializeObject<JsonProcessWrapper>(json, settings);
            if (wrapper?.Steps != null)
            {
                foreach (var stepObj in wrapper.Steps)
                {
                    if (stepObj is JsonStep jStep && jStep.Data != null)
                    {
                        sb.AppendLine("Step: " + jStep.Data.Name);

                        if (jStep.Data.Behaviors?.Data?.Behaviors != null)
                        {
                            sb.AppendLine("  Raw behaviors: " + jStep.Data.Behaviors.Data.Behaviors.Count);
                            foreach (var behObj in jStep.Data.Behaviors.Data.Behaviors)
                            {
                                sb.AppendLine("    Type: " + behObj?.GetType().FullName);
                                if (behObj is JsonGenericBehavior gb)
                                {
                                    sb.AppendLine("    TypeDiscriminator: '" + gb.TypeDiscriminator + "'");
                                    sb.AppendLine("    TypeName (__type): '" + gb.TypeName + "'");
                                    sb.AppendLine("    Data null? " + (gb.Data == null));
                                    if (gb.Data != null)
                                    {
                                        sb.AppendLine("    Data entries: " + gb.Data.Count);
                                        foreach (var kvp in gb.Data)
                                        {
                                            sb.AppendLine("      [" + kvp.Key + "] = " + kvp.Value + " (" + (kvp.Value?.GetType().Name ?? "null") + ")");
                                            if (kvp.Value is System.Collections.Generic.Dictionary<string, object> nested)
                                            {
                                                foreach (var nkvp in nested)
                                                    sb.AppendLine("        [" + nkvp.Key + "] = " + nkvp.Value + " (" + (nkvp.Value?.GetType().Name ?? "null") + ")");
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (jStep.Data.Transitions?.Data?.Transitions != null)
                        {
                            foreach (var trObj in jStep.Data.Transitions.Data.Transitions)
                            {
                                if (trObj is JsonTransition jTr && jTr.Data?.Conditions != null)
                                {
                                    sb.AppendLine("  Raw conditions: " + jTr.Data.Conditions.Count);
                                    foreach (var condObj in jTr.Data.Conditions)
                                    {
                                        sb.AppendLine("    Type: " + condObj?.GetType().FullName);
                                        if (condObj is JsonGenericCondition gc)
                                        {
                                            sb.AppendLine("    TypeDiscriminator: '" + gc.TypeDiscriminator + "'");
                                            sb.AppendLine("    TypeName (__type): '" + gc.TypeName + "'");
                                            sb.AppendLine("    Data null? " + (gc.Data == null));
                                            if (gc.Data != null)
                                            {
                                                sb.AppendLine("    Data entries: " + gc.Data.Count);
                                                foreach (var kvp in gc.Data)
                                                    sb.AppendLine("      [" + kvp.Key + "] = " + kvp.Value + " (" + (kvp.Value?.GetType().Name ?? "null") + ")");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            sb.AppendLine("=== PASS ===");
        }
        catch (System.Exception ex)
        {
            sb.AppendLine("EXCEPTION: " + ex.GetType().Name + ": " + ex.Message);
            sb.AppendLine(ex.StackTrace);
            if (ex.InnerException != null)
            {
                sb.AppendLine("INNER: " + ex.InnerException.Message);
                sb.AppendLine(ex.InnerException.StackTrace);
            }
        }

        System.IO.File.WriteAllText(outputPath, sb.ToString());
        Debug.Log("[TEST] Output written to " + outputPath);
    }
}
