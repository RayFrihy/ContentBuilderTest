# Reactive Flow Engine (RFE) - Implementation Plan

## Context

**Problem:** The project needs a Unity-based workflow engine for XR/VR that supports forward, reverse, and jump-to-step execution with reactive conditions and deterministic replay. The system must parse VRBuilder-format JSON process definitions (like `BackButtonTestProcess2.json`) and execute them as a reactive, reversible, async state machine.

**Current State:** Fresh Unity 6000.2.13f1 project with no existing flow engine code, no R3/UniTask/VContainer packages installed. Only default URP setup and tutorial scripts exist.

**Target:** Production-ready engine using R3 (reactive conditions), UniTask (async execution), VContainer (DI) that runs at 90 FPS in VR.

---

## 1. Architecture Overview

```
                    +--------------------------+
                    |     Unity Scene Root      |
                    |  (RfeLifetimeScope : VCA) |
                    +-----------+--------------+
                                |
                    +-----------v--------------+
                    |    ProcessRunner (mono)   |  <-- Entry point
                    +-----------+--------------+
                                |
               +----------------+----------------+
               |                                 |
  +------------v-----------+       +-------------v-----------+
  |   IProcessLoader       |       |   IFlowEngine           |
  |  (JSON parse & build)  |       |  (orchestrates exec)    |
  +------------+-----------+       +-------------+-----------+
               |                                 |
    +----------v----------+         +------------v-----------+
    | ProcessModel (POCO) |         |    IStepRunner          |
    +---------------------+         +------------+-----------+
                                                 |
                              +------------------+------------------+
                              |                  |                  |
                  +-----------v---+   +----------v------+   +------v--------+
                  |IBehaviorRunner|   |ITransitionEval  |   |IStateStore    |
                  |(exec + undo)  |   |(R3 conditions)  |   |(snapshots)    |
                  +---------------+   +-----------------+   +---------------+
```

### Module Dependency Graph

```
[Abstractions] <--- no dependencies, defines all interfaces
     ^
     |--- [Model]         <--- POCOs for Process/Chapter/Step/etc.
     |       ^
     |       |--- [Serialization] <--- JSON loading, $id/$ref resolution
     |
     |--- [Conditions]    <--- R3 condition implementations
     |--- [Behaviors]     <--- Behavior implementations (reversible)
     |--- [State]         <--- Snapshot store
     |--- [Engine]        <--- StepRunner, FlowEngine, ChapterRunner
     |       ^
     |       |--- [Navigation] <--- Next/Prev/Jump/GoTo
     |
     |--- [DI]            <--- VContainer registrations
     |--- [Runtime]       <--- MonoBehaviours, scene entry points
```

### Key Design Principles

1. **POCOs for data model** -- The JSON maps to plain C# objects with no Unity dependencies. This allows headless testing and clean serialization.
2. **Interfaces everywhere** -- Every module communicates through abstractions. Engine never references concrete conditions/behaviors.
3. **R3 for push-based condition evaluation** -- Conditions emit `Observable<bool>`. Transitions subscribe. First transition whose all conditions go true wins.
4. **UniTask for async behavior execution** -- Every behavior is `async UniTask`. Cancellation flows through `CancellationToken` tied to step lifecycle.
5. **VContainer for lifetime scoping** -- Process-level container is parent. Chapter and Step get child scopes created/destroyed during execution.
6. **Snapshot before transition** -- State is captured after a step's behaviors complete but before transitioning. Reverse restores the previous snapshot.

---

## 2. Folder Structure

```
Assets/
  ReactiveFlowEngine/
    ReactiveFlowEngine.asmdef
    Abstractions/
      IProcess.cs, IChapter.cs, IStep.cs, ITransition.cs
      ICondition.cs, IBehavior.cs, IReversibleBehavior.cs
      IFlowEngine.cs, IStepRunner.cs, ITransitionEvaluator.cs
      INavigationService.cs, IStateStore.cs, IProcessLoader.cs
      ISceneObjectResolver.cs
      ExecutionDirection.cs, StepStatus.cs, NavigationRequest.cs
    Model/
      ProcessModel.cs, ChapterModel.cs, StepModel.cs, TransitionModel.cs
      ConditionDefinition.cs, BehaviorDefinition.cs
      StepSnapshot.cs, SceneObjectRef.cs, AnimationCurveData.cs
    Serialization/
      VRBuilderJsonLoader.cs        (Newtonsoft $id/$ref resolver)
      JsonReferenceResolver.cs
      RfeTypeBinder.cs              ($type -> DTO class mapping)
      JsonModels/                   (raw DTOs matching JSON shape)
        JsonProcessWrapper.cs, JsonStep.cs, JsonChapter.cs, etc.
      ModelBuilder.cs               (DTO -> domain model transform)
      ValidationResult.cs
    Conditions/
      TimeoutCondition.cs, CompositeAndCondition.cs
      CompositeOrCondition.cs, CompositeNotCondition.cs
      ConditionFactory.cs
    Behaviors/
      MoveObjectBehavior.cs, ExecuteChapterBehavior.cs
      DelayBehavior.cs, BehaviorFactory.cs
    State/
      StateStore.cs, SnapshotSerializer.cs
    Engine/
      FlowEngine.cs, StepRunner.cs
      TransitionEvaluator.cs, ChapterRunner.cs
    Navigation/
      NavigationService.cs, NavigationCommand.cs, HistoryStack.cs
    DI/
      RfeLifetimeScope.cs, ProcessLifetimeScope.cs
      ConditionInstaller.cs, BehaviorInstaller.cs
    Runtime/
      ProcessRunner.cs, SceneObjectResolver.cs, RfeDebugUI.cs
  ReactiveFlowEngine.Tests/
    ReactiveFlowEngine.Tests.asmdef
    Engine/, Serialization/, Navigation/, State/
```

---

## 3. Core Interfaces

### 3.1 Process Graph Interfaces

```csharp
// ---- Abstractions/IProcess.cs ----
public interface IProcess
{
    string Id { get; }
    string Name { get; }
    IReadOnlyList<IChapter> Chapters { get; }
    IChapter FirstChapter { get; }
}

// ---- Abstractions/IChapter.cs ----
public interface IChapter
{
    string Id { get; }
    string Name { get; }
    IStep FirstStep { get; }
    IReadOnlyList<IStep> Steps { get; }
    IReadOnlyList<IChapter> SubChapters { get; }  // for step groups
}

// ---- Abstractions/IStep.cs ----
public interface IStep
{
    string Id { get; }               // GUID from JSON
    string Name { get; }
    StepType Type { get; }           // Default, StepGroup
    IReadOnlyList<IBehavior> Behaviors { get; }
    IReadOnlyList<ITransition> Transitions { get; }
    IChapter SubChapter { get; }     // non-null for StepGroup type
}

public enum StepType { Default, StepGroup }

// ---- Abstractions/ITransition.cs ----
public interface ITransition
{
    int Priority { get; }            // index in the transition list (lower = higher priority)
    IReadOnlyList<ICondition> Conditions { get; }
    IStep TargetStep { get; }        // null = end of chapter
    bool IsUnconditional { get; }    // true when Conditions is empty
}
```

### 3.2 Condition Interface (R3-based)

