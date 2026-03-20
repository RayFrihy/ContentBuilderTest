# ReactiveFlowEngine Development Standards

## Architecture

- **Single history mechanism**: Use `IHistoryService` (backed by `HistoryStack`). Do not add history tracking to `StateStore` or create new history systems.
- **No `internal` access modifiers** on engine types. Use interfaces (`IEngineController`, `IFlowEngine`, `INavigationService`) for cross-component communication.
- **NavigationService** must not depend on concrete `FlowEngine`. It uses `IFlowEngine` + `IEngineController`.
- **ChapterRunner** is injected via DI, not manually created with `new`.

## Code Patterns

- **No `await UniTask.CompletedTask`**: For synchronous methods returning `UniTask`, use `return UniTask.CompletedTask;` directly (non-async).
- **All resolver/target resolution failures MUST log warnings**: Use `Debug.LogWarning($"[RFE] {ClassName}: ...")` when `_resolver` is null or target object is not found.
- **No unused `_subscription` fields**: If a condition doesn't manage its own subscription, don't declare the field.
- **Consolidated behaviors**: Use `SetActiveBehavior` (not Enable/DisableObjectBehavior) and `SetRendererVisibilityBehavior` (not Show/HideObjectBehavior).
- **Consolidated conditions**: Use `EventBusCondition` for simple event-bus-matching conditions (ButtonPressed, ButtonReleased, ObjectTouched, ObjectUsed, ObjectSelected, ObjectDeselected, ObjectReleased, InputActionTriggered).

## Serialization

- All new behaviors/conditions MUST have:
  1. A corresponding factory case in `BehaviorFactory.Create()` or `ConditionFactory.Create()`
  2. A type mapping in `RfeTypeBinder` (map to `JsonGenericBehavior` or `JsonGenericCondition` for new types)
  3. The `ModelBuilder` generic fallback handles all types mapped to `JsonGenericBehavior`/`JsonGenericCondition` automatically

## Testing

- All new code MUST include unit tests in `Assets/ReactiveFlowEngine.Tests/`
- Tests run in Unity EditMode (no Play mode dependencies)
- Use test doubles from `TestDoubles/` (MockSceneObjectResolver, MockEventBus, MockStateStore, MockStepRunner, TestCondition, TestBehavior, TestProcessBuilder)

## DI Registration (RfeLifetimeScope)

- `FlowEngine` registered as `IFlowEngine` + `IEngineController` + `AsSelf()`
- `NavigationService` registered as `INavigationService` + `AsSelf()`
- `HistoryStack` registered as `IHistoryService` (singleton)
- `StateStore` no longer has history methods (use `IHistoryService` instead)

## Build & CI

- CI runs via GitHub Actions (`.github/workflows/test.yml`)
- Requires `UNITY_LICENSE` secret configured in repository settings
- Tests run in editmode with Unity 6000.2.13f1
