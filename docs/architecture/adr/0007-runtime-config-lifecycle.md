# 0007 Runtime Config Lifecycle

- Status: Accepted
- Date: 2026-05-11

## Context

Runtime-config objects could enter services directly from binary stores or ScriptableObject sources.
Without a strict lifecycle, invalid values or mutable references can produce divergent behavior across clients and scenes.

## Decision

Adopt unified lifecycle for runtime-configs:

1. Load from source.
2. Validate and normalize data.
3. Freeze into immutable runtime snapshot used by services.

Initial rollout covers Multiplayer and Calendar config pipelines.

## Consequences

Positive:
- Deterministic runtime behavior from validated snapshots.
- Lower risk of config drift during active session.
- Clear extension point for future config sources.

Trade-offs:
- Extra lifecycle code per config domain.
- Slight startup overhead for normalization/reconstruction.

## Rollback / Alternative

Rollback:
- Revert lifecycle usage and pass raw loaded configs directly to services.

Alternative:
- Introduce a shared generic lifecycle framework with strict schema validation and error aggregation.

## Links

- `Assets/Moyva/Scripts/Features/Multiplayer/API/MultiplayerConfigLifecycle.cs`
- `Assets/Moyva/Scripts/Features/Calendar/API/CalendarConfigLifecycle.cs`
- `docs/standarts/runtime-config-lifecycle.md`