```csharp
// ---- Abstractions/ICondition.cs ----
public interface ICondition : IDisposable
{
    /// Returns an Observable that emits true when condition is satisfied,
    /// false when it becomes unsatisfied. Completes when permanently satisfied.
    Observable<bool> Evaluate();

    /// Resets internal state for reuse (e.g., timer restart).
    void Reset();
}
```

### 3.3 Behavior Interfaces (UniTask-based)

```csharp
// ---- Abstractions/IBehavior.cs ----
public interface IBehavior
{
    /// Execution stage flags (bitmask): Activation=1, Deactivation=2, Both=3
    ExecutionStages Stages { get; }
    bool IsBlocking { get; }
    UniTask ExecuteAsync(CancellationToken ct);
}

[Flags]
public enum ExecutionStages { None = 0, Activation = 1, Deactivation = 2 }

// ---- Abstractions/IReversibleBehavior.cs ----
public interface IReversibleBehavior : IBehavior
{
    /// Undoes the effect of ExecuteAsync. Called during reverse navigation.
    UniTask UndoAsync(CancellationToken ct);
}
```

### 3.4 Engine Interfaces

```csharp
// ---- Abstractions/IFlowEngine.cs ----
public interface IFlowEngine
{
    /// Current execution state as R3 observable for UI binding.
    ReadOnlyReactiveProperty<EngineState> State { get; }
    ReadOnlyReactiveProperty<IStep> CurrentStep { get; }
    ReadOnlyReactiveProperty<IChapter> CurrentChapter { get; }

    UniTask StartProcessAsync(IProcess process, CancellationToken ct);
    UniTask StopAsync();
}

public enum EngineState { Idle, Running, Paused, Transitioning, Completed }

// ---- Abstractions/IStepRunner.cs ----
public interface IStepRunner
{
    /// Executes a single step: enter -> execute behaviors -> evaluate transitions.
    /// Returns the transition that fired (or null if cancelled).
    UniTask<ITransition> RunStepAsync(IStep step, CancellationToken ct);

    /// Cancels the currently executing step mid-flight.
    void CancelCurrentStep();
}

// ---- Abstractions/ITransitionEvaluator.cs ----
public interface ITransitionEvaluator
{
    /// Subscribes to all transitions on a step. Returns an Observable that emits
    /// the first transition whose conditions all become true.
    /// Respects priority ordering (first in list = highest priority).
    Observable<ITransition> Evaluate(IReadOnlyList<ITransition> transitions);
}
```

### 3.5 Navigation Interface

```csharp
// ---- Abstractions/INavigationService.cs ----
public interface INavigationService
{
    /// Observable of navigation events for UI/logging.
    Observable<NavigationEvent> OnNavigated { get; }

    UniTask NextStepAsync(CancellationToken ct);
    UniTask PreviousStepAsync(CancellationToken ct);
    UniTask GoToStepAsync(string stepId, CancellationToken ct);
    UniTask JumpToChapterAsync(string chapterId, CancellationToken ct);
    UniTask RestartStepAsync(CancellationToken ct);

    bool CanGoBack { get; }
    bool CanGoForward { get; }
}

public record NavigationEvent(
    NavigationType Type,
    string FromStepId,
    string ToStepId,
    DateTimeOffset Timestamp
);

public enum NavigationType { Forward, Reverse, Jump, Restart }
```

### 3.6 State Interface

```csharp
// ---- Abstractions/IStateStore.cs ----
public interface IStateStore
{
    /// Captures current state for a step. Called after behaviors complete.
    StepSnapshot CaptureSnapshot(IStep step);

    /// Restores a previously captured snapshot.
    UniTask RestoreSnapshotAsync(StepSnapshot snapshot, CancellationToken ct);

    /// Returns snapshot for a given step, or null if none exists.
    StepSnapshot GetSnapshot(string stepId);

    /// Returns ordered history of visited step IDs.
    IReadOnlyList<string> GetHistory();

    /// Pushes a step ID onto the visited history.
    void PushHistory(string stepId);

    /// Pops the most recent step ID from history.
    string PopHistory();

    void Clear();
}

// ---- Model/StepSnapshot.cs ----
public sealed class StepSnapshot
{
    public string StepId { get; init; }
    public string ChapterId { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    public Dictionary<string, object> State { get; init; }  // serializable state bag
    public List<BehaviorSnapshot> BehaviorStates { get; init; }
}

public sealed class BehaviorSnapshot
{
    public string BehaviorType { get; init; }
    public Dictionary<string, object> Data { get; init; }
}
```

### 3.7 Serialization Interfaces

```csharp
// ---- Abstractions/IProcessLoader.cs ----
public interface IProcessLoader
{
    /// Loads and validates a process from a JSON string. Returns a fully linked IProcess.
    UniTask<IProcess> LoadAsync(string json, CancellationToken ct);
}

// ---- Abstractions/ISceneObjectResolver.cs ----
public interface ISceneObjectResolver
{
    /// Resolves a scene object GUID to the corresponding GameObject/Transform.
    /// Returns null if not found.
    UnityEngine.Transform Resolve(string guid);
}
```

---

## 4. JSON Mapping Strategy

### 4.1 The Challenge: `$id`/`$ref` Reference System

The VRBuilder JSON uses Newtonsoft.Json's `PreserveReferencesHandling` feature. Each object has a unique `$id` integer string. Later references use `$ref` pointing back to that ID. The `$type` field encodes the concrete class plus assembly (assembly name is always empty in this export, e.g. `"Step, "`).

### 4.2 Two-Phase Parse Architecture

**Phase 1: Raw Deserialization (JSON DTOs)**

Use `Newtonsoft.Json` with `PreserveReferencesHandling.All` and `TypeNameHandling.Auto` configured with a custom `SerializationBinder` that maps VRBuilder type names to our DTO classes.

```
JSON string
    |
    v
Newtonsoft.Json.JsonConvert.DeserializeObject<JsonProcessWrapper>(json, settings)
    |
    |  settings:
    |    PreserveReferencesHandling = All   (resolves $id/$ref automatically)
    |    TypeNameHandling = Auto            (resolves $type to DTOs)
    |    SerializationBinder = RfeTypeBinder (maps "Step, " -> JsonStep, etc.)
    |
    v
JsonProcessWrapper (fully linked DTO graph, all $ref resolved)
```

**Phase 2: Model Building (DTO -> Domain)**

```
JsonProcessWrapper
    |
    v
ModelBuilder.Build(wrapper)
    |
    |  1. Build step lookup:  Dictionary<string, StepModel> by Guid
    |  2. Build chapters:     ChapterModel with step references resolved
    |  3. Build transitions:  Link TargetStep StepRef.Guid -> StepModel
    |  4. Build behaviors:    BehaviorFactory maps "$type" -> IBehavior ctor
    |  5. Build conditions:   ConditionFactory maps "$type" -> ICondition ctor
    |  6. Validate:           All StepRef GUIDs have matching Step, no orphans
    |
    v
ProcessModel (fully linked domain graph)
```

### 4.3 Type Mapping Table

| JSON `$type`              | JSON DTO Class          | Domain Model / Runtime Class      |
|--------------------------|------------------------|-----------------------------------|
| `Serializer_ProcessWrapper, ` | `JsonProcessWrapper`   | (entry point, not a domain type)  |
| `Process, `              | `JsonProcess`           | `ProcessModel`                    |
| `ProcessData, `          | `JsonProcessData`       | (merged into ProcessModel)        |
| `Chapter, `              | `JsonChapter`           | `ChapterModel`                    |
| `ChapterData, `          | `JsonChapterData`       | (merged into ChapterModel)        |
| `Step, `                 | `JsonStep`              | `StepModel`                       |
| `StepData, `             | `JsonStepData`          | (merged into StepModel)           |
| `StepRef, `              | `JsonStepRef`           | (resolved to StepModel reference) |
| `StepMetadata, `         | `JsonStepMetadata`      | (merged into StepModel)           |
| `Transition, `           | `JsonTransition`        | `TransitionModel`                 |
| `TransitionData, `       | `JsonTransitionData`    | (merged into TransitionModel)     |
| `TimeoutCondition, `     | `JsonTimeoutCondition`  | `TimeoutCondition` (runtime)      |
| `MoveObjectBehavior, `   | `JsonMoveObjectBehavior`| `MoveObjectBehavior` (runtime)    |
| `ExecuteChapterBehavior, ` | `JsonExecChapterBehavior`| `ExecuteChapterBehavior` (runtime)|
| `SingleSceneObjectReference, ` | `JsonSceneObjRef`  | `SceneObjectRef`                  |
| `BehaviorCollection, `   | (structural, unwrapped) | --                                |
| `TransitionCollection, ` | (structural, unwrapped) | --                                |

### 4.4 Custom SerializationBinder

```csharp
public class RfeTypeBinder : ISerializationBinder
{
    private readonly Dictionary<string, Type> _typeMap = new()
    {
        ["Serializer_ProcessWrapper"] = typeof(JsonProcessWrapper),
        ["Process"] = typeof(JsonProcess),
        ["ProcessData"] = typeof(JsonProcessData),
        ["Chapter"] = typeof(JsonChapter),
        ["ChapterData"] = typeof(JsonChapterData),
        ["Step"] = typeof(JsonStep),
        ["StepRef"] = typeof(JsonStepRef),
        ["StepData"] = typeof(JsonStepData),
        ["StepMetadata"] = typeof(JsonStepMetadata),
        ["Transition"] = typeof(JsonTransition),
        ["TransitionData"] = typeof(JsonTransitionData),
        ["TransitionCollection"] = typeof(JsonTransitionCollection),
        ["BehaviorCollection"] = typeof(JsonBehaviorCollection),
        ["TimeoutCondition"] = typeof(JsonTimeoutCondition),
        ["TimeoutConditionData"] = typeof(JsonTimeoutConditionData),
        ["MoveObjectBehavior"] = typeof(JsonMoveObjectBehavior),
        ["MoveObjectBehaviorData"] = typeof(JsonMoveObjectBehaviorData),
        ["ExecuteChapterBehavior"] = typeof(JsonExecuteChapterBehavior),
        ["ExecuteChapterBehaviorData"] = typeof(JsonExecuteChapterBehaviorData),
        // ... collections, metadata, etc. mapped as needed
    };

    public Type BindToType(string assemblyName, string typeName)
    {
        // JSON "$type" is "TypeName, " -- strip trailing comma-space
        var cleanName = typeName.Trim().TrimEnd(',').Trim();
        return _typeMap.GetValueOrDefault(cleanName);
    }
}
```

### 4.5 Validation Rules

After Phase 2, `ModelBuilder` runs these validation checks:

1. Every `StepRef.Guid` in transitions resolves to a step in the flat step list.
2. Every chapter has a non-null `FirstStep` reference.
3. No cycles that would cause infinite forward execution without a condition gating reentry.
4. All behavior `$type` values are recognized by `BehaviorFactory`.
5. All condition `$type` values are recognized by `ConditionFactory`.
6. Scene object GUIDs are collected and reported (resolved at runtime, not parse time).

Validation returns a `ValidationResult` with errors and warnings. Errors halt loading. Warnings are logged.

### 4.6 Handling the Flat Steps List vs. Chapter Steps

The JSON has two step containers:
- `wrapper.Steps.$values[]` -- flat list of ALL steps across all chapters (Steps 1-6)
- `chapter.Data.Steps.$values[]` -- per-chapter step references (using `$ref` to point into the flat list)

Strategy: Build a master `Dictionary<string, StepModel>` from the flat list first. Then when building chapters, resolve the `$ref`-based step references into the same model instances.

### 4.7 Handling StepRef vs Step

`StepRef` objects appear in transitions as `TargetStep` and in chapters as `FirstStep`. They carry only a `StepMetadata.Guid`. They are resolved into the actual `StepModel` by GUID lookup during `ModelBuilder.Build()`.

---

## 5. Execution Model

### 5.1 Forward Execution Flow

```
         +-----------------------------------------------------+
         |               FlowEngine.RunLoop                     |
         |  (async loop, one iteration per step)                |
         +-----------------------+-----------------------------+
                                 |
    +----------------------------v-------------------------------+
    |  1. ENTER STEP                                            |
    |     stepRunner.RunStepAsync(currentStep, ct)               |
    |       a. Set status = Entering                            |
    |       b. Fire OnStepEntered event                         |
    |       c. Create per-step CancellationTokenSource           |
    +----------------------------+-------------------------------+
                                 |
    +----------------------------v-------------------------------+
    |  2. EXECUTE BEHAVIORS                                      |
    |     For each behavior where Stages.HasFlag(Activation):    |
    |       if (IsBlocking)                                      |
    |         await behavior.ExecuteAsync(stepCt)                |
    |       else                                                 |
    |         _ = behavior.ExecuteAsync(stepCt)   // fire&forget |
    |     Set status = Executing -> Evaluating                   |
    +----------------------------+-------------------------------+
                                 |
    +----------------------------v-------------------------------+
    |  3. EVALUATE TRANSITIONS                                   |
    |     transitionEvaluator.Evaluate(step.Transitions)         |
    |       - Subscribes to all conditions on all transitions    |
    |       - For unconditional transitions: fires immediately   |
    |       - For conditional: monitors R3 observables           |
    |       - First transition to satisfy ALL conditions wins    |
    |       - Priority: index order (transition[0] > [1])        |
    |     ITransition winner = await observable.FirstAsync()      |
    +----------------------------+-------------------------------+
                                 |
    +----------------------------v-------------------------------+
    |  4. CAPTURE SNAPSHOT                                       |
    |     stateStore.CaptureSnapshot(currentStep)                |
    |     stateStore.PushHistory(currentStep.Id)                 |
    +----------------------------+-------------------------------+
                                 |
    +----------------------------v-------------------------------+
    |  5. TRANSITION                                             |
    |     if (winner.TargetStep == null)                          |
    |       -> End of chapter. Move to next chapter or end.      |
    |     else                                                    |
    |       -> currentStep = winner.TargetStep                   |
    |       -> loop back to step 1                                |
    +------------------------------------------------------------+
```

### 5.2 StepGroup/SubChapter Execution

When a step has `StepType == StepGroup`, one of its behaviors is `ExecuteChapterBehavior`. This behavior:

1. Retrieves the referenced sub-chapter from the model.
2. Creates a child `ChapterRunner` (which creates its own step execution loop).
3. `await chapterRunner.RunAsync(subChapter, ct)` -- this runs the sub-chapter's steps (Step 4 -> Step 5 -> Step 6 -> end).
4. When the sub-chapter completes (Step 6 transitions to null target), `ExecuteChapterBehavior.ExecuteAsync` returns.
5. The parent step's transitions then evaluate (since the behavior was blocking).
6. Parent transitions fire and the parent step transitions to its target (end of parent chapter, in the sample).

This creates a stack-like execution model: parent step is suspended while sub-chapter runs.

### 5.3 Reverse Execution Flow

```
PreviousStepAsync(ct):
  1. Cancel current step (stepRunner.CancelCurrentStep())
     - Fires per-step CancellationTokenSource
     - Disposes condition subscriptions
     - Cancels in-flight behavior tasks

  2. Undo current step's behaviors (reverse order)
     For each behavior in step.Behaviors.Reverse():
       if (behavior is IReversibleBehavior reversible)
         await reversible.UndoAsync(ct)

  3. Pop history
     string previousStepId = stateStore.PopHistory()

  4. Restore snapshot
     StepSnapshot snapshot = stateStore.GetSnapshot(previousStepId)
     await stateStore.RestoreSnapshotAsync(snapshot, ct)

  5. Resume at previous step
     currentStep = ResolveStep(previousStepId)
     -> re-enter the normal forward execution loop from step 1
```

### 5.4 Jump (GoTo) Execution Flow

```
GoToStepAsync(targetStepId, ct):
  1. Cancel current step (same as reverse step 1)

  2. Determine path: Is target ahead or behind?
     a. If target is in history (behind):
        - Iteratively undo steps from current back to target
        - For each step in reverse order:
          * Undo behaviors
          * Restore snapshot
        - Resume at target step

     b. If target is ahead (not in history):
        - Cancel current step
        - Snapshot current state
        - Jump directly to target step
        - Enter forward execution from target
        - NOTE: intermediate steps are SKIPPED (not executed)

     c. If target is in a different chapter:
        - Cancel current step
        - End current chapter
        - Find target chapter
        - Start target chapter from target step (if it's the first step)
          OR fast-forward to target step within that chapter
```

### 5.5 Restart Step Flow

```
RestartStepAsync(ct):
  1. Cancel current step
  2. Undo current step's behaviors (reverse order)
  3. Restore current step's entry snapshot (the snapshot captured by the PREVIOUS step)
  4. Re-enter current step from the beginning
```

---

## 6. State Management

### 6.1 What Gets Stored in a StepSnapshot

```csharp
StepSnapshot {
    StepId:       "a998c040-a3e2-4e6b-abbf-6e592ecf4cf1"
    ChapterId:    "4a1362b1-3c06-4031-80a4-83e86ab1fc82"
    Timestamp:    2026-03-19T10:30:00Z
    State: {
        // Per-behavior state for reversal
        "MoveObject_db2c9e6d": {
            "OriginalPosition": Vector3(0, 0, 0),
            "OriginalRotation": Quaternion(0, 0, 0, 1),
            "TargetPosition": Vector3(1, 2, 3)
        },
        // Global flow variables
        "rfe.stepIndex": 0,
        "rfe.chapterIndex": 0,
        // User-defined state variables
        "user.score": 100,
        "user.hasKey": true
    },
    BehaviorStates: [
        {
            BehaviorType: "MoveObjectBehavior",
            Data: { "StartPos": "(0,0,0)", "EndPos": "(1,2,3)", "Duration": 4.0 }
        }
    ]
}
```

### 6.2 Snapshot Lifecycle

```
Step N-1 completes
    |
    v
CaptureSnapshot(Step N-1)  -->  stored in Dictionary<string, StepSnapshot>
PushHistory("step-N-1-guid")
    |
    v
Step N begins executing
    |
    v
[If user hits "Back"]:
    PopHistory() -> "step-N-1-guid"
    GetSnapshot("step-N-1-guid") -> StepSnapshot
    RestoreSnapshotAsync(snapshot)
    Resume at Step N-1
```

### 6.3 StateStore Implementation Strategy

```csharp
public class StateStore : IStateStore
{
    // Step GUID -> most recent snapshot for that step
    private readonly Dictionary<string, StepSnapshot> _snapshots = new();

    // Ordered history of visited step IDs (stack behavior)
    private readonly List<string> _history = new();

    // Global state variables (shared across steps)
    private readonly Dictionary<string, object> _globalState = new();

    public StepSnapshot CaptureSnapshot(IStep step)
    {
        var snapshot = new StepSnapshot
        {
            StepId = step.Id,
            Timestamp = DateTimeOffset.UtcNow,
            State = new Dictionary<string, object>(_globalState),  // deep copy
            BehaviorStates = CaptureBehaviorStates(step)
        };
        _snapshots[step.Id] = snapshot;
        return snapshot;
    }

    // ... RestoreSnapshotAsync applies the State dict back to _globalState
    //     and calls each behavior's restore logic
}
```

### 6.4 Behavior State Capture

Each `IReversibleBehavior` implements a `CaptureState() -> Dictionary<string, object>` method that records what it needs for `UndoAsync`. For `MoveObjectBehavior`, this is the object's position/rotation before the move began.

The `StateStore` iterates all behaviors on a step and collects their state into `BehaviorStates`.

### 6.5 Memory Strategy

- **Sliding window pruning:** Keep only the last N snapshots (configurable, default 50). Older snapshots are discarded.
- **Structural sharing:** Snapshots only store changed state. Use copy-on-write for the global state dictionary.
- **Lazy behavior state:** Behaviors only store what they need for undo (e.g., one Vector3, not entire scene state).

---

## 7. Reactive Design (R3)

### 7.1 Condition Signal Pattern

Each condition returns an `Observable<bool>`. The contract is:
- Emits `false` initially (condition not yet met)
- Emits `true` when satisfied
- May toggle back to `false` if the condition becomes unsatisfied (e.g., spatial conditions)
- Completes when the condition is permanently satisfied (optimization to release resources)

### 7.2 TimeoutCondition Implementation

```csharp
public class TimeoutCondition : ICondition
{
    private readonly float _timeout;
    private CompositeDisposable _disposables;

    public TimeoutCondition(float timeout) => _timeout = timeout;

    public Observable<bool> Evaluate()
    {
        // Emit false immediately, then true after timeout seconds
        return Observable.Timer(TimeSpan.FromSeconds(_timeout))
            .Select(_ => true)
            .Prepend(false);
    }

    public void Reset() { /* stateless, timer restarts on new subscription */ }
    public void Dispose() => _disposables?.Dispose();
}
```

### 7.3 TransitionEvaluator: Merging Multiple Transitions

```csharp
public class TransitionEvaluator : ITransitionEvaluator
{
    public Observable<ITransition> Evaluate(IReadOnlyList<ITransition> transitions)
    {
        // For each transition, create an Observable that emits the transition
        // when ALL its conditions are true.
        var transitionStreams = transitions.Select((transition, index) =>
        {
            if (transition.IsUnconditional)
            {
                // Unconditional = immediate fire
                return Observable.Return(transition);
            }

            // CombineLatest all conditions: all must be true simultaneously
            var conditionStreams = transition.Conditions
                .Select(c => c.Evaluate())
                .ToArray();

            return Observable.CombineLatest(conditionStreams)
                .Select(values => values.All(v => v))  // AND logic
                .DistinctUntilChanged()
                .Where(allTrue => allTrue)
                .Select(_ => transition);
        });

        // Merge all, Take(1) -- first to fire wins.
        // Priority is naturally handled because unconditional transitions
        // at lower indices fire synchronously before timers.
        return Observable.Merge(transitionStreams.ToArray())
            .Take(1);  // first to fire wins
    }
}
```

### 7.4 Priority Resolution Detail

When Step 2 has two transitions:
- Transition[0]: TimeoutCondition(2s) -> Step 3
- Transition[1]: TimeoutCondition(1s) -> Step Group

Transition[1] fires after 1 second. Transition[0] would fire after 2 seconds. `Observable.Merge + Take(1)` naturally picks Transition[1] because it emits first chronologically.

If two transitions fire in the exact same frame (e.g., two conditions satisfied simultaneously), priority by index must be enforced. Strategy:

```csharp
// Collect all transitions that fire within the current frame
// Use ObserveOnMainThread + batch by frame
return Observable.Merge(transitionStreams)
    .ThrottleFirstFrame(1)  // debounce to one frame
    .Select(transition => transition);  // pick lowest index if multiple
```

In practice, truly simultaneous firing is rare. The `Merge + Take(1)` approach handles 99% of cases correctly. Frame-buffering can be added in Phase 6 (optimization).

### 7.5 Composite Conditions (AND/OR/NOT)

```csharp
public class CompositeAndCondition : ICondition
{
    private readonly ICondition[] _children;

    public Observable<bool> Evaluate()
    {
        return Observable.CombineLatest(
            _children.Select(c => c.Evaluate()).ToArray()
        ).Select(values => values.All(v => v));
    }
}

public class CompositeOrCondition : ICondition
{
    private readonly ICondition[] _children;

    public Observable<bool> Evaluate()
    {
        return Observable.CombineLatest(
            _children.Select(c => c.Evaluate()).ToArray()
        ).Select(values => values.Any(v => v));
    }
}

public class CompositeNotCondition : ICondition
{
    private readonly ICondition _inner;

    public Observable<bool> Evaluate()
    {
        return _inner.Evaluate().Select(v => !v);
    }
}
```

### 7.6 Condition Disposal Pattern

When a step transitions out (forward, reverse, or cancelled), all condition subscriptions must be disposed:

```csharp
// In StepRunner:
private CompositeDisposable _conditionSubscriptions;

async UniTask<ITransition> RunStepAsync(IStep step, CancellationToken ct)
{
    _conditionSubscriptions = new CompositeDisposable();

    // ... execute behaviors ...

    var transitionObservable = _transitionEvaluator.Evaluate(step.Transitions);

    ITransition winner = null;
    transitionObservable
        .Subscribe(t => winner = t)
        .AddTo(_conditionSubscriptions);

    // Wait for transition or cancellation
    await UniTask.WaitUntil(() => winner != null || ct.IsCancellationRequested);

    // Cleanup
    _conditionSubscriptions.Dispose();
    foreach (var t in step.Transitions)
        foreach (var c in t.Conditions)
            c.Dispose();

    return winner;
}
```

---

## 8. Async Design (UniTask)

### 8.1 UniTask Pipeline in StepRunner

```csharp
public class StepRunner : IStepRunner
{
    private CancellationTokenSource _stepCts;

    public async UniTask<ITransition> RunStepAsync(IStep step, CancellationToken ct)
    {
        _stepCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var stepCt = _stepCts.Token;

        try
        {
            // Phase 1: Execute blocking behaviors sequentially
            var blockingBehaviors = step.Behaviors
                .Where(b => b.IsBlocking && b.Stages.HasFlag(ExecutionStages.Activation))
                .ToList();

            var nonBlockingBehaviors = step.Behaviors
                .Where(b => !b.IsBlocking && b.Stages.HasFlag(ExecutionStages.Activation))
                .ToList();

            // Fire non-blocking first (fire-and-forget with token)
            var nonBlockingTasks = nonBlockingBehaviors
                .Select(b => b.ExecuteAsync(stepCt))
                .ToList();

            // Execute blocking in sequence
            foreach (var behavior in blockingBehaviors)
            {
                await behavior.ExecuteAsync(stepCt);
            }

            // Phase 2: Evaluate transitions (reactive, waits for condition)
            ITransition winner = await EvaluateTransitionsAsync(step.Transitions, stepCt);

            // Phase 3: Cancel non-blocking on transition
            _stepCts.Cancel();
            await UniTask.WhenAll(nonBlockingTasks
                .Select(t => t.SuppressCancellationThrow()));

            return winner;
        }
        catch (OperationCanceledException)
        {
            return null;  // step was cancelled externally
        }
    }

    public void CancelCurrentStep()
    {
        _stepCts?.Cancel();
        _stepCts?.Dispose();
    }
}
```

### 8.2 MoveObjectBehavior with Cancellation

```csharp
public class MoveObjectBehavior : IReversibleBehavior
{
    private readonly ISceneObjectResolver _resolver;
    private readonly string _targetGuid;
    private readonly string _destinationGuid;
    private readonly float _duration;
    private readonly AnimationCurve _curve;

    private Vector3 _originalPosition;
    private Quaternion _originalRotation;

    public ExecutionStages Stages => ExecutionStages.Activation;
    public bool IsBlocking => true;

    public async UniTask ExecuteAsync(CancellationToken ct)
    {
        var target = _resolver.Resolve(_targetGuid);
        var destination = _resolver.Resolve(_destinationGuid);
        if (target == null || destination == null) return;

        _originalPosition = target.position;
        _originalRotation = target.rotation;

        var startPos = target.position;
        var endPos = destination.position;
        var startRot = target.rotation;
        var endRot = destination.rotation;
        float elapsed = 0f;

        while (elapsed < _duration)
        {
            ct.ThrowIfCancellationRequested();

            elapsed += Time.deltaTime;
            float t = _curve.Evaluate(Mathf.Clamp01(elapsed / _duration));
            target.position = Vector3.Lerp(startPos, endPos, t);
            target.rotation = Quaternion.Slerp(startRot, endRot, t);

            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }

        target.position = endPos;
        target.rotation = endRot;
    }

    public async UniTask UndoAsync(CancellationToken ct)
    {
        var target = _resolver.Resolve(_targetGuid);
        if (target == null) return;

        // Instant snap-back (or could animate in reverse)
        target.position = _originalPosition;
        target.rotation = _originalRotation;
        await UniTask.CompletedTask;
    }
}
```

### 8.3 ExecuteChapterBehavior (Sub-Chapter Execution)

```csharp
public class ExecuteChapterBehavior : IReversibleBehavior
{
    private readonly IChapter _subChapter;
    private readonly Func<IChapter, CancellationToken, UniTask> _chapterRunner;

    public ExecutionStages Stages => ExecutionStages.Activation;
    public bool IsBlocking => true;  // parent step waits for sub-chapter to complete

    public async UniTask ExecuteAsync(CancellationToken ct)
    {
        // Delegate to ChapterRunner, which runs the sub-chapter's step loop
        await _chapterRunner(_subChapter, ct);
    }

    public async UniTask UndoAsync(CancellationToken ct)
    {
        // Undo all steps in the sub-chapter in reverse order
        // This requires access to the state store and the sub-chapter's history
        // Implementation delegates to NavigationService reverse logic
    }
}
```

### 8.4 Cancellation Token Hierarchy

```
Application-level CancellationToken (from ProcessRunner OnDestroy)
    |
    +-- Process-level CTS (created by FlowEngine.StartProcessAsync)
        |
        +-- Chapter-level CTS (created by ChapterRunner)
            |
            +-- Step-level CTS (created by StepRunner.RunStepAsync)
                |
                +-- Individual behavior tasks inherit step CTS
                +-- Condition subscriptions checked against step CTS
```

When user navigates backward, only the step-level CTS is cancelled. When jumping chapters, the chapter-level CTS is cancelled. When stopping the process, the process-level CTS cancels everything.

### 8.5 UniTask Integration Points

- `UniTask.Yield(PlayerLoopTiming.Update)` -- for per-frame behavior updates (MoveObject lerp)
- `UniTask.Delay(TimeSpan, cancellationToken: ct)` -- for delay behaviors
- `UniTask.WhenAll(tasks)` -- for parallel non-blocking behaviors
- `UniTask.WaitUntil(() => condition, cancellationToken: ct)` -- for bridging R3 -> UniTask (transition waiting)
- `UniTask.SuppressCancellationThrow()` -- for graceful cancellation of fire-and-forget tasks

---

## 9. Dependency Injection Plan (VContainer)

### 9.1 Root Lifetime Scope

```csharp
public class RfeLifetimeScope : LifetimeScope
{
    [SerializeField] private TextAsset _processJson;  // drag JSON into inspector

    protected override void Configure(IContainerBuilder builder)
    {
        // ---- Singletons (live for entire application) ----

        // Core engine
        builder.Register<IFlowEngine, FlowEngine>(Lifetime.Singleton);
        builder.Register<IStepRunner, StepRunner>(Lifetime.Singleton);
        builder.Register<ITransitionEvaluator, TransitionEvaluator>(Lifetime.Singleton);
        builder.Register<INavigationService, NavigationService>(Lifetime.Singleton);

        // State
        builder.Register<IStateStore, StateStore>(Lifetime.Singleton);

        // Serialization
        builder.Register<IProcessLoader, VRBuilderJsonLoader>(Lifetime.Singleton);
        builder.Register<ModelBuilder>(Lifetime.Singleton);
        builder.Register<RfeTypeBinder>(Lifetime.Singleton);

        // Factories (singleton factories that create transient instances)
        builder.Register<ConditionFactory>(Lifetime.Singleton);
        builder.Register<BehaviorFactory>(Lifetime.Singleton);

        // Scene integration
        builder.Register<ISceneObjectResolver, SceneObjectResolver>(Lifetime.Singleton);

        // ---- Configuration ----
        if (_processJson != null)
            builder.RegisterInstance(_processJson).As<TextAsset>();

        // ---- Entry point ----
        builder.RegisterEntryPoint<ProcessRunner>();
    }
}
```

### 9.2 Factory Registration Pattern

```csharp
public class ConditionFactory
{
    private readonly IObjectResolver _resolver;

    public ConditionFactory(IObjectResolver resolver) => _resolver = resolver;

    public ICondition Create(ConditionDefinition definition)
    {
        return definition.TypeName switch
        {
            "TimeoutCondition" => new TimeoutCondition(definition.GetFloat("Timeout")),
            "InteractionCondition" => _resolver.Resolve<InteractionCondition>(),
            "SpatialZoneCondition" => _resolver.Resolve<SpatialZoneCondition>(),
            _ => throw new ArgumentException($"Unknown condition type: {definition.TypeName}")
        };
    }
}

public class BehaviorFactory
{
    private readonly IObjectResolver _resolver;
    private readonly ISceneObjectResolver _sceneResolver;

    public BehaviorFactory(IObjectResolver resolver, ISceneObjectResolver sceneResolver)
    {
        _resolver = resolver;
        _sceneResolver = sceneResolver;
    }

    public IBehavior Create(BehaviorDefinition definition)
    {
        return definition.TypeName switch
        {
            "MoveObjectBehavior" => new MoveObjectBehavior(
                _sceneResolver,
                definition.GetString("TargetObject"),
                definition.GetString("FinalPosition"),
                definition.GetFloat("Duration"),
                definition.GetAnimationCurve("AnimationCurve")
            ),
            "ExecuteChapterBehavior" => new ExecuteChapterBehavior(
                definition.GetChapter("Chapter"),
                (chapter, ct) => _resolver.Resolve<ChapterRunner>().RunAsync(chapter, ct)
            ),
            "DelayBehavior" => new DelayBehavior(definition.GetFloat("Duration")),
            _ => throw new ArgumentException($"Unknown behavior type: {definition.TypeName}")
        };
    }
}
```

### 9.3 Scoping Strategy

VContainer scoping is relatively flat for this system since the engine manages its own lifecycle:

- **Root scope (RfeLifetimeScope):** All singletons. Lives for the scene's lifetime.
- **No per-process child scope needed initially** because only one process runs at a time. The `StateStore.Clear()` resets between runs.
- **Future optimization:** If multiple processes need to run (e.g., tutorial + main content), a per-process child scope with `Lifetime.Scoped` registrations for `StateStore` and `HistoryStack` would isolate them.

### 9.4 Entry Point

```csharp
public class ProcessRunner : IStartable, IAsyncStartable, IDisposable
{
    private readonly IProcessLoader _loader;
    private readonly IFlowEngine _engine;
    private readonly TextAsset _processJson;
    private CancellationTokenSource _cts;

    [Inject]
    public ProcessRunner(IProcessLoader loader, IFlowEngine engine, TextAsset processJson)
    {
        _loader = loader;
        _engine = engine;
        _processJson = processJson;
    }

    public async UniTask StartAsync(CancellationToken ct)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var process = await _loader.LoadAsync(_processJson.text, _cts.Token);
        await _engine.StartProcessAsync(process, _cts.Token);
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}
```

---

## 10. Implementation Phases

### Phase 1: Foundation
**Goal:** Install packages, define all interfaces, build the data model, parse the sample JSON.

1. **Install packages via UPM / OpenUPM:**
   - R3 (Cysharp/R3) via git URL or OpenUPM
   - UniTask (Cysharp/UniTask) via git URL or OpenUPM
   - VContainer (hadashiA/VContainer) via OpenUPM
   - Newtonsoft.Json for Unity (jilleJr/Newtonsoft.Json-for-Unity or com.unity.nuget.newtonsoft-json)

2. **Create assembly definitions:**
   - `ReactiveFlowEngine.asmdef` (references: R3, UniTask, VContainer, Newtonsoft.Json)
   - `ReactiveFlowEngine.Tests.asmdef` (references: main asmdef + Unity Test Framework)

3. **Implement all interfaces** (Section 3 above) -- pure C# files, no implementation bodies yet.

4. **Implement Model POCOs** (ProcessModel, ChapterModel, StepModel, TransitionModel, etc.)

5. **Implement JSON parsing:**
   - JSON DTO classes matching the VRBuilder JSON shape
   - `RfeTypeBinder` for `$type` resolution
   - `VRBuilderJsonLoader` using Newtonsoft with `PreserveReferencesHandling.All`
   - `ModelBuilder` to transform DTOs to domain models
   - Validation logic

6. **Write tests:**
   - Parse `BackButtonTestProcess2.json` and assert:
     - 1 chapter with 4 steps (Step 1-3 + Step Group)
     - 1 sub-chapter with 3 steps (Step 4-6)
     - Step 1 has 1 unconditional transition -> Step 2
     - Step 2 has 2 conditional transitions (2s timeout -> Step 3, 1s timeout -> Step Group)
     - Step 3 has 1 unconditional transition -> null (end)
     - Step Group has ExecuteChapterBehavior referencing sub-chapter
     - All MoveObjectBehavior data is correctly extracted

**Deliverable:** `IProcessLoader.LoadAsync(json)` returns a fully linked `IProcess` graph.

---

### Phase 2: Minimal Forward Execution
**Goal:** Execute the sample process from Step 1 to completion with forward-only flow.

1. **Implement `StepRunner`:**
   - Enter/Execute/Evaluate lifecycle
   - Blocking behavior execution (sequential)
   - Cancellation token plumbing

2. **Implement `TransitionEvaluator`:**
   - Handle unconditional transitions (immediate fire)
   - Handle TimeoutCondition via R3 `Observable.Timer`

3. **Implement `FlowEngine`:**
   - Main run loop: load process -> run first chapter -> iterate steps
   - Chapter completion detection (null TargetStep)

4. **Implement `TimeoutCondition`:**
   - R3-based timer observable

5. **Implement `MoveObjectBehavior`:**
   - Scene object resolution
   - Lerp with AnimationCurve over Duration
   - Frame-by-frame UniTask.Yield

6. **Implement `SceneObjectResolver`:**
   - Find GameObjects with matching GUIDs in scene
   - GUID component that tags scene objects

7. **Implement basic VContainer setup:**
   - `RfeLifetimeScope` with all registrations
   - `ProcessRunner` as entry point

8. **Write tests:**
   - Mock scene objects, verify step transitions in correct order
   - Verify TimeoutCondition fires after correct duration
   - Verify MoveObjectBehavior completes after Duration

**Deliverable:** Place the `RfeLifetimeScope` MonoBehaviour on a scene object, assign JSON, press Play, and watch the process execute steps forward to completion.

---

### Phase 3: Reverse Execution & Snapshots
**Goal:** Full backward navigation via PreviousStep.

1. **Implement `StateStore`:**
   - Snapshot capture after behaviors complete
   - History stack (push/pop)
   - Snapshot restoration

2. **Implement `IReversibleBehavior` on `MoveObjectBehavior`:**
   - Record original position in ExecuteAsync
   - Restore in UndoAsync

3. **Implement `NavigationService`:**
   - `NextStepAsync` (wraps normal forward flow with explicit trigger)
   - `PreviousStepAsync` (cancel -> undo -> restore -> resume)
   - `RestartStepAsync` (cancel -> undo -> re-enter same step)

4. **Implement `HistoryStack`:**
   - Thread-safe list-based stack
   - `CanGoBack` based on history depth > 0

5. **Wire navigation to UI:**
   - Simple debug buttons: Next, Previous, Restart
   - Display current step name

6. **Write tests:**
   - Forward Step 1 -> Step 2, then PreviousStep, verify back at Step 1
   - Verify MoveObjectBehavior undo restores original position
   - Verify snapshot state matches expected values

**Deliverable:** Forward and backward navigation works correctly with state restoration.

---

### Phase 4: Jump Navigation & Step Groups
**Goal:** GoToStep, JumpToChapter, and ExecuteChapterBehavior (sub-chapters).

1. **Implement `ChapterRunner`:**
   - Manages a chapter's step execution loop
   - Reusable for both main chapters and sub-chapters

2. **Implement `ExecuteChapterBehavior`:**
   - Blocking behavior that delegates to ChapterRunner
   - Undo: reverse all sub-chapter steps

3. **Implement `GoToStepAsync`:**
   - Path calculation (is target in history?)
   - Backward unwinding when target is behind
   - Forward jumping when target is ahead

4. **Implement `JumpToChapterAsync`:**
   - End current chapter
   - Start target chapter from its first step

5. **Handle Step Group edge cases:**
   - PreviousStep while inside a sub-chapter should go to previous sub-step
   - PreviousStep from sub-chapter Step 4 should go back to parent Step 2 (the step that triggered the group)
   - Nested step groups (future-proof but not in sample JSON)

6. **Write tests:**
   - GoTo Step 3 from Step 1 (forward jump, skipping Step 2)
   - GoTo Step 1 from Step 3 (backward unwinding)
   - Execute Step Group and verify sub-chapter runs Steps 4-5-6
   - PreviousStep from within sub-chapter

**Deliverable:** Full navigation including jumps and step group sub-chapters.

---

### Phase 5: Extended Conditions & Behaviors
**Goal:** Implement the full PRD condition and behavior catalog.

1. **Conditions:**
   - `InteractionCondition` (grab, use, touch) -- integrates with XR Interaction Toolkit
   - `SpatialZoneCondition` (object enters trigger zone)
   - `DistanceCondition` (object within distance of target)
   - `StateVariableCondition` (checks global state variable)
   - `CompositeAndCondition`, `CompositeOrCondition`, `CompositeNotCondition`

2. **Behaviors:**
   - `EnableObjectBehavior` (enable/disable GameObjects) -- reversible
   - `HighlightBehavior` (add/remove highlight material) -- reversible
   - `PlayAudioBehavior` (AudioSource.Play) -- reversible (stop)
   - `DelayBehavior` (UniTask.Delay) -- no undo needed
   - Extend `BehaviorFactory` and `ConditionFactory` with new types

3. **Update JSON type binder** to support new condition/behavior types.

4. **Write tests for each new condition and behavior.**

**Deliverable:** All PRD-specified conditions and behaviors are functional and reversible.

---

### Phase 6: Optimization & Polish
**Goal:** Meet non-functional requirements (90 FPS, low GC, scalability).

1. **GC Optimization:**
   - Pool `StepSnapshot` objects and their dictionaries
   - Use `ArrayPool<T>` for temporary collections in TransitionEvaluator
   - Avoid LINQ allocations in hot paths (replace with for-loops)
   - Profile R3 subscription allocations, use `DisposableBag` over `CompositeDisposable`

2. **Frame Budget:**
   - Ensure step transitions complete within 1 frame (no async gap between exit and enter)
   - Profile MoveObjectBehavior lerp loop
   - Use `PlayerLoopTiming.Update` consistently (not `PlayerLoopTiming.LastPostLateUpdate`)

3. **Scalability Testing:**
   - Create a stress test JSON with 100+ steps
   - Verify memory does not grow unbounded (snapshot pruning if needed)
   - Verify GoTo with large history doesn't hitch

4. **Error Handling:**
   - Graceful recovery from behavior exceptions (log + skip + continue)
   - Timeout on stuck conditions (configurable per-process)
   - Invalid JSON produces clear error messages

5. **Debug Tools:**
   - `RfeDebugUI`: overlay showing current step, history, snapshot count
   - Log integration: structured logging for step enter/exit/transition events
   - Optional: Editor window for process visualization

6. **IL2CPP Compatibility:**
   - Test with IL2CPP builds early
   - Add `link.xml` to preserve JSON DTO types from stripping

**Deliverable:** Production-ready system meeting all non-functional requirements.

---

## 11. Risks & Mitigations

### Risk 1: Async Race Conditions During Navigation

**Scenario:** User presses "Back" while a behavior's `ExecuteAsync` is mid-flight. The behavior writes to a scene object. Simultaneously, `UndoAsync` of the previous step also touches the same object.

**Mitigation:**
- Single-threaded execution guarantee via UniTask on main thread.
- Step-level `CancellationTokenSource` is cancelled BEFORE undo begins.
- Enforce sequential: Cancel -> await all tasks to acknowledge cancellation -> then undo.
- A `_isTransitioning` lock in `NavigationService` prevents overlapping navigation requests.

```csharp
// In NavigationService:
private readonly SemaphoreSlim _navigationLock = new(1, 1);

public async UniTask PreviousStepAsync(CancellationToken ct)
{
    if (!await _navigationLock.WaitAsync(0, ct))
    {
        Debug.LogWarning("Navigation already in progress, ignoring.");
        return;
    }
    try { /* ... actual navigation logic ... */ }
    finally { _navigationLock.Release(); }
}
```

### Risk 2: Invalid Transitions (Dangling StepRef GUIDs)

**Scenario:** JSON has a transition targeting a step GUID that does not exist in the flat steps list.

**Mitigation:**
- `ModelBuilder` validation catches this at load time.
- At runtime, `FlowEngine` null-checks the resolved step. If null, log error and treat as end-of-chapter.

### Risk 3: Snapshot Memory Growth

**Scenario:** Process with 100 steps, each snapshot stores behavior state.

**Mitigation:**
- Sliding window pruning: keep only last N snapshots (configurable, default 50).
- Structural sharing: copy-on-write for state dictionary.
- Lazy behavior state: only store what's needed for undo.

### Risk 4: Mid-Jump Step Group Complexity

**Scenario:** User is inside a step group (sub-chapter, Step 5) and issues `GoToStepAsync("step-1-guid")`.

**Mitigation:**
- `NavigationService.GoToStepAsync` implements hierarchical search:
  1. Is the target step in the current (sub-)chapter? Handle locally.
  2. If not, unwind the sub-chapter stack completely.
  3. Search the parent chapter for the target.
- History stack is augmented with chapter context:

```csharp
public class HistoryEntry
{
    public string ChapterId { get; init; }
    public string StepId { get; init; }
    public int Depth { get; init; }  // 0 = root chapter, 1 = sub-chapter, etc.
}
```

### Risk 5: R3 Observable Leaks

**Scenario:** Condition observables subscribed but never disposed.

**Mitigation:**
- All subscriptions go into `CompositeDisposable` owned by `StepRunner`.
- Disposed in `finally` block, guaranteeing cleanup on all exit paths.
- `ICondition.Dispose()` called explicitly after composite disposal.

### Risk 6: Newtonsoft.Json + IL2CPP

**Scenario:** Newtonsoft.Json uses reflection heavily. IL2CPP strips unused types.

**Mitigation:**
- Use `com.unity.nuget.newtonsoft-json` (IL2CPP-safe).
- Add `link.xml` to preserve JSON DTO types from stripping.
- Test with IL2CPP builds early (Phase 2).

### Risk 7: Frame Spikes During State Restoration

**Scenario:** Restoring a snapshot requires repositioning many objects.

**Mitigation:**
- `RestoreSnapshotAsync` is async, can spread work across frames if needed.
- Budget system: if restoration exceeds N operations, spread over M frames.

### Risk 8: Deterministic Replay Fidelity

**Scenario:** Floating-point differences in AnimationCurve evaluation.

**Mitigation:**
- Snapshots store FINAL state (positions after completion), not animation parameters.
- Restoration sets absolute positions, bypassing animation entirely.

---

## Data Flow Summary (End-to-End)

```
[BackButtonTestProcess2.json]
    |  VRBuilderJsonLoader.LoadAsync()
    |  Newtonsoft.Json + RfeTypeBinder + PreserveReferencesHandling
    v
[JsonProcessWrapper (DTO graph)]
    |  ModelBuilder.Build()
    |  Resolve $ref, map types, validate
    v
[ProcessModel (domain graph)]
    |  FlowEngine.StartProcessAsync()
    v
[Chapter 1 loop]
    |  StepRunner.RunStepAsync(Step 1)
    |    MoveObjectBehavior.ExecuteAsync()  -->  moves scene object
    |    TransitionEvaluator.Evaluate()     -->  unconditional -> Step 2
    |    StateStore.CaptureSnapshot(Step 1)
    v
[Step 2]
    |  StepRunner.RunStepAsync(Step 2)
    |    MoveObjectBehavior.ExecuteAsync()  -->  moves scene object
    |    TransitionEvaluator.Evaluate()     -->  TimeoutCondition(1s) -> Step Group
    |                                            TimeoutCondition(2s) -> Step 3
    |    (1s fires first -> Step Group wins)
    |    StateStore.CaptureSnapshot(Step 2)
    v
[Step Group]
    |  StepRunner.RunStepAsync(Step Group)
    |    ExecuteChapterBehavior.ExecuteAsync()
    |      ChapterRunner.RunAsync(Sub-Chapter "Step Group")
    |        StepRunner.RunStepAsync(Step 4) -> Step 5 -> Step 6 -> END
    |    TransitionEvaluator.Evaluate()  -->  unconditional -> null (END)
    |    StateStore.CaptureSnapshot(Step Group)
    v
[Chapter 1 complete -> Process complete]
```

---

## Verification Plan

1. **Phase 1:** Unit test that parses `BackButtonTestProcess2.json` and asserts correct graph structure
2. **Phase 2:** Play mode test - place scene objects with matching GUIDs, verify steps execute in order and objects move
3. **Phase 3:** Play mode test - forward 2 steps, press Back, verify object returns to original position
4. **Phase 4:** Play mode test - GoTo step by name, verify correct unwinding/jumping
5. **Phase 5:** Unit tests for each condition/behavior type
6. **Phase 6:** VR build test on device at 90 FPS; profiler verification of GC and frame time

---

## Critical Files

- `Docs/BackButtonTestProcess2.json` - Reference JSON defining the data model
- `Packages/manifest.json` - Must add R3, UniTask, VContainer, Newtonsoft.Json
- `Assets/` - Target directory for all new code (greenfield)
- `Docs/Reactive_Flow_Engine_PRD.docx` - Feature specification for Phase 5
- `ProjectSettings/ProjectSettings.asset` - May need XR plugin and IL2CPP configuration
